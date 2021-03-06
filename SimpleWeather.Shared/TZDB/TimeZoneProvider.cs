﻿using SimpleWeather.Keys;
using SimpleWeather.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static SimpleWeather.Utils.APIRequestUtils;

namespace SimpleWeather.TZDB
{
    internal class TimeZoneData
    {
        [JsonPropertyName("tz_long")]
        public String TZLong { get; set; }
    }

    public class TimeZoneProvider : ITimeZoneProvider, IRateLimitedRequest
    {
        private const string API_ID = "tzdb";

        public long GetRetryTime() => 60000;

        public async Task<string> GetTimeZone(double latitude, double longitude)
        {
            String tzLong = null;

            try
            {
                // Get Firebase token
                var authLink = await Firebase.FirebaseAuthHelper.GetAuthLink();
                string userToken = authLink.FirebaseToken;
                string tzAPI = APIKeys.GetTimeZoneAPI();
                if (String.IsNullOrWhiteSpace(tzAPI) || String.IsNullOrWhiteSpace(userToken))
                    return null;

                CheckRateLimit(API_ID);

                Uri queryURL = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}?lat={1:0.####}&lon={2:0.####}", tzAPI, latitude, longitude));

                using (var request = new HttpRequestMessage(HttpMethod.Get, queryURL))
                {
                    // Add headers to request
                    request.Headers.Authorization = new HttpCredentialsHeaderValue("Bearer", userToken);
                    request.Headers.CacheControl.MaxAge = TimeSpan.FromDays(1);

                    // Connect to webstream
                    var webClient = SimpleLibrary.GetInstance().WebClient;
                    using (var cts = new CancellationTokenSource(Settings.READ_TIMEOUT))
                    using (var response = await webClient.SendRequestAsync(request).AsTask(cts.Token))
                    {
                        CheckForErrors(API_ID, response.StatusCode);
                        response.EnsureSuccessStatusCode();

                        Stream contentStream = WindowsRuntimeStreamExtensions.AsStreamForRead(await response.Content.ReadAsInputStreamAsync());

                        // Load weather
                        var root = await JsonSerializer.DeserializeAsync<TimeZoneData>(contentStream);

                        tzLong = root.TZLong;
                    }
                }
            }
            catch (Exception ex)
            {
                tzLong = null;
                Logger.WriteLine(LoggerLevel.Error, ex, "TimeZoneProvider: error getting time zone data");
            }

            return tzLong;
        }
    }
}
