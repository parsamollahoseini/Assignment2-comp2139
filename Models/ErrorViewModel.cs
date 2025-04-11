namespace SmartInventory.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; } // Add this property

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
