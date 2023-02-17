using System.Collections;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Dto
{
    public class BaseResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Message { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Status { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection Errors { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Result { get; set; } = null!;
        public int ResultCount { get; set; } = 0;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Page { get; set; } = 0;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int PageCount { get; set; } = 0;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int PageSize { get; set; } = 0;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int TotalCount { get; set; } = 0;
    }
}
