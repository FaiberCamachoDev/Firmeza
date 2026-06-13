namespace Firmeza.Api.DTOs.Sales;

public class SaleDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<SaleDetailDto> Details { get; set; } = [];
}
