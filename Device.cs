namespace DeviceManager;

public class Device
{
    public string DeviceId { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
