﻿using SimpleWeather.Controls;
using SimpleWeather.Location;
using SimpleWeather.Utils;
using SimpleWeather.WeatherData;
using SimpleWeather.WeatherUnderground;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Web;
using Windows.Web.Http;

namespace SimpleWeather.OpenWeather
{
    public class OWMWULocationProvider : LocationProviderImpl
    {
        public override string LocationAPI => WeatherAPI.WeatherUnderground;
        public override bool SupportsWeatherLocale => false;
        public override bool KeyRequired => false;

        /// <exception cref="WeatherException">Thrown when task is unable to retrieve data</exception>
        public override async Task<ObservableCollection<LocationQueryViewModel>> GetLocations(string query, string weatherAPI)
        {
            ObservableCollection<LocationQueryViewModel> locations = null;

            string queryAPI = "https://autocomplete.wunderground.com/aq?query=";
            string options = "&h=0&cities=1";
            Uri queryURL = new Uri(queryAPI + query + options);
            WeatherException wEx = null;
            // Limit amount of results shown
            int maxResults = 10;

            using (HttpClient webClient = new HttpClient())
            using (var cts = new CancellationTokenSource(Settings.READ_TIMEOUT))
            {
                try
                {
                    // Connect to webstream
                    HttpResponseMessage response = await webClient.GetAsync(queryURL).AsTask(cts.Token);
                    response.EnsureSuccessStatusCode();
                    Stream contentStream = WindowsRuntimeStreamExtensions.AsStreamForRead(await response.Content.ReadAsInputStreamAsync());
                    // End Stream
                    webClient.Dispose();

                    // Load data
                    locations = new ObservableCollection<LocationQueryViewModel>();

                    var root = JSONParser.Deserializer<AC_Rootobject>(contentStream);

                    foreach (AC_RESULT result in root.RESULTS)
                    {
                        // Filter: only store city results
                        if (result.type != "city")
                            continue;

                        locations.Add(new LocationQueryViewModel(result, weatherAPI));

                        // Limit amount of results
                        maxResults--;
                        if (maxResults <= 0)
                            break;
                    }

                    // End Stream
                    contentStream.Dispose();
                }
                catch (Exception ex)
                {
                    if (WebError.GetStatus(ex.HResult) > WebErrorStatus.Unknown)
                    {
                        wEx = new WeatherException(WeatherUtils.ErrorStatus.NetworkError);
                    }
                    Logger.WriteLine(LoggerLevel.Error, ex, "OpenWeatherMapProvider: error getting locations");
                }
            }

            if (wEx != null)
                throw wEx;

            if (locations == null || locations.Count == 0)
                locations = new ObservableCollection<LocationQueryViewModel>() { new LocationQueryViewModel() };

            return locations;
        }

        /// <exception cref="WeatherException">Thrown when task is unable to retrieve data</exception>
        public override async Task<LocationQueryViewModel> GetLocation(WeatherUtils.Coordinate coord, string weatherAPI)
        {
            LocationQueryViewModel location = null;

            string queryAPI = "https://api.wunderground.com/auto/wui/geo/GeoLookupXML/index.xml?query=";
            string options = "";
            string query = string.Format("{0},{1}", coord.Latitude, coord.Longitude);
            Uri queryURL = new Uri(queryAPI + query + options);
            location result;
            WeatherException wEx = null;

            using (HttpClient webClient = new HttpClient())
            using (var cts = new CancellationTokenSource(Settings.READ_TIMEOUT))
            {
                try
                {
                    // Connect to webstream
                    HttpResponseMessage response = await webClient.GetAsync(queryURL).AsTask(cts.Token);
                    response.EnsureSuccessStatusCode();
                    Stream contentStream = WindowsRuntimeStreamExtensions.AsStreamForRead(await response.Content.ReadAsInputStreamAsync());

                    // End Stream
                    webClient.Dispose();

                    // Load data
                    XmlSerializer deserializer = new XmlSerializer(typeof(location));
                    result = (location)deserializer.Deserialize(contentStream);

                    // End Stream
                    contentStream?.Dispose();
                }
                catch (Exception ex)
                {
                    result = null;
                    if (WebError.GetStatus(ex.HResult) > WebErrorStatus.Unknown)
                    {
                        wEx = new WeatherException(WeatherUtils.ErrorStatus.NetworkError);
                    }
                    Logger.WriteLine(LoggerLevel.Error, ex, "OpenWeatherMapProvider: error getting location");
                }
            }

            if (wEx != null)
                throw wEx;

            if (result != null && !String.IsNullOrWhiteSpace(result.query))
                location = new LocationQueryViewModel(result, weatherAPI);
            else
                location = new LocationQueryViewModel();

            return location;
        }

        public override Task<bool> IsKeyValid(string key)
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(false);
            return tcs.Task;
        }

        public override string GetAPIKey()
        {
            return null;
        }
    }
}