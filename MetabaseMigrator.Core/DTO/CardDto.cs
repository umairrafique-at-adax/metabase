using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MetabaseMigrator.Core.DTO
{
    public class CardDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("collection_id")]
        public int? CollectionId { get; set; }

        [JsonProperty("database_id")]
        public int? DatabaseId { get; set; }

        [JsonProperty("dataset_query")]
        public JsonElement DatasetQuery { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }
}
