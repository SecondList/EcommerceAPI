using System.Collections;

namespace EcommerceAPI.Dto
{
    public class BaseResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string[] Errors { get; set; } = Array.Empty<string>();
        public object Result { get; set; } = null!;
        public int ResultCount { get; set; } = 0;
        public int Page { get; set; } = 0;
        public int PageCount { get; set; } = 0;
        public int PageSize { get; set; } = 0;
    }
}
