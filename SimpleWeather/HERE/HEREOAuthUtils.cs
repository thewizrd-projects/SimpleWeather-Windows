﻿using SimpleWeather.Keys;
using SimpleWeather.Utils;
using SimpleWeather.WeatherData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleWeather.HERE
{
    public partial class HEREOAuthUtils
    {
        public const string HERE_OAUTH_URL = "https://account.api.here.com/oauth2/token";
        private const string KEY_TOKEN = "token";

        public static async Task<String> GetBearerToken(bool forceRefresh = false)
        {
            if (!forceRefresh)
            {
                var token = await AsyncTask.RunAsync(GetTokenFromStorage);
                if (token != null)
                    return token;
                else
                    forceRefresh = true;
            }

            if (forceRefresh)
            {
                Stream contentStream = null;
                var oAuthRequest = new OAuthRequest(APIKeys.GetHERECliID(), APIKeys.GetHERECliSecr(), OAuthSignatureMethod.HMAC_SHA256, HTTPRequestType.POST);

                using (HttpClient webClient = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, HERE_OAUTH_URL))
                using (var cts = new CancellationTokenSource(Settings.READ_TIMEOUT))
                {
                    try
                    {
                        // Add headers to request
                        var authHeader = oAuthRequest.GetAuthorizationHeader(request.RequestUri, true);
                        request.Headers.Add("Authorization", authHeader);
                        request.Headers.CacheControl = new CacheControlHeaderValue()
                        {
                            NoCache = true
                        };

                        // Connect to webstream
                        var contentList = new List<KeyValuePair<string, string>>(1)
                        {
                            new KeyValuePair<string, string>("grant_type", "client_credentials")
                        };
                        request.Content = new FormUrlEncodedContent(contentList);

                        HttpResponseMessage response = await AsyncTask.RunAsync(webClient.SendAsync(request, cts.Token));
                        response.EnsureSuccessStatusCode();
                        contentStream = await AsyncTask.RunAsync(response.Content.ReadAsStreamAsync());

                        var date = response.Headers.Date.GetValueOrDefault(DateTimeOffset.UtcNow);

                        var tokenRoot = await AsyncTask.RunAsync(() =>
                        {
                            return JSONParser.Deserializer<TokenRootobject>(contentStream);
                        });

                        if (tokenRoot != null)
                        {
                            var tokenStr = String.Format(CultureInfo.InvariantCulture, "Bearer {0}", tokenRoot.access_token);

                            // Store token for future operations
                            var token = new Token()
                            {
                                expiration_date = date.UtcDateTime.AddSeconds(tokenRoot.expires_in),
                                access_token = tokenStr
                            };

                            StoreToken(token);

                            return tokenStr;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(LoggerLevel.Error, e, "HEREOAuthUtils: Error retrieving token");
                    }
                }
            }

            return null;
        }

        private static async Task<String> GetTokenFromStorage()
        {
            // Shared Settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var HERESettingsContainer = localSettings.CreateContainer(WeatherAPI.Here, ApplicationDataCreateDisposition.Always);

            if (HERESettingsContainer.Values.ContainsKey(KEY_TOKEN))
            {
                var tokenJSON = HERESettingsContainer.Values[KEY_TOKEN]?.ToString();
                if (tokenJSON != null)
                {
                    var token = await AsyncTask.RunAsync(() =>
                    {
                        return JSONParser.Deserializer<Token>(tokenJSON);
                    });

                    // Add buffer before expiration to avoid any auth issues
                    if (token != null && token.expiration_date.AddMinutes(-1.5) > DateTime.UtcNow)
                    {
                        return token.access_token;
                    }
                }
            }

            return null;
        }

        private static void StoreToken(Token token)
        {
            // Shared Settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var HERESettingsContainer = localSettings.CreateContainer(WeatherAPI.Here, ApplicationDataCreateDisposition.Always);

            AsyncTask.Run(() =>
            {
                HERESettingsContainer.Values[KEY_TOKEN] = JSONParser.Serializer(token, typeof(Token));
            });
        }
    }

    public class TokenRootobject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    internal class Token
    {
        public String access_token { get; set; }
        public DateTime expiration_date { get; set; }
    }
}