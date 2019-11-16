﻿using SimpleWeather.Controls;
using SimpleWeather.UWP.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleWeather.UWP.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WeatherAlertPage : Page, ICommandBarPage, IBackRequestedPage
    {
        public WeatherNowViewModel WeatherView { get; set; }

        public string CommandBarLabel { get; set; }
        public List<ICommandBarElement> PrimaryCommands { get; set; }

        public WeatherAlertPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            // CommandBar
            CommandBarLabel = App.ResLoader.GetString("Label_WeatherAlerts/Text");
        }

        public Task<bool> OnBackRequested()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                tcs.SetResult(true);
                return tcs.Task;
            }

            tcs.SetResult(false);
            return tcs.Task;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeatherView = e?.Parameter as WeatherNowViewModel;
        }
    }
}