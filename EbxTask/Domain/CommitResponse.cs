using System.Text.Json.Serialization;

public class CommitResponse
{
    [JsonPropertyName("author")]
    public CommitAuthor Author { get; set; }

    [JsonPropertyName("committer")]
    public CommitAuthor Committer { get; set; }
}