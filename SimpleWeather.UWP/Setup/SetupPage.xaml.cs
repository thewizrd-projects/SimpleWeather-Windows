﻿using SimpleWeather.Location;
using SimpleWeather.Utils;
using SimpleWeather.UWP.Helpers;
using SimpleWeather.UWP.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace SimpleWeather.UWP.Setup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupPage : Page
    {
        public Frame AppFrame { get { return FrameContent; } }
        public static SetupPage Instance { get; set; }
        public LocationData Location { get; set; }

        private List<Type> Pages = new List<Type>()
        {
            typeof(SetupWelcomePage),
            typeof(SetupLocationsPage),
            typeof(SetupSettingsPage)
        };

        private int PageIdx = 0;

        public SetupPage()
        {
            this.InitializeComponent();

            Instance = this;
            AppFrame.CacheSize = 1;
            AppFrame.Navigated += AppFrame_Navigated;
            BackBtn.Click += BackBtn_Click;
            NextBtn.Click += NextBtn_Click;

            // Setup Pages & Indicator
            if (Settings.WeatherLoaded)
            {
                Pages.Remove(typeof(SetupLocationsPage));
            }

            IndicatorBox.ItemsSource = Pages;
        }

        public void Next()
        {
            PageIdx++;
            if (PageIdx >= Pages.Count) PageIdx = 0;

            if (!(AppFrame.Content is IPageVerification page) || page.CanContinue())
            {
                var transition = new SlideNavigationTransitionInfo();

                if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo", "Effect"))
                {
                    transition.Effect = SlideNavigationTransitionEffect.FromLeft;
                }

                AppFrame.Navigate(Pages[PageIdx], null, transition);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (AppFrame.Content == null)
            {
                AppFrame.Navigate(Pages[PageIdx = 0]);
            }
            AnalyticsLogger.LogEvent("SetupPage");
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.SourcePageType != Pages[0])
            {
                PageIdx--;
                AppFrame.GoBack();
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.SourcePageType == Pages.Last())
            {
                // Retrieve setiings
                if (CoreApplication.Properties.TryGetValue(Settings.KEY_USEALERTS, out object alertsValue))
                {
                    Settings.ShowAlerts = (bool)alertsValue;
                    CoreApplication.Properties.Remove(Settings.KEY_USEALERTS);
                }
                if (CoreApplication.Properties.TryGetValue(Settings.KEY_REFRESHINTERVAL, out object refreshValue))
                {
                    Settings.RefreshInterval = (int)refreshValue;
                    CoreApplication.Properties.Remove(Settings.KEY_REFRESHINTERVAL);
                }
                if (CoreApplication.Properties.TryGetValue(Settings.KEY_TEMPUNIT, out object tempValue))
                {
                    Settings.SetDefaultUnits((string)tempValue);
                    CoreApplication.Properties.Remove(Settings.KEY_TEMPUNIT);
                }

                Settings.OnBoardComplete = true;
                this.Frame.Navigate(typeof(Shell), Location);
            }
            else
            {
                Next();
            }
        }

        private void AppFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == Pages[0])
            {
                BackBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Visible;
                NextBtn.Icon = new SymbolIcon(Symbol.Forward);
                NextBtn.Label = App.ResLoader.GetString("Label_Next/Label");
            }
            else if (e.SourcePageType == Pages.Last())
            {
                BackBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Visible;
                NextBtn.Icon = new SymbolIcon(Symbol.Accept);
                NextBtn.Label = App.ResLoader.GetString("Label_Done");
            }
            else
            {
                if (e.SourcePageType == typeof(SetupLocationsPage))
                {
                    BackBtn.Visibility = Visibility.Collapsed;
                    NextBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BackBtn.Visibility = Visibility.Visible;
                    NextBtn.Visibility = Visibility.Visible;
                }
                NextBtn.Icon = new SymbolIcon(Symbol.Forward);
                NextBtn.Label = App.ResLoader.GetString("Label_Next");
            }

            // Change indicators
            IndicatorBox.SelectedIndex = PageIdx;
        }
    }
}