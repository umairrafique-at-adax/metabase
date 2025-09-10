using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MetabaseMigrator.Console.Models
{
    public class Database
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("engine")]
        public string? Engine { get; set; }
    }

}
