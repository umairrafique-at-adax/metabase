using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MetabaseMigrator.Console.DTO
{
    public class DashboardUpdatePayloadDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("collection_id")]
        public int? CollectionId { get; set; }

        [JsonProperty("tabs")]
        public List<DashboardTabPostDto> Tabs { get; set; }

        [JsonProperty("dashcards")]
        public List<DashboardCardPostDto> Dashcards { get; set; }

        [JsonProperty("parameters")]
        public List<DashboardParameterPostDto> Parameters { get; set; }
    }

    public class DashboardCardPostDto
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("card_id")]
        public int? CardId { get; set; }

        [JsonProperty("dashboard_tab_id")]
        public int? DashboardTabId { get; set; }

        [JsonProperty("row")]
        public int Row { get; set; }

        [JsonProperty("col")]
        public int Col { get; set; }

        [JsonProperty("size_x")]
        public int SizeX { get; set; }

        [JsonProperty("size_y")]
        public int SizeY { get; set; }

        [JsonProperty("parameter_mappings")]
        public List<ParameterMappingPostDto> ParameterMappings { get; set; }

        [JsonProperty("visualization_settings")]
        public Dictionary<string, object> VisualizationSettings { get; set; }
    }

    public class ParameterMappingPostDto
    {
        [JsonProperty("parameter_id")]
        public string ParameterId { get; set; }

        [JsonProperty("card_id")]
        public int? CardId { get; set; }

        [JsonProperty("target")]
        public object Target { get; set; }
    }

    public class DashboardTabPostDto
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }
    }

    public class DashboardParameterPostDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("sectionId")]
        public string SectionId { get; set; }

        [JsonProperty("isMultiSelect", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMultiSelect { get; set; }

        [JsonProperty("values_source_type", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValuesSourceType { get; set; }

        [JsonProperty("values_source_config", NullValueHandling = NullValueHandling.Ignore)]
        public object? ValuesSourceConfig { get; set; }

        [JsonProperty("temporal_units", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? TemporalUnits { get; set; }
    }

}
