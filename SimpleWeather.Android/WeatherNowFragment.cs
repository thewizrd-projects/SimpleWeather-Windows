﻿using System;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using SimpleWeather.WeatherData;
using SimpleWeather.Droid.Controls;
using Android.Support.V7.App;
using SimpleWeather.Controls;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using SimpleWeather.Droid.Utils;
using SimpleWeather.Utils;
using System.Collections.Generic;

namespace SimpleWeather.Droid
{
    public class WeatherNowFragment : Fragment, WeatherLoadedListener
    {
        private Context context;
        private Pair<int, string> pair;

        WeatherDataLoader wLoader = null;
        WeatherNowView weatherView = null;

        // Views
        private View contentView;
        private ProgressBar progressBar;
        // Condition
        private TextView locationName;
        private TextView updateTime;
        private WeatherIcon weatherIcon;
        private TextView weatherCondition;
        private WeatherIcon weatherTemp;
        // Details
        private LinearLayout detailsPanel;
        private TextView humidity;
        private WeatherIcon pressureState;
        private TextView pressure;
        private TextView visiblity;
        private TextView feelslike;
        private WeatherIcon windDirection;
        private TextView windSpeed;
        private TextView sunrise;
        private TextView sunset;

        private String fahrenheit = App.Context.GetString(Resource.String.wi_fahrenheit);
        private String celsius = App.Context.GetString(Resource.String.wi_celsius);

        public void onWeatherLoaded(int locationIdx, Weather weather)
        {
            if (weather != null)
            {
                if (weatherView == null)
                    weatherView = new WeatherNowView(weather);
                else
                    weatherView.updateView(weather);

                this.SetView(weatherView);
            }

            ShowLoadingView(false);
        }

        public WeatherNowFragment()
        {
            // Required empty public constructor
        }

        /**
         * Use this factory method to create a new instance of
         * this fragment using the provided parameters.
         *
         * @param query Weather query.
         * @param index Location index.
         * @return A new instance of fragment WeatherNowFragment.
         */
        public static WeatherNowFragment NewInstance(Pair<int, string> pair)
        {
            WeatherNowFragment fragment = new WeatherNowFragment();
            Bundle args = new Bundle();
            args.PutString("pair", JSONParser.Serializer(pair, typeof(Pair<int, string>)));
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            if (Arguments != null)
            {
                pair = JSONParser.Deserializer(Arguments.GetString("pair"), typeof(Pair<int, string>)) as Pair<int, string>;

                if (pair != null && wLoader == null)
                    wLoader = new WeatherDataLoader(this, pair.Value, pair.Key);
            }

            context = Activity.ApplicationContext;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Inflate the layout for this fragment
            View view = inflater.Inflate(Resource.Layout.fragment_weather_now, container, false);

            // Setup ActionBar
            HasOptionsMenu = true;

            contentView = view.FindViewById(Resource.Id.content_view);
            progressBar = (ProgressBar)view.FindViewById(Resource.Id.progressBar);
            // Condition
            locationName = (TextView)view.FindViewById(Resource.Id.label_location_name);
            updateTime = (TextView)view.FindViewById(Resource.Id.label_updatetime);
            weatherIcon = (WeatherIcon)view.FindViewById(Resource.Id.weather_icon);
            weatherCondition = (TextView)view.FindViewById(Resource.Id.weather_condition);
            weatherTemp = (WeatherIcon)view.FindViewById(Resource.Id.weather_temp);
            // Details
            detailsPanel = (LinearLayout)view.FindViewById(Resource.Id.details_panel);
            humidity = (TextView)view.FindViewById(Resource.Id.humidity);
            pressureState = (WeatherIcon)view.FindViewById(Resource.Id.pressure_state);
            pressure = (TextView)view.FindViewById(Resource.Id.pressure);
            visiblity = (TextView)view.FindViewById(Resource.Id.visibility_val);
            feelslike = (TextView)view.FindViewById(Resource.Id.feelslike);
            windDirection = (WeatherIcon)view.FindViewById(Resource.Id.wind_direction);
            windSpeed = (TextView)view.FindViewById(Resource.Id.wind_speed);
            sunrise = (TextView)view.FindViewById(Resource.Id.sunrise_time);
            sunset = (TextView)view.FindViewById(Resource.Id.sunset_time);

            view.Post(() => Restore());

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            menu.Clear();
            inflater.Inflate(Resource.Menu.weather_now, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.
            int id = item.ItemId;

            //noinspection SimplifiableIfStatement
            if (id == Resource.Id.action_refresh)
            {
                RefreshWeather(true);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnResume()
        {
            base.OnResume();

            // Update view on resume
            // ex. If temperature unit changed
            if (wLoader != null)
            {
                if (wLoader.getWeather() != null)
                {
                    weatherView.updateView(wLoader.getWeather());
                    SetView(weatherView);
                }
            }

            // Title
            AppCompatActivity activity = (AppCompatActivity)Activity;
            activity.SupportActionBar.Title = GetString(Resource.String.title_activity_weather_now);
        }

        private async void Restore()
        {
            if (wLoader == null)
            {
                // Weather was loaded before. Lets load it up...
                List<string> locations = await Settings.getLocations();
                string local = locations[App.HomeIdx];

                wLoader = new WeatherDataLoader(this, local, App.HomeIdx);
            }

            // Load up weather data
            RefreshWeather(false);
        }

        private async void RefreshWeather(bool forceRefresh)
        {
            // Hide view until weather is loaded
            ShowLoadingView(true);

            await wLoader.loadWeatherData(forceRefresh);
        }

        private void ShowLoadingView(bool show)
        {
            contentView.Visibility = show ? ViewStates.Gone : ViewStates.Visible;
            progressBar.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SetView(WeatherNowView weatherView)
        {
            // Background
            if (View != null && View.Width > 0)
            {
                View.Background = new BitmapDrawable(this.Activity.Resources, ThumbnailUtils.ExtractThumbnail(
                    weatherView.Background, View.Width, View.Height, ThumnailExtractOptions.None));
            }

            LinearLayout forecastPanel = (LinearLayout)contentView.FindViewById(Resource.Id.forecast_panel);
            forecastPanel.SetBackgroundColor(weatherView.PanelBackground);
            detailsPanel.SetBackgroundColor(weatherView.PanelBackground);

            // Location
            locationName.Text = weatherView.Location;

            // Date Updated
            updateTime.Text = weatherView.UpdateDate;

            // Update Current Condition
            weatherTemp.Text = weatherView.CurTemp;
            weatherCondition.Text = weatherView.CurCondition;
            weatherIcon.Text = weatherView.WeatherIcon;

            // WeatherDetails
            // Astronomy
            sunrise.Text = weatherView.Sunrise;
            sunset.Text = weatherView.Sunset;

            // Wind
            feelslike.Text = weatherView.WindChill;
            windSpeed.Text = weatherView.WindSpeed;
            windDirection.Rotation = weatherView.WindDirection;

            // Atmosphere
            humidity.Text = weatherView.Humidity;
            pressure.Text = weatherView.Pressure;

            pressureState.Visibility = weatherView.RisingVisiblity;
            pressureState.Text = weatherView.RisingIcon;

            visiblity.Text = weatherView._Visibility;

            // Add UI elements
            forecastPanel.RemoveAllViews();
            foreach (ForecastItemView forecast in weatherView.Forecasts)
            {
                forecastPanel.AddView(new ForecastItem(Activity, forecast));
            }
        }
    }
}