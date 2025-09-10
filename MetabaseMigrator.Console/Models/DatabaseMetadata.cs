using System.Text.Json.Serialization;

namespace MetabaseMigrator.Console.Models
{
    public class DatabaseMetadata
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("engine")]
        public string? Engine { get; set; }

        [JsonPropertyName("tables")]
        public List<TableDto> Tables { get; set; } = new();
    }

}
