namespace ultraReader.Models.DTOs
{
    public class ExtendedErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 