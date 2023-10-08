using log4net;
using System.Text;
using System.Text.Json;

namespace AimRobotLite.service {
    class RequestUtils {

        private static readonly ILog log = LogManager.GetLogger(typeof(RequestUtils)); 

        private static string ToQueryString(Dictionary<string, object> parameters) {
            if (parameters == null || parameters.Count == 0) return string.Empty;

            var queryString = "?" + string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));
            return queryString;
        }

        public static async Task<string> Post(string url, object data) {
            return await Post(url, data, new Dictionary<string, string>());
        }

        public static async Task<string> Post(string url, object data, Dictionary<string, string> headers) {
            string dataString = JsonSerializer.Serialize(data);

            using (HttpClient httpClient = new HttpClient()) {

                foreach (var header in headers) {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                httpClient.Timeout = TimeSpan.FromSeconds(10);

                StringContent content = new StringContent(dataString, Encoding.UTF8, "application/json");

                try {
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode) {
                        return await response.Content.ReadAsStringAsync(); ;
                    } else {
                        return string.Empty;
                    }
                } catch (HttpRequestException ex) {
                    log.Error(ex);
                    return string.Empty;
                }
            }

        }

        public static async Task<string> Get(string url, Dictionary<string, object> parameters) {
            string queryString = ToQueryString(parameters);

            using (HttpClient httpClient = new HttpClient()) {
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                try {
                    HttpResponseMessage response = await httpClient.GetAsync(url + queryString);
                    if (response.IsSuccessStatusCode) {
                        return await response.Content.ReadAsStringAsync();
                    } else {
                        return string.Empty;
                    }
                } catch (HttpRequestException ex) {
                    log.Error($"HttpRequestException {ex}");
                    return string.Empty;
                } catch(TaskCanceledException ex) {
                    log.Error($"TaskCanceledException");
                    return string.Empty;
                }
            }

        }

    }
}
