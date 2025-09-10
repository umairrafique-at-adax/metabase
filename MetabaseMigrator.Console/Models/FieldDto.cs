using System.Text.Json.Serialization;

namespace MetabaseMigrator.Console.Models
{
    public class FieldDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("table_id")]
        public int TableId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("base_type")]
        public string BaseType { get; set; } = "";

        [JsonPropertyName("semantic_type")]
        public string? SemanticType { get; set; }
    }

}
