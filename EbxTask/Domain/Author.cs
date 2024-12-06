using System.Text.Json.Serialization;

public class CommitAuthor
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
}