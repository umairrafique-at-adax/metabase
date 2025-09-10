using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using MetabaseMigrator.Console.Config;
using RestSharp;

namespace MetabaseMigrator.Console.Services
{
    /// <summary>
    /// HTTP client wrapper for Metabase API operations
    /// </summary>
    public class MetabaseClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly LoggerService _logger;
        private readonly string _baseUrl;
        private string? _apiToken;

        public MetabaseClient(string baseUrl,string apiKey, MigrationConfig config, LoggerService logger)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiToken = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MetabaseMigrator/1.0");
        }

        /// <summary>
        /// Authenticate with Metabase and get session token
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                _logger.LogDebug($"Attempting authentication with {_baseUrl}");
                var response = await GetAsync("/api/api-key");

                _logger.LogSuccess($"Authenticated with {_baseUrl}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Authentication failed for {_baseUrl}", ex);
                return false;
            }
        }



        public async Task<JsonElement> UpdateDashboardAsync(int dashboardId, object payload,bool model)
        {
            string json;
            if (model)
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            }
            else { 
            
                json = JsonSerializer.Serialize(payload);
            }

            var responseStr = await PutAsync($"/api/dashboard/{dashboardId}", json);

            if (string.IsNullOrWhiteSpace(responseStr))
                throw new InvalidOperationException("Dashboard update returned empty response.");

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(responseStr);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to parse dashboard update response. Raw response:\n{responseStr}", ex);
            }
        }



        /// <summary>
        /// Get data from Metabase API
        /// </summary>
        public async Task<string> GetAsync(string endpoint)
        {
            return await SendRequestAsync(HttpMethod.Get, endpoint);
        }


        /// <summary>
        /// Post data to Metabase API
        /// </summary>
        public async Task<string> PostAsync(string endpoint, string jsonContent)
        {
            return await SendRequestAsync(HttpMethod.Post, endpoint, jsonContent);
        }

        /// <summary>
        /// Put data to Metabase API
        /// </summary>
        public async Task<string> PutAsync(string endpoint, string jsonContent)
        {
            return await SendRequestAsync(HttpMethod.Put, endpoint, jsonContent);
        }

        /// <summary>
        /// Delete data from Metabase API
        /// </summary>
        public async Task<string> DeleteAsync(string endpoint)
        {
            return await SendRequestAsync(HttpMethod.Delete, endpoint);
        }

        /// <summary>
        /// Get all dashboards
        /// </summary>
        public async Task<JsonElement> GetDashboardsAsync()
        {
            var response = await GetAsync("/api/dashboard");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Get specific dashboard by ID
        /// </summary>
        public async Task<JsonElement> GetDashboardAsync(int dashboardId)
        {
            var response = await GetAsync($"/api/dashboard/{dashboardId}");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Get all databases
        /// </summary>
        public async Task<JsonElement> GetDatabasesAsync()
        {
            var response = await GetAsync("/api/database");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }        
        
        
        public async Task<JsonElement> GetDatabaseMetadataAsync(int id)
        {
            var response = await GetAsync($"/api/database/{id}/metadata");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }



        /// <summary>
        /// Get all cards
        /// </summary>
        public async Task<JsonElement> GetCardsAsync()
        {
            var response = await GetAsync("/api/card");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }


        /// <summary>
        /// Get specific card by ID
        /// </summary>
        public async Task<JsonElement> GetCardAsync(int cardId)
        {
            var response = await GetAsync($"/api/card/{cardId}");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Get specific collection by ID
        /// </summary>
        public async Task<JsonElement> GetCollectionAsync(int collectionId)
        {
            var response = await GetAsync($"/api/collection/{collectionId}");
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Create a new card
        /// </summary>
        public async Task<JsonElement> CreateCardAsync(object cardData)
        {
            var json = JsonSerializer.Serialize(cardData);
            var response = await PostAsync("/api/card", json);
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Create a new collection
        /// </summary>
        public async Task<JsonElement> CreateCollectionAsync(object payload)
        {

            var json = JsonSerializer.Serialize(payload);
            var responseStr = await PostAsync("/api/collection", json);

            var created = JsonSerializer.Deserialize<JsonElement>(responseStr);

            // Optional: sanity check that it's an object and contains an "id"
            if (created.ValueKind != JsonValueKind.Object || !created.TryGetProperty("id", out _))
            {
                throw new InvalidOperationException("Expected single collection object with 'id'.");
            }

            return created;
        }

        /// <summary>
        /// Create a new dashboard
        /// </summary>
        public async Task<JsonElement> CreateDashboardAsync(object dashboardData)
        {
            var json = JsonSerializer.Serialize(dashboardData);
            var response = await PostAsync("/api/dashboard", json);
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        /// <summary>
        /// Add card to dashboard
        /// </summary>
        public async Task<JsonElement> AddCardToDashboardAsync(int dashboardId, object cardData)
        {
            var json = JsonSerializer.Serialize(cardData);
            var response = await PostAsync($"/api/dashboard/{dashboardId}/cards", json);
            return JsonSerializer.Deserialize<JsonElement>(response);
        }

        //private async Task<string> SendRequestAsync(HttpMethod method, string endpoint, string? jsonContent = null)
        //{
        //    if (_apiToken == null)
        //    {
        //        throw new InvalidOperationException("Not authenticated. Please check config.");
        //    }


        //    //var client = new RestClient(_baseUrl);
        //    //var req = new RestRequest("/api/collection", Method.Post);

        //    //// Headers
        //    //req.AddHeader("x-api-key", _apiToken);

        //    //// JSON body
        //    //req.AddJsonBody(new
        //    //{
        //    //    parent_id = 6,
        //    //    authority_level = null,
        //    //    color = "#509EE3",
        //    //    description = null,
        //    //    name = "TestingCollection"
        //    //});

        //    //var resp = await client.ExecuteAsync(req);


        //    var url = $"{_baseUrl}{endpoint}";
        //    _logger.LogDebug($"{method} {url}");

        //    var request = new HttpRequestMessage(method, url);

        //    if (_apiToken != null)
        //    {
        //        request.Headers.Add("x-api-key", _apiToken);
        //    }

        //    if (jsonContent != null)
        //    {
        //        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        //    }


        //    var response = await _httpClient.SendAsync(request);
        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogError($"HTTP {response.StatusCode}: {responseContent}");
        //        throw new HttpRequestException($"Request failed: {response.StatusCode} - {responseContent}");
        //    }

        //    _logger.LogDebug($"Response: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
        //    return responseContent;
        //}

        private async Task<string> SendRequestAsync(HttpMethod method, string endpoint, string? jsonContent = null)
        {
            if (string.IsNullOrWhiteSpace(_apiToken))
                throw new InvalidOperationException("Not authenticated. Please check config.");

            var url = $"{_baseUrl}{endpoint}";
            _logger.LogDebug($"{method} {url}");

            using var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url)
            };

            request.Headers.Add("x-api-key", _apiToken);

            if (!string.IsNullOrEmpty(jsonContent))
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP {response.StatusCode}: {responseContent}");
                throw new HttpRequestException($"Request failed: {response.StatusCode} - {responseContent}");
            }

            _logger.LogDebug($"Response: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            return responseContent;
        }


        /// <summary>
        /// Test connection to Metabase instance
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await GetAsync("/api/health");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Logout and clear session
        /// </summary>
        public async Task LogoutAsync()
        {
            if (_apiToken != null)
            {
                try
                {
                    await DeleteAsync("/api/session");
                    _logger.LogDebug("Logged out successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Logout warning: {ex.Message}");
                }
                finally
                {
                    _apiToken = null;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                LogoutAsync().Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore logout errors during disposal
            }
            finally
            {
                _httpClient?.Dispose();
            }
        }
    }
}