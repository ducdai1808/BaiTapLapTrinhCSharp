using System.ComponentModel;
using System.Globalization;

namespace DeviceManager;

public class frmDeviceManager : Form
{
    private readonly BindingList<Device> devices = new();
    private readonly BindingSource bindingSource = new();
    private readonly ErrorProvider errorProvider = new();

    private readonly TextBox txtDeviceId = new();
    private readonly TextBox txtDeviceName = new();
    private readonly ComboBox cboCategory = new();
    private readonly TextBox txtPrice = new();
    private readonly NumericUpDown numQuantity = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnClear = new();
    private readonly DataGridView dgvDevices = new();

    private int nextId = 1;

    public frmDeviceManager()
    {
        Text = "QUẢN LÝ DANH MỤC THIẾT BỊ ĐIỆN TỬ";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1050, 720);
        Font = new Font("Segoe UI", 10F);
        BuildInterface();
        ConfigureGrid();
        bindingSource.DataSource = devices;
        dgvDevices.DataSource = bindingSource;

        btnAdd.Click += btnAdd_Click;
        btnUpdate.Click += btnUpdate_Click;
        btnDelete.Click += btnDelete_Click;
        btnClear.Click += (_, _) => ClearInput();
        dgvDevices.SelectionChanged += dgvDevices_SelectionChanged;
    }

    private void BuildInterface()
    {
        var grpInput = new GroupBox
        {
            Name = "grpInput",
            Text = "THÔNG TIN THIẾT BỊ",
            Location = new Point(15, 15),
            Size = new Size(990, 245),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(grpInput);

        AddLabel(grpInput, "Mã thiết bị:", 25, 35);
        txtDeviceId.Location = new Point(150, 32);
        txtDeviceId.Size = new Size(300, 28);
        txtDeviceId.ReadOnly = true;
        grpInput.Controls.Add(txtDeviceId);

        AddLabel(grpInput, "Tên thiết bị:", 490, 35);
        txtDeviceName.Location = new Point(615, 32);
        txtDeviceName.Size = new Size(320, 28);
        grpInput.Controls.Add(txtDeviceName);

        AddLabel(grpInput, "Loại thiết bị:", 25, 80);
        cboCategory.Location = new Point(150, 77);
        cboCategory.Size = new Size(300, 28);
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.Items.AddRange(new object[]
        {
            "Máy tính", "Điện thoại", "Thiết bị mạng",
            "Thiết bị gia dụng", "Phụ kiện"
        });
        grpInput.Controls.Add(cboCategory);

        AddLabel(grpInput, "Đơn giá:", 490, 80);
        txtPrice.Location = new Point(615, 77);
        txtPrice.Size = new Size(320, 28);
        grpInput.Controls.Add(txtPrice);

        AddLabel(grpInput, "Số lượng tồn:", 25, 125);
        numQuantity.Location = new Point(150, 122);
        numQuantity.Size = new Size(300, 28);
        numQuantity.Minimum = 0;
        numQuantity.Maximum = 1_000_000;
        grpInput.Controls.Add(numQuantity);

        ConfigureButton(btnAdd, "Thêm mới", "btnAdd", 15);
        ConfigureButton(btnUpdate, "Cập nhật", "btnUpdate", 155);
        ConfigureButton(btnDelete, "Xóa", "btnDelete", 295);
        ConfigureButton(btnClear, "Làm mới", "btnClear", 435);

        var lblListTitle = new Label
        {
            Name = "lblListTitle",
            Text = "DANH SÁCH THIẾT BỊ",
            Location = new Point(18, 320),
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        Controls.Add(lblListTitle);

        dgvDevices.Name = "dgvDevices";
        dgvDevices.Location = new Point(15, 350);
        dgvDevices.Size = new Size(990, 315);
        dgvDevices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                            AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(dgvDevices);

        errorProvider.ContainerControl = this;
        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(120, 28),
            TextAlign = ContentAlignment.MiddleLeft
        });
    }

    private void ConfigureButton(Button button, string text, string name, int x)
    {
        button.Name = name;
        button.Text = text;
        button.Location = new Point(x, 275);
        button.Size = new Size(130, 38);
        Controls.Add(button);
    }

    private void ConfigureGrid()
    {
        dgvDevices.AutoGenerateColumns = false;
        dgvDevices.Columns.Clear();
        dgvDevices.ReadOnly = true;
        dgvDevices.AllowUserToAddRows = false;
        dgvDevices.AllowUserToDeleteRows = false;
        dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDevices.MultiSelect = false;
        dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDevices.RowHeadersVisible = false;

        dgvDevices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDeviceId", HeaderText = "Mã thiết bị",
            DataPropertyName = nameof(Device.DeviceId), FillWeight = 15
        });
        dgvDevices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDeviceName", HeaderText = "Tên thiết bị",
            DataPropertyName = nameof(Device.DeviceName), FillWeight = 30
        });
        dgvDevices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCategory", HeaderText = "Loại thiết bị",
            DataPropertyName = nameof(Device.Category), FillWeight = 22
        });
        dgvDevices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPrice", HeaderText = "Đơn giá",
            DataPropertyName = nameof(Device.Price), FillWeight = 18,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N0",
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
        dgvDevices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colQuantity", HeaderText = "Số lượng tồn",
            DataPropertyName = nameof(Device.Quantity), FillWeight = 15
        });
    }

    private bool ValidateInput()
    {
        errorProvider.Clear();
        bool valid = true;

        if (string.IsNullOrWhiteSpace(txtDeviceName.Text))
        {
            errorProvider.SetError(txtDeviceName, "Tên thiết bị không được để trống.");
            valid = false;
        }

        if (cboCategory.SelectedIndex < 0)
        {
            errorProvider.SetError(cboCategory, "Vui lòng chọn loại thiết bị.");
            valid = false;
        }

        if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number,
                CultureInfo.CurrentCulture, out decimal price) || price < 0)
        {
            errorProvider.SetError(txtPrice, "Đơn giá phải là số hợp lệ và không âm.");
            valid = false;
        }

        return valid;
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput()) return;

        decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number,
            CultureInfo.CurrentCulture, out decimal price);

        devices.Add(new Device
        {
            DeviceId = $"TB{nextId:D3}",
            DeviceName = txtDeviceName.Text.Trim(),
            Category = cboCategory.Text,
            Price = price,
            Quantity = (int)numQuantity.Value
        });

        nextId++;
        bindingSource.ResetBindings(false);
        ClearInput();
        MessageBox.Show("Thêm thiết bị thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (dgvDevices.CurrentRow?.DataBoundItem is not Device device)
        {
            MessageBox.Show("Vui lòng chọn thiết bị cần cập nhật.");
            return;
        }

        if (!ValidateInput()) return;

        decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number,
            CultureInfo.CurrentCulture, out decimal price);

        device.DeviceName = txtDeviceName.Text.Trim();
        device.Category = cboCategory.Text;
        device.Price = price;
        device.Quantity = (int)numQuantity.Value;

        bindingSource.ResetBindings(false);
        MessageBox.Show("Cập nhật thiết bị thành công!", "Thông báo");
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvDevices.CurrentRow?.DataBoundItem is not Device device)
        {
            MessageBox.Show("Vui lòng chọn thiết bị cần xóa.");
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa thiết bị {device.DeviceName}?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            devices.Remove(device);
            bindingSource.ResetBindings(false);
            ClearInput();
        }
    }

    private void dgvDevices_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvDevices.CurrentRow?.DataBoundItem is not Device device) return;

        txtDeviceId.Text = device.DeviceId;
        txtDeviceName.Text = device.DeviceName;
        cboCategory.SelectedItem = device.Category;
        txtPrice.Text = device.Price.ToString("N0", CultureInfo.CurrentCulture);
        numQuantity.Value = Math.Clamp(device.Quantity,
            (int)numQuantity.Minimum, (int)numQuantity.Maximum);
        errorProvider.Clear();
    }

    private void ClearInput()
    {
        dgvDevices.ClearSelection();
        txtDeviceId.Clear();
        txtDeviceName.Clear();
        cboCategory.SelectedIndex = -1;
        txtPrice.Clear();
        numQuantity.Value = 0;
        errorProvider.Clear();
        txtDeviceName.Focus();
    }
}
