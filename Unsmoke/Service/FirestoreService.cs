using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Unsmoke.MVVM.Models;

namespace Unsmoke.Service
{
    public class FirestoreService
    {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;
        private readonly string _apiKey;

        public FirestoreService(string projectId, string apiKey)
        {
            _httpClient = new HttpClient();
            _projectId = projectId;
            _apiKey = apiKey;
        }

        // Add a new Post document
        public async Task AddPostAsync(Post post)
        {
            post.DateCreated = DateTime.UtcNow; // Set creation time
            await AddDocumentAsync("Posts", post); // "Posts" is your Firestore collection name
        }

        // ... inside FirestoreService class
        public async Task<List<Post>> GetPostsAsync()
        {
            var json = await GetDocumentsAsync("Posts");
            var root = JObject.Parse(json);
            var posts = new List<Post>();

            // documents may be absent if collection is empty
            var documents = root["documents"] as JArray;
            if (documents == null)
                return posts;

            foreach (var doc in documents)
            {
                // safe extraction of document id from "name" (projects/.../documents/posts/{documentId})
                string id = null;
                var nameToken = doc["name"];
                if (nameToken != null)
                {
                    var fullName = nameToken.ToString();
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        var parts = fullName.Split('/');
                        id = parts.Length > 0 ? parts[parts.Length - 1] : fullName;
                    }
                }

                var fields = doc["fields"];
                var post = new Post
                {
                    Id = id,
                    Tags = fields?["Tags"]?["stringValue"]?.ToString(),
                    Content = fields?["Content"]?["stringValue"]?.ToString(),
                    UserId = 0,
                    DateCreated = DateTime.UtcNow
                };

                // parse UserId (integerValue)
                var userIdToken = fields?["UserId"]?["integerValue"];
                if (userIdToken != null && int.TryParse(userIdToken.ToString(), out var uid))
                    post.UserId = uid;

                // parse DateCreated (timestampValue)
                var tsToken = fields?["DateCreated"]?["timestampValue"];
                if (tsToken != null && DateTime.TryParse(tsToken.ToString(), null, DateTimeStyles.RoundtripKind, out var dt))
                    post.DateCreated = dt;

                posts.Add(post);
            }

            return posts.OrderByDescending(p => p.DateCreated).ToList();
        }

        private string GetCollectionUrl(string collection) =>
            $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?key={_apiKey}";

        // Create Document (auto-generated document id)
        public async Task<string> AddDocumentAsync(string collection, object data)
        {
            var url = GetCollectionUrl(collection);

            // Build Firestore formatted JSON: { "fields": { "FieldName": { "stringValue": "..." }, ... } }
            var firestoreObj = new JObject();
            firestoreObj["fields"] = ConvertToFirestoreFields(data);

            var json = firestoreObj.ToString(Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(); // caller can parse the response
        }

        // Read Documents
        public async Task<string> GetDocumentsAsync(string collection)
        {
            var url = GetCollectionUrl(collection);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<T?> GetDocumentByIdAsync<T>(string collection, string docId) where T : new()
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}/{docId}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);
            var fields = root["fields"];
            if (fields == null) return default;

            var item = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (fields[prop.Name] == null) continue;
                var fieldType = fields[prop.Name].First as JProperty;
                var fieldValue = fieldType?.Value?.ToString();

                if (prop.PropertyType == typeof(int) && int.TryParse(fieldValue, out var intVal))
                    prop.SetValue(item, intVal);
                else if (prop.PropertyType == typeof(double) && double.TryParse(fieldValue, out var dblVal))
                    prop.SetValue(item, dblVal);
                else if (prop.PropertyType == typeof(bool) && bool.TryParse(fieldValue, out var boolVal))
                    prop.SetValue(item, boolVal);
                else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(fieldValue, out var dtVal))
                    prop.SetValue(item, dtVal);
                else
                    prop.SetValue(item, fieldValue);
            }

            return item;
        }

        public async Task<List<T>> GetDocumentsAsync<T>(string collection) where T : new()
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<T>();

            var json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);
            var documents = root["documents"] as JArray;
            var result = new List<T>();

            if (documents == null) return result;

            foreach (var doc in documents)
            {
                var fields = doc["fields"];
                if (fields == null) continue;

                var item = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (fields[prop.Name] == null) continue;

                    var fieldType = fields[prop.Name].First as JProperty;
                    var fieldValue = fieldType?.Value?.ToString();

                    if (prop.PropertyType == typeof(int) && int.TryParse(fieldValue, out var intVal))
                        prop.SetValue(item, intVal);
                    else if (prop.PropertyType == typeof(double) && double.TryParse(fieldValue, out var dblVal))
                        prop.SetValue(item, dblVal);
                    else if (prop.PropertyType == typeof(bool) && bool.TryParse(fieldValue, out var boolVal))
                        prop.SetValue(item, boolVal);
                    else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(fieldValue, out var dtVal))
                        prop.SetValue(item, dtVal);
                    else
                        prop.SetValue(item, fieldValue);
                }

                result.Add(item);
            }

            return result;
        }

        // Update Document
        public async Task<string> UpdateDocumentAsync(string collection, string docId, object data)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}/{docId}?key={_apiKey}";

            var firestoreObj = new JObject();
            firestoreObj["fields"] = ConvertToFirestoreFields(data);

            var json = firestoreObj.ToString(Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        // Delete Document
        public async Task DeleteDocumentAsync(string collection, string docId)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}/{docId}?key={_apiKey}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
        // Convert object properties to Firestore field format (JObject)
        private JObject ConvertToFirestoreFields(object data)
        {
            var fields = new JObject();
            var props = data.GetType().GetProperties();

            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(data);

                if (value == null)
                {
                    fields[name] = new JObject(new JProperty("nullValue", null));
                    continue;
                }

                switch (value)
                {
                    case int i:
                    case long l:
                        fields[name] = new JObject(new JProperty("integerValue", value.ToString()));
                        break;

                    case double d:
                    case float f:
                    case decimal dec:
                        // Firestore expects double as a number (use invariant culture for decimal point)
                        fields[name] = new JObject(new JProperty("doubleValue", Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture)));
                        break;

                    case bool b:
                        fields[name] = new JObject(new JProperty("booleanValue", b));
                        break;

                    case DateTime dt:
                        // Use RFC3339 / ISO 8601
                        fields[name] = new JObject(new JProperty("timestampValue", dt.ToString("o")));
                        break;

                    default:
                        // Fallback to string for other types
                        fields[name] = new JObject(new JProperty("stringValue", value.ToString()));
                        break;
                }
            }

            return fields;
        }
    }


    // PATCH extension
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return await client.SendAsync(request);
        }
    }


}
