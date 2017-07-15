﻿using Android;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SimpleWeather.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using Android.Runtime;
using System.Collections.Specialized;
using SimpleWeather.Droid.Controls;
using SimpleWeather.Controls;
using SimpleWeather.Droid.Utils;
using SimpleWeather.Droid.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWeather.Droid
{
    public class LocationSearchFragment : Fragment
    {
        private Location mLocation;
        private RecyclerView mRecyclerView;
        private LocationQueryAdapter mAdapter;
        private RecyclerView.LayoutManager mLayoutManager;

        private LocationListener mLocListnr;

        private const int PERMISSION_LOCATION_REQUEST_CODE = 0;

        private String selected_query = String.Empty;

        private CancellationTokenSource cts;

        public LocationSearchFragment()
        {
            // Required empty public constructor
            ClickListener = LocationSearchFragment_clickListener;
            cts = new CancellationTokenSource();
        }

        public event EventHandler<RecyclerClickEventArgs> ClickListener;

        public void SetClickListener(EventHandler<RecyclerClickEventArgs> listener)
        {
            ClickListener = listener;
        }

        private async void LocationSearchFragment_clickListener(object sender, RecyclerClickEventArgs e)
        {
            // Get selected query view
            LocationQuery v = (LocationQuery)e.View;

            if (!String.IsNullOrEmpty(mAdapter.Dataset[e.Position].LocationQuery))
                selected_query = mAdapter.Dataset[e.Position].LocationQuery;
            else
                selected_query = string.Empty;

            if (String.IsNullOrWhiteSpace(selected_query))
            {
                // Stop since there is no valid query
                return;
            }

            if (String.IsNullOrWhiteSpace(Settings.API_KEY) && Settings.API == Settings.API_WUnderground)
            {
                String errorMsg = new WeatherException(WeatherUtils.ErrorStatus.INVALIDAPIKEY).Message;
                Toast.MakeText(Activity.ApplicationContext, errorMsg, ToastLength.Short).Show();
                return;
            }

            Pair<int, string> pair;
            
            // Get Weather Data
            OrderedDictionary weatherData = await Settings.GetWeatherData();

            WeatherData.Weather weather = await WeatherData.WeatherLoaderTask.GetWeather(selected_query);

            if (weather == null)
                return;

            // Save weather data
            if (weatherData.Contains(selected_query))
                weatherData[selected_query] = weather;
            else
                weatherData.Add(selected_query, weather);
            Settings.SaveWeatherData();

            pair = new Pair<int, string>(App.HomeIdx, selected_query);

            // Start WeatherNow Activity with weather data
            Intent intent = new Intent(Activity, typeof(MainActivity));
            intent.PutExtra("pair", await JSONParser.SerializerAsync(pair, typeof(Pair<int, string>)));

            Settings.WeatherLoaded = true;
            Activity.StartActivity(intent);
            Activity.FinishAffinity();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Inflate the layout for this fragment
            View view = inflater.Inflate(Resource.Layout.fragment_location_search, container, false);
            SetupView(view);

            Activity.Window.SetSoftInputMode(SoftInput.AdjustResize);
            return view;
        }

        public override void OnDetach()
        {
            base.OnDetach();
        }

        private void SetupView(View view)
        {
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_view);

            // Location Listener
            mLocListnr = new LocationListener();
            mLocListnr.LocationChanged += (Location location) =>
            {
                mLocation = location;
                FetchGeoLocation();
            };

            // use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            mRecyclerView.HasFixedSize = true;

            // use a linear layout manager
            mLayoutManager = new LinearLayoutManager(Activity);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            // specify an adapter (see also next example)
            mAdapter = new LocationQueryAdapter(new List<LocationQueryViewModel>());
            mAdapter.ItemClick += ClickListener;
            mRecyclerView.SetAdapter(mAdapter);
        }

        public void FetchLocations(String queryString)
        {
            // Cancel pending searches
            cts.Cancel();
            cts = new CancellationTokenSource();

            // Get locations
            if (!String.IsNullOrWhiteSpace(queryString))
            {
                Task.Run(async () =>
                {
                    if (cts.IsCancellationRequested) return;

                    var results = await WeatherData.AutoCompleteQuery.GetLocations(queryString);

                    if (cts.IsCancellationRequested) return;

                    this.Activity.RunOnUiThread(() => mAdapter.SetLocations(results.ToList()));
                });
            }
            else if (String.IsNullOrWhiteSpace(queryString))
            {
                // Cancel pending searches
                cts.Cancel();
                // Hide flyout if query is empty or null
                mAdapter.Dataset.Clear();
                mAdapter.NotifyDataSetChanged();
            }
        }

        public void FetchGeoLocation()
        {
            // Cancel pending searches
            cts.Cancel();
            cts = new CancellationTokenSource();

            if (mLocation != null)
            {
                Task.Run(async () =>
                {
                    if (cts.IsCancellationRequested) return;

                    // Get geo location
                    LocationQueryViewModel gpsLocation = await WeatherData.GeopositionQuery.GetLocation(mLocation);

                    if (cts.IsCancellationRequested) return;

                    this.Activity.RunOnUiThread(() => mAdapter.SetLocations(new List<LocationQueryViewModel>() { gpsLocation }));
                });
            }
            else
            {
                UpdateLocation();
            }
        }

        private void UpdateLocation()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) != Permission.Granted &&
                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                RequestPermissions(new String[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation },
                        PERMISSION_LOCATION_REQUEST_CODE);
                return;
            }

            LocationManager locMan = (LocationManager)Activity.GetSystemService(Context.LocationService);
            bool isGPSEnabled = locMan.IsProviderEnabled(LocationManager.GpsProvider);
            bool isNetEnabled = locMan.IsProviderEnabled(LocationManager.NetworkProvider);

            Location location = null;

            if (isGPSEnabled)
            {
                location = locMan.GetLastKnownLocation(LocationManager.GpsProvider);

                if (location == null)
                    location = locMan.GetLastKnownLocation(LocationManager.NetworkProvider);

                if (location == null)
                    locMan.RequestSingleUpdate(LocationManager.GpsProvider, mLocListnr, null);
                else
                {
                    mLocation = location;
                    FetchGeoLocation();
                }
            }
            else if (isNetEnabled)
            {
                location = locMan.GetLastKnownLocation(LocationManager.NetworkProvider);

                if (location == null)
                    locMan.RequestSingleUpdate(LocationManager.NetworkProvider, mLocListnr, null);
                else
                {
                    mLocation = location;
                    FetchGeoLocation();
                }
            }
            else
            {
                Toast.MakeText(Activity, "Unable to get location", ToastLength.Short).Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case PERMISSION_LOCATION_REQUEST_CODE:
                {
                    // If request is cancelled, the result arrays are empty.
                    if (grantResults.Length > 0
                            && grantResults[0] == Permission.Granted)
                    {

                        // permission was granted, yay!
                        // Do the task you need to do.
                        FetchGeoLocation();
                    }
                    else
                    {
                        // permission denied, boo! Disable the
                        // functionality that depends on this permission.
                        Toast.MakeText(Activity, "Location access denied", ToastLength.Short).Show();
                    }
                    return;
                }
            }
        }
    }
}