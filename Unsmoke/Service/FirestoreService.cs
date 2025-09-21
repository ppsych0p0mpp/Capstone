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
using Google.Cloud.Firestore;
using System.Net;

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
                    UserId = "0",
                    DateCreated = DateTime.UtcNow
                };

                // parse UserId (integerValue)
                var userIdToken = fields?["UserId"]?["integerValue"];
                if (userIdToken != null && int.TryParse(userIdToken.ToString(), out var uid))
                    post.UserId = uid.ToString();

                // parse DateCreated (timestampValue)
                var tsToken = fields?["DateCreated"]?["timestampValue"];
                if (tsToken != null && DateTime.TryParse(tsToken.ToString(), null, DateTimeStyles.RoundtripKind, out var dt))
                    post.DateCreated = dt;

                posts.Add(post);
            }

            return posts.OrderByDescending(p => p.DateCreated).ToList();
        }

        public async Task<List<T>> QueryDocumentsAsync<T>(string collectionName, string fieldName, object value) where T : new()
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents:runQuery?key={_apiKey}";

            // Firestore structured query
            var query = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = collectionName } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = fieldName },
                            op = "EQUAL",
                            value = new { stringValue = value.ToString() }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(query);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var resultArray = JArray.Parse(resultJson);

            var list = new List<T>();

            foreach (var item in resultArray)
            {
                var doc = item["document"];
                if (doc == null) continue;

                var fields = doc["fields"];
                if (fields == null) continue;

                var obj = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (fields[prop.Name] == null) continue;
                    var fieldType = fields[prop.Name].First as JProperty;
                    var fieldValue = fieldType?.Value?.ToString();

                    if (prop.PropertyType == typeof(int) && int.TryParse(fieldValue, out var intVal))
                        prop.SetValue(obj, intVal);
                    else if (prop.PropertyType == typeof(double) && double.TryParse(fieldValue, out var dblVal))
                        prop.SetValue(obj, dblVal);
                    else if (prop.PropertyType == typeof(bool) && bool.TryParse(fieldValue, out var boolVal))
                        prop.SetValue(obj, boolVal);
                    else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(fieldValue, out var dtVal))
                        prop.SetValue(obj, dtVal);
                    else
                        prop.SetValue(obj, fieldValue);
                }

                list.Add(obj);
            }

            return list;
        }

        private string GetCollectionUrl(string collection) =>
            $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?key={_apiKey}";

        public async Task CreateOrUpdateDocumentAsync<T>(string collection, string documentId, T data)
        {
            var docUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}/{documentId}?key={_apiKey}";

            // Convert object to Firestore format
            var firestoreObj = new JObject { ["fields"] = ConvertToFirestoreFields(data) };
            var json = firestoreObj.ToString(Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Check if document exists
            var getResponse = await _httpClient.GetAsync(docUrl);

            if (getResponse.IsSuccessStatusCode)
            {
                // Document exists → Update using PATCH with updateMask for partial update
                var patchUrl = $"{docUrl}&updateMask.fieldPaths=*"; // update all fields
                var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), patchUrl)
                {
                    Content = content
                };

                var patchResponse = await _httpClient.SendAsync(patchRequest);
                patchResponse.EnsureSuccessStatusCode();
            }
            else if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Document doesn't exist → Create using POST with documentId
                var createUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?documentId={WebUtility.UrlEncode(documentId)}&key={_apiKey}";
                var postResponse = await _httpClient.PostAsync(createUrl, content);
                postResponse.EnsureSuccessStatusCode();
            }
            else
            {
                // Any other error
                getResponse.EnsureSuccessStatusCode();
            }
        }

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

        public async Task SetDocumentAsync<T>(string collection, string documentId, T data)
        {
            // Ensure time/timeSpan conversions are already done on the object (e.g., TimewithoutCigSeconds)
            var docUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}/{documentId}?key={_apiKey}";
            var firestoreObj = new JObject { ["fields"] = ConvertToFirestoreFields(data) };
            var json = firestoreObj.ToString(Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Check if document exists
            var getResponse = await _httpClient.GetAsync(docUrl);

            if (getResponse.IsSuccessStatusCode)
            {
                // Document exists -> update via PATCH
                var patchResponse = await _httpClient.PatchAsync(docUrl, content);
                patchResponse.EnsureSuccessStatusCode();
                return;
            }

            // If not found -> create with documentId using createDocument endpoint (POST with documentId query)
            if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // collection create URL already includes ?key=... so we append &documentId=
                var createUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?documentId={WebUtility.UrlEncode(documentId)}&key={_apiKey}";
                var postResponse = await _httpClient.PostAsync(createUrl, content);
                postResponse.EnsureSuccessStatusCode();
                return;
            }

            // Any other error -> throw
            getResponse.EnsureSuccessStatusCode();
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
        public async Task<string> AddDocumentWithIdAsync<T>(string collection, T data)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collection}?key={_apiKey}";

            // Convert object to Firestore format
            var firestoreObj = new JObject
            {
                ["fields"] = ConvertToFirestoreFields(data)
            };

            var json = firestoreObj.ToString(Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send request
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // Parse response to get document name
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseString);

            // Example of response "name": "projects/.../databases/(default)/documents/Users/abcd123"
            var fullName = responseJson["name"]?.ToString();
            if (string.IsNullOrEmpty(fullName))
                throw new Exception("Failed to get Firestore document ID.");

            // Extract only the document ID
            var parts = fullName.Split('/');
            var docId = parts.Length > 0 ? parts[^1] : fullName;

            return docId; // This is your Firestore document ID
        }
        private T ConvertFromFirestoreFields<T>(JToken fields) where T : new()
        {
            var item = new T();

            if (fields == null) return item;

            foreach (var prop in typeof(T).GetProperties())
            {
                try
                {
                    var fieldToken = fields[prop.Name];
                    if (fieldToken == null) continue;

                    // Firestore field tokens look like: { "stringValue": "..." } or { "integerValue":"1" }, etc.
                    // Check known Firestore value types:
                    if (fieldToken["stringValue"] != null)
                    {
                        var s = fieldToken["stringValue"].ToString();
                        if (prop.PropertyType == typeof(string))
                            prop.SetValue(item, s);
                        else if (prop.PropertyType == typeof(int) && int.TryParse(s, out var i)) prop.SetValue(item, i);
                        else if (prop.PropertyType == typeof(double) && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) prop.SetValue(item, d);
                        else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt)) prop.SetValue(item, dt);
                        else if (prop.PropertyType == typeof(bool) && bool.TryParse(s, out var b)) prop.SetValue(item, b);
                        else prop.SetValue(item, Convert.ChangeType(s, prop.PropertyType));
                    }
                    else if (fieldToken["integerValue"] != null)
                    {
                        var s = fieldToken["integerValue"].ToString();
                        if (prop.PropertyType == typeof(int) && int.TryParse(s, out var i)) prop.SetValue(item, i);
                        else if (prop.PropertyType == typeof(long) && long.TryParse(s, out var l)) prop.SetValue(item, l);
                        else if (prop.PropertyType == typeof(double) && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) prop.SetValue(item, d);
                        else if (prop.PropertyType == typeof(string)) prop.SetValue(item, s);
                    }
                    else if (fieldToken["doubleValue"] != null)
                    {
                        var s = fieldToken["doubleValue"].ToString();
                        if (prop.PropertyType == typeof(double) && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) prop.SetValue(item, d);
                        else if (prop.PropertyType == typeof(string)) prop.SetValue(item, s);
                    }
                    else if (fieldToken["booleanValue"] != null)
                    {
                        var s = fieldToken["booleanValue"].ToString();
                        if (prop.PropertyType == typeof(bool) && bool.TryParse(s, out var b)) prop.SetValue(item, b);
                        else if (prop.PropertyType == typeof(string)) prop.SetValue(item, s);
                    }
                    else if (fieldToken["timestampValue"] != null)
                    {
                        var s = fieldToken["timestampValue"].ToString();
                        if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt)) prop.SetValue(item, dt);
                        else if (prop.PropertyType == typeof(string)) prop.SetValue(item, s);
                    }
                    else if (fieldToken["nullValue"] != null)
                    {
                        prop.SetValue(item, null);
                    }
                    else
                    {
                        // fallback: try the first primitive inside token
                        var first = fieldToken.Children<JProperty>().FirstOrDefault()?.Value?.ToString();
                        if (first != null)
                        {
                            try { prop.SetValue(item, Convert.ChangeType(first, prop.PropertyType)); } catch { /* ignore */ }
                        }
                    }

                    // Special support: if property is TimeSpan and Firestore stores seconds as integer/double
                    if ((prop.PropertyType == typeof(TimeSpan) || prop.PropertyType == typeof(TimeSpan?)) &&
                        (fieldToken["integerValue"] != null || fieldToken["doubleValue"] != null))
                    {
                        double seconds = 0;
                        if (fieldToken["integerValue"] != null) double.TryParse(fieldToken["integerValue"].ToString(), out seconds);
                        else if (fieldToken["doubleValue"] != null) double.TryParse(fieldToken["doubleValue"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out seconds);

                        prop.SetValue(item, TimeSpan.FromSeconds(seconds));
                    }
                }
                catch
                {
                    // ignore per-field conversion errors to avoid breaking whole parsing
                }
            }

            return item;
        }

        public async Task<List<T>> GetDocumentsWithIdAsync<T>(string collectionName) where T : new()
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{collectionName}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(json);

            var result = new List<T>();
            var documents = root["documents"] as JArray;
            if (documents == null) return result;

            foreach (var doc in documents)
            {
                var fields = doc["fields"];
                var obj = ConvertFromFirestoreFields<T>(fields);

                // extract document id from "name"
                var name = doc["name"]?.ToString();
                var docId = name?.Split('/').Last();

                // set common id properties if present (Id, UserID or UserId)
                TrySetProperty(obj, "Id", docId);
                TrySetProperty(obj, "UserID", docId);
                TrySetProperty(obj, "UserId", docId);

                result.Add(obj);
            }

            return result;
        }

        private void TrySetProperty<T>(T obj, string propName, string value)
        {
            var prop = typeof(T).GetProperty(propName);
            if (prop == null || string.IsNullOrEmpty(value)) return;

            try
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(obj, value);
                else if (prop.PropertyType == typeof(int) && int.TryParse(value, out var i))
                    prop.SetValue(obj, i);
                else if (prop.PropertyType == typeof(long) && long.TryParse(value, out var l))
                    prop.SetValue(obj, l);
                else if (prop.PropertyType == typeof(Guid) && Guid.TryParse(value, out var g))
                    prop.SetValue(obj, g);
                else
                {
                    // attempt ChangeType fallback
                    var converted = Convert.ChangeType(value, prop.PropertyType, CultureInfo.InvariantCulture);
                    prop.SetValue(obj, converted);
                }
            }
            catch
            {
                // ignore conversion errors
            }
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
