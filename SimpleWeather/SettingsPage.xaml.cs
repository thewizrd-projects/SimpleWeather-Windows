﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleWeather
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            RestoreSettings();
        }

        private void RestoreSettings()
        {
            // Temperature
            if (Settings.Unit == "F")
            {
                Fahrenheit.IsChecked = true;
                Celsius.IsChecked = false;
            }
            else
            {
                Fahrenheit.IsChecked = false;
                Celsius.IsChecked = true;
            }
        }

        private void Fahrenheit_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Unit = "F";
        }

        private void Celsius_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Unit = "C";
        }
    }
}
