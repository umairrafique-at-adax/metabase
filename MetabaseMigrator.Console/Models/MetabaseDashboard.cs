using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetabaseMigrator.Console.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class MetabaseDashboard
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("collection_position")]
        public int? CollectionPosition { get; set; }

        [JsonProperty("dashcards")]
        public List<DashboardCard> Dashcards { get; set; }

        [JsonProperty("initially_published_at")]
        public DateTime? InitiallyPublishedAt { get; set; }

        [JsonProperty("can_write")]
        public bool CanWrite { get; set; }

        [JsonProperty("tabs")]
        public List<DashboardTab> Tabs { get; set; }

        [JsonProperty("enable_embedding")]
        public bool EnableEmbedding { get; set; }

        [JsonProperty("collection_id")]
        public int? CollectionId { get; set; }

        [JsonProperty("show_in_getting_started")]
        public bool ShowInGettingStarted { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("caveats")]
        public string Caveats { get; set; }

        [JsonProperty("collection_authority_level")]
        public string CollectionAuthorityLevel { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("can_restore")]
        public bool CanRestore { get; set; }

        [JsonProperty("moderation_reviews")]
        public List<object> ModerationReviews { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("made_public_by_id")]
        public int? MadePublicById { get; set; }

        [JsonProperty("embedding_params")]
        public object EmbeddingParams { get; set; }

        [JsonProperty("cache_ttl")]
        public int? CacheTtl { get; set; }

        [JsonProperty("last_used_param_values")]
        public Dictionary<string, object> LastUsedParamValues { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("last_viewed_at")]
        public DateTime LastViewedAt { get; set; }

        [JsonProperty("position")]
        public int? Position { get; set; }

        [JsonProperty("archived_directly")]
        public bool ArchivedDirectly { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("param_fields")]
        public Dictionary<string, List<ParamField>> ParamFields { get; set; }

        [JsonProperty("last-edit-info")]
        public LastEditInfo LastEditInfo { get; set; }

        [JsonProperty("collection")]
        public DashboardCollection Collection { get; set; }

        [JsonProperty("parameters")]
        public List<DashboardParameter> Parameters { get; set; }

        [JsonProperty("auto_apply_filters")]
        public bool? AutoApplyFilters { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("public_uuid")]
        public string PublicUuid { get; set; }

        [JsonProperty("points_of_interest")]
        public string PointsOfInterest { get; set; }

        [JsonProperty("can_delete")]
        public bool CanDelete { get; set; }
    }

    public class DashboardCard
    {
        [JsonProperty("size_x")]
        public int SizeX { get; set; }

        [JsonProperty("dashboard_tab_id")]
        public int? DashboardTabId { get; set; }

        [JsonProperty("series")]
        public List<object> Series { get; set; }

        [JsonProperty("action_id")]
        public int? ActionId { get; set; }

        [JsonProperty("collection_authority_level")]
        public string CollectionAuthorityLevel { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("col")]
        public int Col { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("parameter_mappings")]
        public List<ParameterMapping> ParameterMappings { get; set; }

        [JsonProperty("card_id")]
        public int? CardId { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("visualization_settings")]
        public Dictionary<string, object> VisualizationSettings { get; set; }

        [JsonProperty("size_y")]
        public int SizeY { get; set; }

        [JsonProperty("dashboard_id")]
        public int DashboardId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("row")]
        public int Row { get; set; }
    }

    public class Card
    {
        [JsonProperty("cache_invalidated_at")]
        public DateTime? CacheInvalidatedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("collection_position")]
        public int? CollectionPosition { get; set; }

        [JsonProperty("source_card_id")]
        public int? SourceCardId { get; set; }

        [JsonProperty("table_id")]
        public int? TableId { get; set; }

        [JsonProperty("result_metadata")]
        public List<ResultMetadata> ResultMetadata { get; set; }

        [JsonProperty("initially_published_at")]
        public DateTime? InitiallyPublishedAt { get; set; }

        [JsonProperty("can_write")]
        public bool CanWrite { get; set; }

        [JsonProperty("card_schema")]
        public int? CardSchema { get; set; }

        [JsonProperty("database_id")]
        public int? DatabaseId { get; set; }

        [JsonProperty("enable_embedding")]
        public bool EnableEmbedding { get; set; }

        [JsonProperty("collection_id")]
        public int? CollectionId { get; set; }

        [JsonProperty("query_type")]
        public string QueryType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("query_average_duration")]
        public int? QueryAverageDuration { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("moderation_reviews")]
        public List<object> ModerationReviews { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("made_public_by_id")]
        public int? MadePublicById { get; set; }

        [JsonProperty("embedding_params")]
        public object EmbeddingParams { get; set; }

        [JsonProperty("query_description")]
        public string QueryDescription { get; set; }

        [JsonProperty("cache_ttl")]
        public int? CacheTtl { get; set; }

        [JsonProperty("dataset_query")]
        public DatasetQuery DatasetQuery { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("parameter_mappings")]
        public List<object> ParameterMappings { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("archived_directly")]
        public bool ArchivedDirectly { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("collection_preview")]
        public bool CollectionPreview { get; set; }

        [JsonProperty("visualization_settings")]
        public Dictionary<string, object> VisualizationSettings { get; set; }

        [JsonProperty("metabase_version")]
        public string MetabaseVersion { get; set; }

        [JsonProperty("parameters")]
        public List<object> Parameters { get; set; }

        [JsonProperty("dashboard_id")]
        public int? DashboardId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("public_uuid")]
        public string PublicUuid { get; set; }
        public MigrationActions MigrationAction { get; set; } = MigrationActions.None;
        public int? ExistingTargetCardId { get; set; }
    }

    public class ResultMetadata
    {
        [JsonProperty("semantic_type")]
        public string SemanticType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("settings")]
        public object Settings { get; set; }

        [JsonProperty("field_ref")]
        public List<object> FieldRef { get; set; }

        [JsonProperty("effective_type")]
        public string EffectiveType { get; set; }

        [JsonProperty("ident")]
        public string Ident { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("fingerprint")]
        public Fingerprint Fingerprint { get; set; }

        [JsonProperty("base_type")]
        public string BaseType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("database_type")]
        public string DatabaseType { get; set; }

        [JsonProperty("table_id")]
        public int? TableId { get; set; }

        [JsonProperty("coercion_strategy")]
        public string CoercionStrategy { get; set; }

        [JsonProperty("fk_target_field_id")]
        public int? FkTargetFieldId { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("visibility_type")]
        public string VisibilityType { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("aggregation_index")]
        public int? AggregationIndex { get; set; }
    }

    public class Fingerprint
    {
        [JsonProperty("global")]
        public GlobalFingerprint Global { get; set; }

        [JsonProperty("type")]
        public Dictionary<string, TypeFingerprint> Type { get; set; }
    }

    public class GlobalFingerprint
    {
        [JsonProperty("distinct-count")]
        public int DistinctCount { get; set; }

        [JsonProperty("nil%")]
        public double? NilPercentage { get; set; }
    }

    public class TypeFingerprint
    {
        [JsonProperty("min")]
        public double? Min { get; set; }

        [JsonProperty("q1")]
        public double? Q1 { get; set; }

        [JsonProperty("q3")]
        public double? Q3 { get; set; }

        [JsonProperty("max")]
        public double? Max { get; set; }

        [JsonProperty("sd")]
        public double? StandardDeviation { get; set; }

        [JsonProperty("avg")]
        public double? Average { get; set; }

        [JsonProperty("percent-json")]
        public double? PercentJson { get; set; }

        [JsonProperty("percent-url")]
        public double? PercentUrl { get; set; }

        [JsonProperty("percent-email")]
        public double? PercentEmail { get; set; }

        [JsonProperty("percent-state")]
        public double? PercentState { get; set; }

        [JsonProperty("average-length")]
        public double? AverageLength { get; set; }
    }

    public class DatasetQuery
    {
        [JsonProperty("database")]
        public int Database { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("query")]
        public Query Query { get; set; }
    }

    public class Query
    {
        [JsonProperty("source-table")]
        public object SourceTable { get; set; }

        [JsonProperty("aggregation")]
        public List<object> Aggregation { get; set; }

        [JsonProperty("breakout")]
        public List<object> Breakout { get; set; }

        [JsonProperty("aggregation-idents")]
        public Dictionary<string, string> AggregationIdents { get; set; }

        [JsonProperty("breakout-idents")]
        public Dictionary<string, string> BreakoutIdents { get; set; }

        [JsonProperty("joins")]
        public List<Join> Joins { get; set; }
    }

    public class Join
    {
        [JsonProperty("ident")]
        public string Ident { get; set; }

        [JsonProperty("strategy")]
        public string Strategy { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("condition")]
        public List<object> Condition { get; set; }

        [JsonProperty("source-table")]
        public string SourceTable { get; set; }
    }

    public class ParameterMapping
    {
        [JsonProperty("parameter_id")]
        public string ParameterId { get; set; }

        [JsonProperty("card_id")]
        public int CardId { get; set; }

        [JsonProperty("target")]
        public List<object> Target { get; set; }

    }

    public class DashboardTab
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("dashboard_id")]
        public int DashboardId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ParamField
    {
        [JsonProperty("semantic_type")]
        public string SemanticType { get; set; }

        [JsonProperty("table_id")]
        public int TableId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("has_field_values")]
        public string HasFieldValues { get; set; }

        [JsonProperty("fk_target_field_id")]
        public int? FkTargetFieldId { get; set; }

        [JsonProperty("dimensions")]
        public List<object> Dimensions { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("target")]
        public object Target { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("name_field")]
        public object NameField { get; set; }

        [JsonProperty("base_type")]
        public string BaseType { get; set; }
    }

    public class LastEditInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public class DashboardCollection
    {
        [JsonProperty("authority_level")]
        public string AuthorityLevel { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("archive_operation_id")]
        public int? ArchiveOperationId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("personal_owner_id")]
        public int? PersonalOwnerId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("is_sample")]
        public bool IsSample { get; set; }

        [JsonProperty("effective_location")]
        public string EffectiveLocation { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("archived_directly")]
        public bool? ArchivedDirectly { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("is_personal")]
        public bool IsPersonal { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sectionId")]
        public string SectionId { get; set; }

        [JsonProperty("isMultiSelect")]
        public bool? IsMultiSelect { get; set; }

        [JsonProperty("values_source_type")]
        public string ValuesSourceType { get; set; }

        [JsonProperty("values_source_config")]
        public ValuesSourceConfig ValuesSourceConfig { get; set; }
    }

    public class ValuesSourceConfig
    {
        [JsonProperty("card_id")]
        public int CardId { get; set; }

        [JsonProperty("value_field")]
        public List<object> ValueField { get; set; }
    }
}
