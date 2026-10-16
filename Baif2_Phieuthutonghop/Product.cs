namespace Bai2_QuanLyKho;
public class Product
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImagePath { get; set; } = "";
    public decimal TotalValue => Price * Quantity;
}