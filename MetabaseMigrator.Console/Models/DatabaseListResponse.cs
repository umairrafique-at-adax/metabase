using System.Text.Json.Serialization;

namespace MetabaseMigrator.Console.Models
{
    public class DatabaseListResponse
    {
        [JsonPropertyName("data")]
        public List<Database> Data { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

}
