using System.Text.Json;
using MetabaseMigrator.Console.Config;
using System;
using MetabaseMigrator.Console.Services;
using System.Text.Json.Serialization;
using MetabaseMigrator.Console.Models;
using System.Runtime.Intrinsics.Arm;
using System.Net.Http;
using System.Text;

namespace MetabaseMigrator.Services
{
    /// <summary>
    /// Main service for handling Metabase dashboard migrations
    /// </summary>
    public class MigrationService : IDisposable
    {
        private readonly MetabaseClient _sourceClient;
        private readonly MetabaseClient _targetClient;
        private readonly LoggerService _logger;
        private readonly MigrationConfig _config;
        private MetabaseDashboard? _lastPreviewDashboard;

        public MigrationService(MigrationConfig config, LoggerService logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _sourceClient = new MetabaseClient(config.SourceUrl, config.SourceAPIToken, config, logger);
            _targetClient = new MetabaseClient(config.TargetUrl, config.TargetAPIToken, config, logger);
        }

        /// <summary>
        /// Execute complete migration process
        /// </summary>
        //public async Task<bool> MigrateDashboardAsync(string dashboardName)
        //{
        //    try
        //    {
        //        _logger.LogStep(1, "Authentication", "Authenticating with source and target instances");
        //        if (!await AuthenticateAsync())
        //            return false;

        //        _logger.LogStep(2, "Dashboard Discovery", $"Finding dashboard: {dashboardName}");
        //        var dashboard = await FindDashboardAsync(dashboardName);
        //        if (!dashboard.HasValue)
        //            return false;

        //        //_logger.LogStep(3, "Database Mapping", "Mapping databases between instances");
        //        //var dbMapping = await MapDatabasesAsync();

        //        _logger.LogStep(4, "Collection Migration", "Migrating collections");
        //        int? newCollectionId = null;

        //        if (dashboard.Value.TryGetProperty("collection_id", out var collectionProp) &&
        //            collectionProp.ValueKind != JsonValueKind.Null)
        //        {
        //            newCollectionId = await MigrateCollectionAsync(collectionProp.GetInt32());
        //        }


        //        _logger.LogStep(5, "Card Migration", "Migrating dashboard cards");
        //        var cardMapping = await MigrateCardsAsync(dashboard.Value, dbMapping);

        //        _logger.LogStep(6, "Dashboard Creation", "Creating dashboard in target instance");
        //        var newDashboardId = await CreateDashboardAsync(dashboard.Value, newCollectionId);
        //        if (!newDashboardId.HasValue)
        //            return false;

        //        _logger.LogStep(7, "Card Assignment", "Adding cards to dashboard");
        //        await AssignCardsAsync(newDashboardId.Value, dashboard.Value, cardMapping);

        //        _logger.LogSuccess("Migration completed successfully!");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Migration failed", ex);
        //        return false;
        //    }
        //}

        /// <summary>
        /// Step 1: Authenticate with both instances
        /// </summary>
        private async Task<bool> AuthenticateAsync()
        {
            try
            {
                var sourceAuth = await _sourceClient.AuthenticateAsync();
                var targetAuth = await _targetClient.AuthenticateAsync();

                if (!sourceAuth || !targetAuth)
                {
                    _logger.LogFailure("Authentication failed");
                    return false;
                }

                _logger.LogSuccess("Authentication completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Authentication process failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Step 2: Find dashboard in source instance
        /// </summary>
        public async Task<JsonElement?> ListSourceDashboardAsync()
        {
            try
            {
                System.Console.WriteLine($"Fetching Source dashboard");

                return await ListDashboardInternal(_sourceClient);
            }
            catch (Exception ex)
            {
                _logger.LogError("Dashboard discovery failed", ex);
                return null;
            }
        }

        private async Task<int?> ResolveCardCollectionAsync(int? cardCollectionId, Dictionary<int, int> sourceToTargetCollectionMap)
        {
            // If card has no collection, use null (root collection)
            if (!cardCollectionId.HasValue)
                return null;

            // If we already have a mapping for this collection, use it
            if (sourceToTargetCollectionMap.TryGetValue(cardCollectionId.Value, out var existingTargetId))
                return existingTargetId;

            // Otherwise, resolve this collection
            var targetCollectionId = await EnsureCollectionExistsAsync(cardCollectionId.Value, sourceToTargetCollectionMap);

            if (targetCollectionId.HasValue)
            {
                System.Console.WriteLine($"✓ Resolved card collection {cardCollectionId} → {targetCollectionId}");
                return targetCollectionId;
            }
            else
            {
                System.Console.WriteLine($"⚠ Could not resolve card collection {cardCollectionId}, using root collection");
                return null; // Fallback to root collection
            }
        }

        private async Task<Dictionary<string, int>> LoadTargetCardsAsync()
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var cardsJsonStr = await _targetClient.GetAsync("/api/card");
                var cardsJson = JsonSerializer.Deserialize<JsonElement>(cardsJsonStr);

                // /api/card returns an array, not { data: [...] }
                foreach (var card in cardsJson.EnumerateArray())
                {
                    var name = card.GetProperty("name").GetString();
                    var id = card.GetProperty("id").GetInt32();

                    if (!string.IsNullOrEmpty(name) && !result.ContainsKey(name))
                        result[name] = id;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Failed to load target cards: {ex.Message}");
            }

            return result;
        }

        private async Task<MetabaseDashboard?> PreviewCopy(int dashboardId)
        {
            MetabaseDashboard? dashboardObject;

            if (_lastPreviewDashboard != null && dashboardId == _lastPreviewDashboard.Id)
            {
                dashboardObject = _lastPreviewDashboard;
            }
            else
            {
                var dashboard = await FindDashboardAsync(dashboardId);
                dashboardObject = Newtonsoft.Json.JsonConvert.DeserializeObject<MetabaseDashboard>(dashboard?.ToString() ?? "");

                if (dashboardObject == null)
                {
                    PrintError("Dashboard not found or could not deserialize.");
                    return null;
                }

                _lastPreviewDashboard = dashboardObject;
            }

            System.Console.WriteLine($"\nFetching dashboard. Found {dashboardObject.Id}: {dashboardObject.Name}");
            System.Console.WriteLine($"Fetching dependencies. Found {dashboardObject.Dashcards.Count}.");


            // Load all target cards ONCE to save API calls
            var targetCards = await LoadTargetCardsAsync();

            bool skipAll = false;

            foreach (var dashCard in dashboardObject.Dashcards)
            {
                if (dashCard.Card.Id > 0)
                {
                    if (targetCards.TryGetValue(dashCard.Card.Name, out var existingId))
                    {
                        if (skipAll)
                        {
                            // User already chose "Skip All"
                            dashCard.Card.MigrationAction = MigrationActions.Skip;
                            dashCard.Card.ExistingTargetCardId = existingId;
                        }
                        else if (dashCard.Card.MigrationAction == MigrationActions.None)
                        {
                            // Ask user for decision
                            string? choice;
                            do
                            {
                                System.Console.WriteLine($"\nType\t\tID\tName");
                                System.Console.WriteLine($"--------------------------------");
                                System.Console.WriteLine(
                                    $"[{dashCard.Card.Type}]\t{dashCard.Card.Id}\t{dashCard.Card.Name} already exists in target. "

                                );
                                System.Console.WriteLine("\"Skip (S) / Override (O) / Skip All (SA): \"");
                                choice = System.Console.ReadLine()?.Trim().ToUpperInvariant();

                                if (choice == "S")
                                {
                                    dashCard.Card.MigrationAction = MigrationActions.Skip;
                                    dashCard.Card.ExistingTargetCardId = existingId;
                                }
                                else if (choice == "O")
                                {
                                    dashCard.Card.MigrationAction = MigrationActions.Override;
                                    dashCard.Card.ExistingTargetCardId = existingId;
                                }
                                else if (choice == "SA")
                                {
                                    dashCard.Card.MigrationAction = MigrationActions.Skip;
                                    dashCard.Card.ExistingTargetCardId = existingId;
                                    skipAll = true; // From now on all duplicates will be skipped
                                }
                                else
                                {
                                    System.Console.WriteLine("❌ Invalid choice. Please enter 'S' for Skip, 'O' for Override, or 'SA' for Skip All.");
                                    choice = null; // force loop again
                                }
                            }
                            while (choice == null);
                        }
                    }
                    else
                    {
                        dashCard.Card.MigrationAction = MigrationActions.New;
                    }

                }
            }

            // Print summary
            System.Console.WriteLine("\n=== Dependency Summary ===");
            System.Console.WriteLine($"\nType\t\tID\tName => Decision");
            System.Console.WriteLine($"--------------------------------");
            foreach (var dashCard in dashboardObject.Dashcards)
            {
                if (dashCard.Card.Id > 0)
                {
                    System.Console.WriteLine($"[{dashCard.Card.Type}]\t{dashCard.Card.Id}\t{dashCard.Card.Name} => {dashCard.Card.MigrationAction}");
                }
            }

            return dashboardObject;
        }


        public async Task DryCopy()
        {
            System.Console.Write("Enter Source ID to dry copy: ");
            if (!int.TryParse(System.Console.ReadLine(), out var id))
            {
                PrintError("Invalid ID");
                return;
            }

            await PreviewCopy(id);

        }


        public async Task Copy()
        {
            System.Console.Write("Enter Source ID to copy: ");
            if (!int.TryParse(System.Console.ReadLine(), out var id))
            {
                PrintError("Invalid ID");
                return;
            }

            var dashboardObject = await PreviewCopy(id);
            if (dashboardObject == null) return;

            System.Console.Write("\nAre you sure you want to continue with copy? (Y/N): ");
            var confirm = System.Console.ReadLine();

            return;

            if (confirm?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    //await PerformCopy(dashboardObject);

                    //System.Console.WriteLine($"\n Copy completed! New dashboard ID: {newId}");
                }
                catch (Exception ex)
                {
                    PrintError($"Error during copy: {ex.Message}");
                }
            }
            else
            {
                PrintError("Copy cancelled. Returning to main menu...");
            }
        }

        public async Task<int?> PerformCopy(MetabaseDashboard dashboard)
        {
            try
            {
                System.Console.WriteLine("Starting migration process...");

                // Handle Collection
                int? newCollectionId = null;

                if (dashboard.CollectionId != null)
                {
                    var collectionMapping = new Dictionary<int, int>();
                    newCollectionId = await EnsureCollectionExistsAsync(dashboard.CollectionId.Value, collectionMapping);

                    if (newCollectionId.HasValue)
                        System.Console.WriteLine($"Collection resolved to target ID {newCollectionId}");
                    else
                    {
                        System.Console.WriteLine($"Could not resolve collection for source id {dashboard.CollectionId}. Using root.");
                        newCollectionId = null; // Use root collection
                    }
                }
                else
                {
                    System.Console.WriteLine("Dashboard has no collection (Root). Keeping collection_id null on create.");
                    newCollectionId = null;
                }

                // Migrate cards based on preview decisions
                var cardMapping = new Dictionary<int, int>();
                var sourceToTargetCollectionMap = new Dictionary<int, int>();

                // Pre-populate with dashboard collection mapping if it exists
                if (dashboard.CollectionId.HasValue && newCollectionId.HasValue)
                {
                    sourceToTargetCollectionMap[dashboard.CollectionId.Value] = newCollectionId.Value;
                }

                foreach (var dashCard in dashboard.Dashcards ?? new List<DashboardCard>())
                {
                    var card = dashCard.Card;

                    if (card.Id <= 0) continue; // Skip invalid cards
                    int? cardCollectionId;
                    switch (card.MigrationAction)
                    {
                        case MigrationActions.Skip:
                            if (card.ExistingTargetCardId.HasValue)
                            {
                                cardMapping[card.Id] = card.ExistingTargetCardId.Value;
                                System.Console.WriteLine($"⚠ Skipped card '{card.Name}' (using existing ID: {card.ExistingTargetCardId})");
                            }
                            break;

                        case MigrationActions.Override:
                            if (card.ExistingTargetCardId.HasValue)
                            {
                                // Resolve card's specific collection (might be different from dashboard collection)
                                cardCollectionId = await ResolveCardCollectionAsync(card.CollectionId, sourceToTargetCollectionMap);

                                // Update existing card
                                var updatePayload = new
                                {
                                    name = card.Name,
                                    description = card.Description ?? "",
                                    dataset_query = card.DatasetQuery,
                                    display = card.Display,
                                    visualization_settings = card.VisualizationSettings,
                                    collection_id = cardCollectionId,
                                    type = card.Type,
                                };

                                await UpdateCardAsync(card.ExistingTargetCardId.Value, updatePayload);
                                cardMapping[card.Id] = card.ExistingTargetCardId.Value;
                                System.Console.WriteLine($"✓ Updated existing card '{card.Name}' (ID: {card.ExistingTargetCardId})");
                            }
                            break;

                        case MigrationActions.New:
                            // Resolve card's specific collection (might be different from dashboard collection)
                             cardCollectionId = await ResolveCardCollectionAsync(card.CollectionId, sourceToTargetCollectionMap);

                            // Create new card
                            var newCardPayload = new
                            {
                                name = card.Name,
                                description = card.Description ?? "",
                                dataset_query = card.DatasetQuery,
                                display = card.Display,
                                visualization_settings = card.VisualizationSettings,
                                collection_id = cardCollectionId,
                                type = card.Type,
                            };

                            var newCard = await _targetClient.CreateCardAsync(newCardPayload);
                            var newCardId = newCard.GetProperty("id").GetInt32();
                            cardMapping[card.Id] = newCardId;

                            System.Console.WriteLine($"Created new card '{card.Name}' ({card.Id} → {newCardId})");
                            break;

                        default:
                            System.Console.WriteLine($"Unknown migration action for card '{card.Name}', skipping");
                            break;
                    }
                }

                // 3️⃣ Create or reuse dashboard
                int? newDashboardId;

                if (await DashboardExistsAsync(dashboard.Name))
                {
                    newDashboardId = await GetExistingDashboardIdAsync(dashboard.Name);
                    System.Console.WriteLine($"Dashboard '{dashboard.Name}' already exists in target (ID: {newDashboardId})");
                }
                else
                {
                    var dashPayload = new
                    {
                        name = dashboard.Name,
                        description = dashboard.Description ?? "",
                        collection_id = newCollectionId
                    };

                    var newDash = await _targetClient.CreateDashboardAsync(dashPayload);
                    newDashboardId = newDash.GetProperty("id").GetInt32();

                    System.Console.WriteLine($"Created dashboard '{dashboard.Name}' (ID: {newDashboardId})");
                }

                // Assign cards to dashboard
                foreach (var dashCard in dashboard.Dashcards ?? new List<DashboardCard>())
                {
                    if (cardMapping.TryGetValue(dashCard.Card.Id, out var targetCardId))
                    {
                        var payload = new
                        {
                            cardId = targetCardId,
                            row = dashCard.Row,
                            col = dashCard.Col,
                            size_x = dashCard.SizeX,
                            size_y = dashCard.SizeY,
                            parameter_mappings = dashCard.ParameterMappings ?? new { },
                            visualization_settings = dashCard.VisualizationSettings ?? new { }
                        };

                        await _targetClient.AddCardToDashboardAsync(newDashboardId.Value, payload);

                        System.Console.WriteLine($"✓ Added card '{dashCard.Card.Name}' to dashboard {newDashboardId}");
                    }
                    else
                    {
                        System.Console.WriteLine($"⚠ Skipped card {dashCard.Card.Id} (not migrated)");
                    }
                }

                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"\n✓ Migration completed! New dashboard ID: {newDashboardId}");
                System.Console.ResetColor();

                return newDashboardId;
            }
            catch (Exception ex)
            {
                PrintError($"Error in copy: {ex.Message}");
                return null;
            }
        }

        public async Task<JsonElement> UpdateCardAsync(int cardId, object payload)
        {
            // Serialize the payload
            var json = JsonSerializer.Serialize(payload);

            // Call your wrapper’s PutAsync (returns string)
            var responseStr = await _targetClient.PutAsync($"/api/card/{cardId}", json);

            // Deserialize response into JsonElement
            return JsonSerializer.Deserialize<JsonElement>(responseStr)!;
        }

        private async Task<int?> EnsureCollectionExistsAsync(
            int sourceCollectionId,
            Dictionary<int, int> sourceToTargetCollectionMap,
            HashSet<int>? visited = null)
        {
            visited ??= new HashSet<int>();

            // cycle protection
            if (!visited.Add(sourceCollectionId))
            {
                System.Console.WriteLine($"⚠ Detected cycle while resolving collection {sourceCollectionId}. Aborting this branch.");
                return null;
            }

            // cached mapping?
            if (sourceToTargetCollectionMap.TryGetValue(sourceCollectionId, out var cachedTargetId))
                return cachedTargetId;

            // Fetch collection JSON from source
            JsonElement sourceCollection;
            try
            {
                sourceCollection = await GetSourceCollectionAsync(sourceCollectionId);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Failed to fetch source collection {sourceCollectionId}: {ex.Message}");
                return null;
            }

            // read fields safely
            string name = sourceCollection.TryGetProperty("name", out var nameProp) && nameProp.ValueKind != JsonValueKind.Null
                ? nameProp.GetString() ?? $"Collection_{sourceCollectionId}"
                : $"Collection_{sourceCollectionId}";

            string description = sourceCollection.TryGetProperty("description", out var descProp) && descProp.ValueKind != JsonValueKind.Null
                ? descProp.GetString() ?? ""
                : "";

            string authority = sourceCollection.TryGetProperty("authority_level", out var authProp) && authProp.ValueKind != JsonValueKind.Null
                ? authProp.GetString() ?? "official"
                : "official";

            string ns = sourceCollection.TryGetProperty("namespace", out var nsProp) && nsProp.ValueKind != JsonValueKind.Null
                ? nsProp.GetString() ?? ""
                : "";

            int? sourceParentId = null;
            if (sourceCollection.TryGetProperty("parent_id", out var parentProp) && parentProp.ValueKind != JsonValueKind.Null)
            {
                if (parentProp.ValueKind == JsonValueKind.Number && parentProp.TryGetInt32(out var p))
                    sourceParentId = p;
            }

            // Resolve target parent (recursive) — if source had no parent, put under root (use 1 as default)
            int? targetParentId;
            if (sourceParentId.HasValue)
            {
                targetParentId = await EnsureCollectionExistsAsync(sourceParentId.Value, sourceToTargetCollectionMap, visited);
                // if parent resolution failed, decide fallback behavior (we choose root fallback)
                if (!targetParentId.HasValue)
                {
                    System.Console.WriteLine($"⚠ Parent collection {sourceParentId} could not be created/resolved. Using root as parent for '{name}'.");
                    targetParentId = null; // Use null for root
                }
            }
            else
            {
                targetParentId = null; // root
            }

            // Try to find an existing collection on target with same name + parent
            var existingTargetId = await FindTargetCollectionByNameAndParentAsync(name, targetParentId);
            if (existingTargetId.HasValue)
            {
                sourceToTargetCollectionMap[sourceCollectionId] = existingTargetId.Value;
                System.Console.WriteLine($"✓ Reusing existing target collection '{name}' (ID: {existingTargetId.Value}) under parent {targetParentId}");
                return existingTargetId.Value;
            }

            // Not found — create a new collection on target using source's properties
            var payload = new
            {
                name = name,
                description = description,
                authority_level = authority,
                @namespace = ns,                 // use @namespace for the anonymous object
                parent_id = targetParentId
            };

            try
            {
                var created = await CreateTargetCollectionAsync(payload);
                var newId = created.GetProperty("id").GetInt32();
                sourceToTargetCollectionMap[sourceCollectionId] = newId;
                System.Console.WriteLine($"✓ Created collection '{name}' on target (ID: {newId}) with parent {targetParentId}");
                return newId;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Failed to create collection '{name}' on target: {ex}");
                return null;
            }
        }

                private async Task<JsonElement> CreateTargetCollectionAsync(object payload)
        {
            var response = await _targetClient.CreateCollectionAsync(payload);
            return response;
        }

        private async Task<int?> FindTargetCollectionByNameAndParentAsync(string name, int? parentId)
        {
            try
            {
                var collections = await _targetClient.GetAsync("/api/collection");
                var collectionsJson = JsonSerializer.Deserialize<JsonElement>(collections);

                foreach (var collection in collectionsJson.GetProperty("data").EnumerateArray())
                {
                    var collectionName = collection.GetProperty("name").GetString();
                    var collectionParentId = collection.TryGetProperty("parent_id", out var parentProp) && parentProp.ValueKind != JsonValueKind.Null
                        ? (int?)parentProp.GetInt32()
                        : null;

                    if (collectionName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                        collectionParentId == parentId)
                    {
                        return collection.GetProperty("id").GetInt32();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<JsonElement> GetSourceCollectionAsync(int collectionId)
        {
            var response = await _sourceClient.GetAsync($"/api/collection/{collectionId}");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        public void PrintError(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: {message}");
            System.Console.ResetColor();
        }
        public async Task<JsonElement?> ListTargetDashboardAsync()
        {
            try
            {
                System.Console.WriteLine($"Fetching Target dashboard");
                return await ListDashboardInternal(_targetClient);
            }
            catch (Exception ex)
            {
                _logger.LogError("Dashboard discovery failed", ex);
                return null;
            }
        }
        private async Task<JsonElement?> ListDashboardInternal(MetabaseClient client)
        {
            try
            {

                var dashboards = await client.GetDashboardsAsync();

                var dashboardsList = dashboards.EnumerateArray().ToList();
                var count = dashboardsList.Count;
                if (count > 0)
                {
                    System.Console.WriteLine($"Total dashbaord found: {count}\n");
                    System.Console.WriteLine($"Id\tName");
                    System.Console.WriteLine($"_________________________________________");
                }
                else
                    System.Console.WriteLine("No dashboard found. Please check parameters");


                var orderedDashboards = dashboardsList.Select(d => new
                {
                    Id = d.GetProperty("id").GetInt32(),
                    Name = d.GetProperty("name").GetString()
                })
                    .OrderBy(d => d.Name)
                    .ToList();

                foreach (var dashboard in orderedDashboards)
                {
                    var id = dashboard.Id;
                    var name = dashboard.Name;

                    System.Console.WriteLine($"{id}\t{name}");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Dashboard discovery failed", ex);
                return null;
            }
        }
        /// <summary>
        /// Step 2: Find dashboard in source instance
        /// </summary>
        private async Task<JsonElement?> FindDashboardAsync(int dashboardId)
        {
            try
            {
                var response = await _sourceClient.GetDashboardAsync(dashboardId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Dashboard discovery failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Step 3: Map databases between source and target
        /// </summary>
        private async Task<Dictionary<int, int>> MapDatabasesAsync()
        {
            var mapping = new Dictionary<int, int>();

            try
            {
                var sourceDbs = await _sourceClient.GetDatabasesAsync();
                var targetDbs = await _targetClient.GetDatabasesAsync();

                var sourceDbList = sourceDbs.GetProperty("data").EnumerateArray().ToList();
                var targetDbList = targetDbs.GetProperty("data").EnumerateArray().ToList();

                foreach (var sourceDb in sourceDbList)
                {
                    var sourceName = sourceDb.GetProperty("name").GetString();
                    var sourceId = sourceDb.GetProperty("id").GetInt32();

                    var matchingTarget = targetDbList.FirstOrDefault(db =>
                        db.GetProperty("name").GetString()?.Equals(sourceName, StringComparison.OrdinalIgnoreCase) == true);

                    if (matchingTarget.ValueKind != JsonValueKind.Undefined)
                    {
                        var targetId = matchingTarget.GetProperty("id").GetInt32();
                        mapping[sourceId] = targetId;
                        _logger.LogProgress($"Mapped database: {sourceName} ({sourceId} → {targetId})");
                    }
                    else
                    {
                        _logger.LogWarning($"No matching target database found for: {sourceName}");
                    }
                }

                _logger.LogSuccess($"Mapped {mapping.Count} databases");
                return mapping;
            }
            catch (Exception ex)
            {
                _logger.LogError("Database mapping failed", ex);
                return mapping;
            }
        }

        /// <summary>
        /// Step 4: Migrate collection if needed
        /// </summary>
        private async Task<int?> MigrateCollectionAsync(int collectionId)
        {
            try
            {
                if (_config.SkipExistingCollections)
                {
                    _logger.LogProgress("Skipping collection migration (configured to skip)");
                    return null;
                }

                var collection = await _sourceClient.GetCollectionAsync(collectionId);

                var collectionName = collection.GetProperty("name").GetString();

                // Check if collection with same name already exists
                if (await CollectionExistsAsync(collectionName))
                {
                    _logger.LogProgress($"Collection '{collectionName}' already exists, skipping");
                    return await GetExistingCollectionIdAsync(collectionName);
                }

                var payload = new
                {
                    name = collectionName,
                    description = collection.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                    color = collection.TryGetProperty("color", out var color) ? color.GetString() : "#509EE3"
                };

                var newCollection = await _targetClient.CreateCollectionAsync(payload);
                var newId = newCollection.GetProperty("id").GetInt32();

                _logger.LogSuccess($"Migrated collection: {payload.name}");
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError("Collection migration failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Step 5: Migrate all cards used in the dashboard
        /// </summary>
        //private async Task<Dictionary<int, int>> MigrateCardsAsync(JsonElement dashboard, Dictionary<int, int> dbMapping)
        //{
        //    var cardMapping = new Dictionary<int, int>();

        //    try
        //    {
        //        var orderedCards = dashboard.GetProperty("ordered_cards").EnumerateArray().ToList();
        //        _logger.LogProgress($"Processing {orderedCards.Count} cards");

        //        int processedCount = 0;
        //        foreach (var orderedCard in orderedCards)
        //        {
        //            var cardId = orderedCard.GetProperty("card").GetProperty("id").GetInt32();

        //            processedCount++;
        //            _logger.LogProgress($"Processing card {processedCount}/{orderedCards.Count} (ID: {cardId})");

        //            if (_config.SkipExistingCards && cardMapping.ContainsKey(cardId))
        //            {
        //                _logger.LogProgress($"Skipping existing card: {cardId}");
        //                continue;
        //            }

        //            // Retry mechanism for card migration
        //            int retryCount = 0;
        //            while (retryCount < _config.RetryAttempts)
        //            {
        //                try
        //                {
        //                    var cardData = await _sourceClient.GetCardAsync(cardId);
        //                    var cardName = cardData.GetProperty("name").GetString() ?? $"Card {cardId}";

        //                    // Check if card with same name exists
        //                    if (await CardExistsAsync(cardName))
        //                    {
        //                        _logger.LogProgress($"Card '{cardName}' already exists, skipping");
        //                        var existingCardId = await GetExistingCardIdAsync(cardName);
        //                        if (existingCardId.HasValue)
        //                        {
        //                            cardMapping[cardId] = existingCardId.Value;
        //                        }
        //                        break;
        //                    }

        //                    var newCardId = await CreateCardAsync(cardData, dbMapping);

        //                    if (newCardId.HasValue)
        //                    {
        //                        cardMapping[cardId] = newCardId.Value;
        //                        _logger.LogProgress($"✓ Migrated card: {cardName} ({cardId} → {newCardId})");
        //                        break;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    retryCount++;
        //                    if (retryCount < _config.RetryAttempts)
        //                    {
        //                        _logger.LogWarning($"Card migration attempt {retryCount} failed for card {cardId}: {ex.Message}. Retrying...");
        //                        await Task.Delay(TimeSpan.FromSeconds(_config.RetryDelaySeconds));
        //                    }
        //                    else
        //                    {
        //                        _logger.LogError($"Failed to migrate card {cardId} after {_config.RetryAttempts} attempts", ex);
        //                    }
        //                }
        //            }
        //        }

        //        _logger.LogSuccess($"Migrated {cardMapping.Count} out of {orderedCards.Count} cards");
        //        return cardMapping;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Card migration failed", ex);
        //        return cardMapping;
        //    }
        //}

        /// <summary>
        /// Step 6: Create dashboard in target instance
        /// </summary>
        private async Task<int?> CreateDashboardAsync(JsonElement sourceDashboard, int? collectionId)
        {
            try
            {
                var dashboardName = sourceDashboard.GetProperty("name").GetString();

                // Check if dashboard already exists
                if (await DashboardExistsAsync(dashboardName))
                {
                    _logger.LogWarning($"Dashboard '{dashboardName}' already exists in target");
                    var existingId = await GetExistingDashboardIdAsync(dashboardName);
                    if (existingId.HasValue)
                    {
                        _logger.LogProgress($"Using existing dashboard ID: {existingId}");
                        return existingId;
                    }
                }

                var payload = new
                {
                    name = dashboardName,
                    description = sourceDashboard.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                    collection_id = collectionId
                };

                var newDashboard = await _targetClient.CreateDashboardAsync(payload);
                var dashboardId = newDashboard.GetProperty("id").GetInt32();

                _logger.LogSuccess($"Created dashboard: {payload.name} (ID: {dashboardId})");
                return dashboardId;
            }
            catch (Exception ex)
            {
                _logger.LogError("Dashboard creation failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Step 7: Add cards to the new dashboard
        /// </summary>
        private async Task AssignCardsAsync(int dashboardId, JsonElement sourceDashboard, Dictionary<int, int> cardMapping)
        {
            try
            {
                var orderedCards = sourceDashboard.GetProperty("ordered_cards").EnumerateArray().ToList();
                int assignedCount = 0;
                int skippedCount = 0;

                _logger.LogProgress($"Assigning {orderedCards.Count} cards to dashboard");

                foreach (var orderedCard in orderedCards)
                {
                    var sourceCardId = orderedCard.GetProperty("card").GetProperty("id").GetInt32();

                    if (cardMapping.TryGetValue(sourceCardId, out var targetCardId))
                    {
                        try
                        {
                            var payload = new
                            {
                                cardId = targetCardId,
                                row = orderedCard.TryGetProperty("row", out var rowProp) ? rowProp.GetInt32() : 0,
                                col = orderedCard.TryGetProperty("col", out var colProp) ? colProp.GetInt32() : 0,
                                size_x = orderedCard.TryGetProperty("size_x", out var sizeXProp) ? sizeXProp.GetInt32() : 4,
                                size_y = orderedCard.TryGetProperty("size_y", out var sizeYProp) ? sizeYProp.GetInt32() : 4,
                                parameter_mappings = orderedCard.TryGetProperty("parameter_mappings", out var paramMappings)
                                    ? JsonSerializer.Deserialize<object>(paramMappings.GetRawText())
                                    : new { },
                                visualization_settings = orderedCard.TryGetProperty("visualization_settings", out var vizSettings)
                                    ? JsonSerializer.Deserialize<object>(vizSettings.GetRawText())
                                    : new { }
                            };

                            await _targetClient.AddCardToDashboardAsync(dashboardId, payload);
                            assignedCount++;
                            _logger.LogProgress($"✓ Added card to dashboard: {sourceCardId} → {targetCardId} (position: {payload.row},{payload.col})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to add card {sourceCardId} to dashboard: {ex.Message}");
                            skippedCount++;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Card {sourceCardId} was not migrated, skipping dashboard assignment");
                        skippedCount++;
                    }
                }

                _logger.LogSuccess($"Card assignment completed: {assignedCount} assigned, {skippedCount} skipped");
            }
            catch (Exception ex)
            {
                _logger.LogError("Card assignment failed", ex);
            }
        }

        /// <summary>
        /// Helper: Create a new card with updated database references
        /// </summary>
        private async Task<int?> CreateCardAsync(JsonElement sourceCard, Dictionary<int, int> dbMapping)
        {
            try
            {
                // Clone the dataset query and update database reference
                var datasetQuery = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    sourceCard.GetProperty("dataset_query").GetRawText());

                if (datasetQuery != null && datasetQuery.TryGetValue("database", out var dbObj))
                {
                    if (dbObj is JsonElement dbElement && dbElement.ValueKind == JsonValueKind.Number)
                    {
                        var oldDbId = dbElement.GetInt32();
                        if (dbMapping.TryGetValue(oldDbId, out var newDbId))
                        {
                            datasetQuery["database"] = newDbId;
                            _logger.LogDebug($"Updated database reference: {oldDbId} → {newDbId}");
                        }
                        else
                        {
                            _logger.LogWarning($"No database mapping found for database ID {oldDbId}");
                            return null;
                        }
                    }
                }

                var payload = new
                {
                    name = sourceCard.GetProperty("name").GetString(),
                    description = sourceCard.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                    dataset_query = datasetQuery,
                    display = sourceCard.GetProperty("display").GetString(),
                    visualization_settings = sourceCard.TryGetProperty("visualization_settings", out var vis)
                        ? JsonSerializer.Deserialize<object>(vis.GetRawText())
                        : new { },
                    collection_id = sourceCard.TryGetProperty("collection_id", out var collId) && collId.ValueKind != JsonValueKind.Null
                        ? collId.GetInt32()
                        : (int?)null
                };


                var newCard = await _targetClient.CreateCardAsync(payload);
                return newCard.GetProperty("id").GetInt32();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create card: {sourceCard.GetProperty("name")}", ex);
                return null;
            }
        }

        /// <summary>
        /// Check if collection exists in target
        /// </summary>
        private async Task<bool> CollectionExistsAsync(string? collectionName)
        {
            if (string.IsNullOrEmpty(collectionName)) return false;

            try
            {
                var collections = await _targetClient.GetAsync("/api/collection");
                var collectionsJson = JsonSerializer.Deserialize<JsonElement>(collections);

                foreach (var collection in collectionsJson.GetProperty("data").EnumerateArray())
                {
                    var name = collection.GetProperty("name").GetString();
                    if (name?.Equals(collectionName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get existing collection ID by name
        /// </summary>
        private async Task<int?> GetExistingCollectionIdAsync(string? collectionName)
        {
            if (string.IsNullOrEmpty(collectionName)) return null;

            try
            {
                var collections = await _targetClient.GetAsync("/api/collection");
                var collectionsJson = JsonSerializer.Deserialize<JsonElement>(collections);

                foreach (var collection in collectionsJson.GetProperty("data").EnumerateArray())
                {
                    var name = collection.GetProperty("name").GetString();
                    if (name?.Equals(collectionName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return collection.GetProperty("id").GetInt32();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if card exists in target
        /// </summary>
        private async Task<int?> CardExistsAsync(string? cardName)
        {
            if (string.IsNullOrEmpty(cardName)) return null;

            try
            {
                var cards = await _targetClient.GetAsync("/api/card");
                var cardsJson = JsonSerializer.Deserialize<JsonElement>(cards);

                foreach (var card in cardsJson.GetProperty("data").EnumerateArray())
                {
                    var name = card.GetProperty("name").GetString();
                    if (name?.Equals(cardName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return card.GetProperty("id").GetInt32(); // return ID if found
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get existing card ID by name
        /// </summary>
        private async Task<int?> GetExistingCardIdAsync(string? cardName)
        {
            if (string.IsNullOrEmpty(cardName)) return null;

            try
            {
                var cards = await _targetClient.GetAsync("/api/card");
                var cardsJson = JsonSerializer.Deserialize<JsonElement>(cards);

                foreach (var card in cardsJson.GetProperty("data").EnumerateArray())
                {
                    var name = card.GetProperty("name").GetString();
                    if (name?.Equals(cardName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return card.GetProperty("id").GetInt32();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if dashboard exists in target
        /// </summary>
        private async Task<bool> DashboardExistsAsync(string? dashboardName)
        {
            if (string.IsNullOrEmpty(dashboardName)) return false;

            try
            {
                var dashboards = await _targetClient.GetDashboardsAsync();

                foreach (var dashboard in dashboards.GetProperty("data").EnumerateArray())
                {
                    var name = dashboard.GetProperty("name").GetString();
                    if (name?.Equals(dashboardName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get existing dashboard ID by name
        /// </summary>
        private async Task<int?> GetExistingDashboardIdAsync(string? dashboardName)
        {
            if (string.IsNullOrEmpty(dashboardName)) return null;

            try
            {
                var dashboards = await _targetClient.GetDashboardsAsync();

                foreach (var dashboard in dashboards.GetProperty("data").EnumerateArray())
                {
                    var name = dashboard.GetProperty("name").GetString();
                    if (name?.Equals(dashboardName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return dashboard.GetProperty("id").GetInt32();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Test connections to both instances
        /// </summary>
        public async Task<bool> TestConnectionsAsync()
        {
            _logger.LogProgress("Testing source connection...");
            var sourceOk = await _sourceClient.TestConnectionAsync();

            _logger.LogProgress("Testing target connection...");
            var targetOk = await _targetClient.TestConnectionAsync();

            if (sourceOk && targetOk)
            {
                _logger.LogSuccess("Both connections are working");
                return true;
            }
            else
            {
                _logger.LogFailure($"Connection test failed - Source: {(sourceOk ? "OK" : "FAILED")}, Target: {(targetOk ? "OK" : "FAILED")}");
                return false;
            }
        }

        /// <summary>
        /// Get migration summary
        /// </summary>
        //public async Task<MigrationSummary> GetMigrationSummaryAsync(string dashboardName)
        //{
        //    var summary = new MigrationSummary
        //    {
        //        DashboardName = dashboardName,
        //        StartTime = DateTime.Now
        //    };

        //    try
        //    {
        //        // Get source dashboard info
        //        var dashboard = await FindDashboardAsync(dashboardName);
        //        if (dashboard.HasValue)
        //        {
        //            summary.SourceDashboardFound = true;
        //            summary.CardCount = dashboard.Value.GetProperty("ordered_cards").GetArrayLength();

        //            if (dashboard.Value.TryGetProperty("collection_id", out var collectionProp) &&
        //                collectionProp.ValueKind != JsonValueKind.Null)
        //            {
        //                summary.HasCollection = true;
        //            }
        //        }

        //        // Get database mapping info
        //        var dbMapping = await MapDatabasesAsync();
        //        summary.DatabaseMappings = dbMapping.Count;

        //        summary.EndTime = DateTime.Now;
        //        return summary;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Failed to get migration summary", ex);
        //        summary.EndTime = DateTime.Now;
        //        summary.ErrorMessage = ex.Message;
        //        return summary;
        //    }
        //}

        public void Dispose()
        {
            _sourceClient?.Dispose();
            _targetClient?.Dispose();
        }
    }

    /// <summary>
    /// Migration summary information
    /// </summary>
    public class MigrationSummary
    {
        public string DashboardName { get; set; } = string.Empty;
        public bool SourceDashboardFound { get; set; }
        public int CardCount { get; set; }
        public bool HasCollection { get; set; }
        public int DatabaseMappings { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? ErrorMessage { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        public override string ToString()
        {
            return $"Migration Summary:\n" +
                   $"  Dashboard: {DashboardName}\n" +
                   $"  Cards: {CardCount}\n" +
                   $"  Has Collection: {HasCollection}\n" +
                   $"  Database Mappings: {DatabaseMappings}\n" +
                   $"  Duration: {Duration.TotalSeconds:F1} seconds\n" +
                   $"  Status: {(string.IsNullOrEmpty(ErrorMessage) ? "Success" : $"Failed - {ErrorMessage}")}";
        }
    }
}