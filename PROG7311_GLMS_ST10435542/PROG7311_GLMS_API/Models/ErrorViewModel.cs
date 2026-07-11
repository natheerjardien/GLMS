namespace PROG7311_GLMS_API.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool? ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
