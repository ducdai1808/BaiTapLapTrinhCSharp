namespace Bai2_QuanLyKho;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new frmInventoryManager());
    }
}