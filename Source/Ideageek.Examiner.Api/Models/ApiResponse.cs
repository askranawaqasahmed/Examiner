using System.Text.Json.Serialization;

namespace Ideageek.Examiner.Api.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public T? Value { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
