using System.Text.Json.Serialization;

namespace MetabaseMigrator.Console.Models
{
    public class TableDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("db_id")]
        public int DatabaseId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("schema")]
        public string? Schema { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("fields")]
        public List<FieldDto> Fields { get; set; } = new();
    }

}
