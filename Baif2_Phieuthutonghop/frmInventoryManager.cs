using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Bai2_QuanLyKho;

public class frmInventoryManager : Form
{
    readonly BindingList<Product> products = new();
    readonly BindingSource source = new();
    readonly ErrorProvider errors = new();
    readonly TextBox id = new(), nameBox = new(), priceBox = new(), imagePath = new();
    readonly ComboBox category = new();
    readonly NumericUpDown quantity = new();
    readonly PictureBox picture = new();
    readonly DataGridView grid = new();
    readonly ToolStripStatusLabel countStatus = new("Tổng số lượng: 0"), valueStatus = new("Tổng giá trị kho: 0 đ"), timeStatus = new("");
    readonly System.Windows.Forms.Timer timer = new();
    int nextId = 1;

    public frmInventoryManager()
    {
        Text = "BÀI 2 - QUẢN LÝ KHO VÀ XUẤT BÁO CÁO TỔNG HỢP";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1200, 800);
        MinimumSize = new Size(1000, 650);
        errors.ContainerControl = this;

        var menu = new MenuStrip();
        var system = new ToolStripMenuItem("System");
        system.DropDownItems.Add("Open File", null, (_,_) => OpenCsv());
        system.DropDownItems.Add("Save Report", null, (_,_) => ExportCsv());
        system.DropDownItems.Add("Exit", null, (_,_) => Close());
        var tools = new ToolStripMenuItem("Tools");
        tools.DropDownItems.Add("Import CSV", null, (_,_) => OpenCsv());
        tools.DropDownItems.Add("Export CSV", null, (_,_) => ExportCsv());
        menu.Items.AddRange(new ToolStripItem[] { system, tools });

        var tool = new ToolStrip();
        tool.Items.Add(Button("📂 Mở file", (_,_) => OpenCsv()));
        tool.Items.Add(Button("⬇ Nhập CSV", (_,_) => OpenCsv()));
        tool.Items.Add(Button("⬆ Xuất CSV", (_,_) => ExportCsv()));
        tool.Items.Add(new ToolStripSeparator());
        tool.Items.Add(Button("Thoát", (_,_) => Close()));

        var status = new StatusStrip();
        status.Items.Add(countStatus);
        status.Items.Add(new ToolStripStatusLabel(" | "));
        status.Items.Add(valueStatus);
        status.Items.Add(new ToolStripStatusLabel { Spring = true });
        status.Items.Add(timeStatus);

        var heading = new Label { Text = "QUẢN LÝ KHO VÀ XUẤT BÁO CÁO TỔNG HỢP", Dock = DockStyle.Top, Height = 38, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.Navy };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(8) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        for(int i=0;i<4;i++) fields.RowStyles.Add(new RowStyle(SizeType.Percent,25));
        id.ReadOnly = true;
        category.DropDownStyle = ComboBoxStyle.DropDownList;
        category.Items.AddRange(new object[]{"Máy tính","Điện thoại","Thiết bị mạng","Thiết bị gia dụng","Phụ kiện","Khác"});
        quantity.Maximum = 1000000;
        imagePath.ReadOnly = true;
        Field(fields,"Mã sản phẩm:",id,0,0);
        Field(fields,"Tên sản phẩm:",nameBox,2,0);
        Field(fields,"Loại sản phẩm:",category,0,1);
        Field(fields,"Đơn giá:",priceBox,2,1);
        Field(fields,"Số lượng:",quantity,0,2);
        Field(fields,"Đường dẫn ảnh:",imagePath,2,2);
        picture.Size = new Size(125,90); picture.SizeMode = PictureBoxSizeMode.Zoom; picture.BorderStyle = BorderStyle.FixedSingle;
        var imageRow = new FlowLayoutPanel { Dock=DockStyle.Fill, WrapContents=false };
        imageRow.Controls.Add(picture);
        var choose = new Button { Text="Chọn ảnh", AutoSize=true, Margin=new Padding(8,25,0,0) };
        choose.Click += (_,_) => ChooseImage();
        imageRow.Controls.Add(choose);
        fields.Controls.Add(new Label{Text="Ảnh sản phẩm:",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft},0,3);
        fields.Controls.Add(imageRow,1,3); fields.SetColumnSpan(imageRow,3);
        var inputGroup = new GroupBox { Text="Thông tin sản phẩm", Dock=DockStyle.Fill, Padding=new Padding(10) };
        inputGroup.Controls.Add(fields);

        var actions = new FlowLayoutPanel { Dock=DockStyle.Fill, Height=45, Padding=new Padding(0,5,0,0) };
        actions.Controls.Add(ActionButton("Thêm mới", (_,_) => AddProduct()));
        actions.Controls.Add(ActionButton("Cập nhật", (_,_) => UpdateProduct()));
        actions.Controls.Add(ActionButton("Xóa", (_,_) => DeleteProduct()));
        actions.Controls.Add(ActionButton("Làm mới", (_,_) => ClearFields()));

        grid.Dock=DockStyle.Fill; grid.ReadOnly=true; grid.AllowUserToAddRows=false; grid.AllowUserToDeleteRows=false;
        grid.MultiSelect=false; grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; grid.RowHeadersVisible=false;
        AddCol("ProductId","Mã sản phẩm",12); AddCol("ProductName","Tên sản phẩm",22); AddCol("Category","Loại",15);
        AddCol("Price","Đơn giá",14,"N0"); AddCol("Quantity","Số lượng",10); AddCol("TotalValue","Thành tiền",15,"N0"); AddCol("ImagePath","Đường dẫn ảnh",25);
        grid.SelectionChanged += (_,_) => LoadSelected();
        var listGroup = new GroupBox { Text="Danh sách sản phẩm trong kho", Dock=DockStyle.Fill, Padding=new Padding(8) }; listGroup.Controls.Add(grid);
        var layout = new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=1, RowCount=3, Padding=new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,225)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48)); layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        layout.Controls.Add(inputGroup,0,0); layout.Controls.Add(actions,0,1); layout.Controls.Add(listGroup,0,2);
        Controls.Add(layout); Controls.Add(heading); Controls.Add(tool); Controls.Add(menu); Controls.Add(status);
        status.Dock=DockStyle.Bottom; menu.Dock=DockStyle.Top; tool.Dock=DockStyle.Top;
        source.DataSource=products; grid.DataSource=source;
        products.ListChanged += (_,_) => UpdateStatus(); UpdateStatus();
        timer.Interval=1000; timer.Tick += (_,_) => timeStatus.Text="Thời gian: "+DateTime.Now.ToString("HH:mm:ss"); timer.Start();
        FormClosing += (_,e) => { if(MessageBox.Show(this,"Bạn có chắc chắn muốn thoát chương trình?","Xác nhận thoát",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.No)e.Cancel=true; };
    }

    ToolStripButton Button(string text, EventHandler click) { var b=new ToolStripButton(text); b.Click+=click; return b; }
    static Button ActionButton(string text, EventHandler click) { var b=new Button{Text=text,Width=115,Height=34,Margin=new Padding(4)}; b.Click+=click; return b; }
    static void Field(TableLayoutPanel p,string label,Control c,int col,int row) { p.Controls.Add(new Label{Text=label,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft},col,row); c.Dock=DockStyle.Fill;c.Margin=new Padding(3,7,10,7);p.Controls.Add(c,col+1,row); }
    void AddCol(string prop,string title,float weight,string? format=null) { var c=new DataGridViewTextBoxColumn{DataPropertyName=prop,HeaderText=title,FillWeight=weight}; if(format!=null)c.DefaultCellStyle.Format=format;grid.Columns.Add(c); }

    void ChooseImage() {
        using var d=new OpenFileDialog{Title="Chọn ảnh sản phẩm",Filter="Ảnh (*.jpg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"};
        if(d.ShowDialog(this)==DialogResult.OK){imagePath.Text=d.FileName;ShowImage(d.FileName);}
    }
    void ShowImage(string path) {
        try { picture.Image?.Dispose(); picture.Image=null; if(File.Exists(path)){using var fs=File.OpenRead(path);using var img=Image.FromStream(fs);picture.Image=new Bitmap(img);} } catch { picture.Image=null; }
    }
    bool ValidateInput() {
        errors.Clear(); bool ok=true;
        if(string.IsNullOrWhiteSpace(nameBox.Text)){errors.SetError(nameBox,"Tên sản phẩm không được để trống.");ok=false;}
        if(category.SelectedIndex<0){errors.SetError(category,"Hãy chọn loại sản phẩm.");ok=false;}
        if(!decimal.TryParse(priceBox.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var price)||price<0){errors.SetError(priceBox,"Đơn giá phải là số hợp lệ và không âm.");ok=false;}
        return ok;
    }
    Product FromFields(string productId)=>new(){ProductId=productId,ProductName=nameBox.Text.Trim(),Category=category.Text,Price=decimal.Parse(priceBox.Text,NumberStyles.Number,CultureInfo.CurrentCulture),Quantity=(int)quantity.Value,ImagePath=imagePath.Text.Trim()};
    void AddProduct(){if(!ValidateInput())return;products.Add(FromFields("SP"+(nextId++).ToString("D3")));ClearFields();}
    void UpdateProduct(){if(grid.CurrentRow?.DataBoundItem is not Product p){MessageBox.Show("Vui lòng chọn sản phẩm cần cập nhật.");return;}if(!ValidateInput())return;var n=FromFields(p.ProductId);p.ProductName=n.ProductName;p.Category=n.Category;p.Price=n.Price;p.Quantity=n.Quantity;p.ImagePath=n.ImagePath;source.ResetBindings(false);}
    void DeleteProduct(){if(grid.CurrentRow?.DataBoundItem is not Product p){MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.");return;}if(MessageBox.Show($"Xóa sản phẩm {p.ProductName}?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes){products.Remove(p);ClearFields();}}
    void ClearFields(){grid.ClearSelection();id.Clear();nameBox.Clear();category.SelectedIndex=-1;priceBox.Clear();quantity.Value=0;imagePath.Clear();picture.Image?.Dispose();picture.Image=null;errors.Clear();nameBox.Focus();}
    void LoadSelected(){if(grid.CurrentRow?.DataBoundItem is not Product p)return;id.Text=p.ProductId;nameBox.Text=p.ProductName;category.SelectedItem=p.Category;priceBox.Text=p.Price.ToString(CultureInfo.CurrentCulture);quantity.Value=p.Quantity;imagePath.Text=p.ImagePath;ShowImage(p.ImagePath);}
    void UpdateStatus(){countStatus.Text="Tổng số lượng: "+products.Sum(p=>p.Quantity).ToString("N0");valueStatus.Text="Tổng giá trị kho: "+products.Sum(p=>p.TotalValue).ToString("N0")+" đ";}
    static string Csv(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
    void ExportCsv(){
        using var d=new SaveFileDialog{Title="Xuất báo cáo kho",Filter="CSV (*.csv)|*.csv",DefaultExt="csv",FileName="BaoCaoKho.csv"};
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        try{using var w=new StreamWriter(d.FileName,false,new UTF8Encoding(true));w.WriteLine("ProductId,ProductName,Category,Price,Quantity,TotalValue,ImagePath");foreach(var p in products)w.WriteLine(string.Join(",",Csv(p.ProductId),Csv(p.ProductName),Csv(p.Category),p.Price.ToString(CultureInfo.InvariantCulture),p.Quantity,p.TotalValue.ToString(CultureInfo.InvariantCulture),Csv(p.ImagePath)));MessageBox.Show("Xuất CSV thành công.");}
        catch(Exception ex){MessageBox.Show("Lỗi xuất CSV: "+ex.Message);}
    }
    void OpenCsv(){
        using var d=new OpenFileDialog{Title="Nhập dữ liệu CSV",Filter="CSV (*.csv)|*.csv"};
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        try{var imported=new List<Product>();foreach(var line in File.ReadAllLines(d.FileName,Encoding.UTF8).Skip(1)){if(string.IsNullOrWhiteSpace(line))continue;var f=ParseCsv(line);if(f.Count<7||!decimal.TryParse(f[3],NumberStyles.Number,CultureInfo.InvariantCulture,out var price)||!int.TryParse(f[4],out var qty))continue;imported.Add(new Product{ProductId=f[0],ProductName=f[1],Category=f[2],Price=price,Quantity=Math.Max(0,qty),ImagePath=f[6]});}products.Clear();foreach(var p in imported)products.Add(p);nextId=products.Select(p=>int.TryParse(new string(p.ProductId.Where(char.IsDigit).ToArray()),out var n)?n:0).DefaultIfEmpty(0).Max()+1;source.ResetBindings(false);UpdateStatus();MessageBox.Show($"Đã nhập {imported.Count} sản phẩm.");}
        catch(Exception ex){MessageBox.Show("Lỗi nhập CSV: "+ex.Message);}
    }
    static List<string> ParseCsv(string line){var result=new List<string>();var s=new StringBuilder();bool q=false;for(int i=0;i<line.Length;i++){char c=line[i];if(c=='"'){if(q&&i+1<line.Length&&line[i+1]=='"'){s.Append('"');i++;}else q=!q;}else if(c==','&&!q){result.Add(s.ToString());s.Clear();}else s.Append(c);}result.Add(s.ToString());return result;}
}