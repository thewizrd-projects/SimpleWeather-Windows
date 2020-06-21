﻿using SimpleWeather.WeatherData;
using System;

namespace SimpleWeather.Controls
{
    public enum WeatherDetailsType
    {
        Sunrise,
        Sunset,
        FeelsLike,
        WindSpeed,
        Humidity,
        Pressure,
        Visibility,
        PoPCloudiness,
        PoPChance,
        PoPRain,
        PoPSnow,
        Dewpoint,
        Moonrise,
        Moonset,
        MoonPhase,
        Beaufort,
        UV,
        AirQuality
    }

    public class DetailItemViewModel
    {
        public WeatherDetailsType DetailsType { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public string Value { get; set; }
        public int IconRotation { get; set; }

        public DetailItemViewModel(WeatherDetailsType detailsType, String value)
            : this(detailsType, value, 0)
        {
        }

        public DetailItemViewModel(WeatherDetailsType detailsType, String value, int iconRotation)
        {
            this.DetailsType = detailsType;

            switch (detailsType)
            {
                case WeatherDetailsType.Sunrise:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Sunrise/Text");
                    this.Icon = WeatherIcons.SUNRISE;
                    break;

                case WeatherDetailsType.Sunset:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Sunset/Text");
                    this.Icon = WeatherIcons.SUNSET;
                    break;

                case WeatherDetailsType.FeelsLike:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_FeelsLike/Text");
                    this.Icon = WeatherIcons.THERMOMETER;
                    break;

                case WeatherDetailsType.WindSpeed:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Wind/Text");
                    this.Icon = WeatherIcons.WIND_DIRECTION;
                    break;

                case WeatherDetailsType.Humidity:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Humidity/Text");
                    this.Icon = WeatherIcons.HUMIDITY;
                    break;

                case WeatherDetailsType.Pressure:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Pressure/Text");
                    this.Icon = WeatherIcons.BAROMETER;
                    break;

                case WeatherDetailsType.Visibility:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Visibility/Text");
                    this.Icon = WeatherIcons.FOG;
                    break;

                case WeatherDetailsType.PoPCloudiness:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Cloudiness/Text");
                    this.Icon = WeatherIcons.CLOUDY;
                    break;

                case WeatherDetailsType.PoPChance:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Chance/Text");
                    this.Icon = WeatherIcons.UMBRELLA;
                    break;

                case WeatherDetailsType.PoPRain:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Rain/Text");
                    this.Icon = WeatherIcons.RAINDROPS;
                    break;

                case WeatherDetailsType.PoPSnow:
                    this.Label = SimpleLibrary.ResLoader.GetString("Label_Snow/Text");
                    this.Icon = WeatherIcons.SNOWFLAKE_COLD;
                    break;

                case WeatherDetailsType.Dewpoint:
                    this.Label = SimpleLibrary.ResLoader.GetString("Dewpoint_Label");
                    this.Icon = WeatherIcons.THERMOMETER;
                    break;

                case WeatherDetailsType.Moonrise:
                    this.Label = SimpleLibrary.ResLoader.GetString("Moonrise_Label");
                    this.Icon = WeatherIcons.MOONRISE;
                    break;

                case WeatherDetailsType.Moonset:
                    this.Label = SimpleLibrary.ResLoader.GetString("Moonset_Label");
                    this.Icon = WeatherIcons.MOONSET;
                    break;

                case WeatherDetailsType.MoonPhase:
                    this.Label = SimpleLibrary.ResLoader.GetString("MoonPhase_Label");
                    this.Icon = WeatherIcons.MOON_ALT_NEW;
                    break;

                case WeatherDetailsType.Beaufort:
                    this.Label = SimpleLibrary.ResLoader.GetString("Beaufort_Label");
                    this.Icon = WeatherIcons.WIND_BEAUFORT_0;
                    break;

                case WeatherDetailsType.UV:
                    this.Label = SimpleLibrary.ResLoader.GetString("UV_Label");
                    this.Icon = WeatherIcons.DAY_SUNNY;
                    break;
            }

            this.Value = value;
            this.IconRotation = iconRotation;
        }

        public DetailItemViewModel(MoonPhase.MoonPhaseType moonPhaseType, String description)
        {
            this.Label = SimpleLibrary.ResLoader.GetString("MoonPhase_Label");
            this.Value = description;
            this.IconRotation = 0;

            switch (moonPhaseType)
            {
                case MoonPhase.MoonPhaseType.NewMoon:
                    this.Icon = WeatherIcons.MOON_ALT_NEW;
                    break;

                case MoonPhase.MoonPhaseType.WaxingCrescent:
                    this.Icon = WeatherIcons.MOON_ALT_WAXING_CRESCENT_3;
                    break;

                case MoonPhase.MoonPhaseType.FirstQtr:
                    this.Icon = WeatherIcons.MOON_ALT_FIRST_QUARTER;
                    break;

                case MoonPhase.MoonPhaseType.WaxingGibbous:
                    this.Icon = WeatherIcons.MOON_ALT_WAXING_GIBBOUS_3;
                    break;

                case MoonPhase.MoonPhaseType.FullMoon:
                    this.Icon = WeatherIcons.MOON_ALT_FULL;
                    break;

                case MoonPhase.MoonPhaseType.WaningGibbous:
                    this.Icon = WeatherIcons.MOON_ALT_WANING_GIBBOUS_3;
                    break;

                case MoonPhase.MoonPhaseType.LastQtr:
                    this.Icon = WeatherIcons.MOON_ALT_THIRD_QUARTER;
                    break;

                case MoonPhase.MoonPhaseType.WaningCrescent:
                    this.Icon = WeatherIcons.MOON_ALT_WANING_CRESCENT_3;
                    break;
            }
        }

        public DetailItemViewModel(Beaufort.BeaufortScale beaufortScale, String description)
        {
            this.Label = SimpleLibrary.ResLoader.GetString("Beaufort_Label");
            this.Value = description;
            this.IconRotation = 0;

            switch (beaufortScale)
            {
                case Beaufort.BeaufortScale.B0:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_0;
                    break;

                case Beaufort.BeaufortScale.B1:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_1;
                    break;

                case Beaufort.BeaufortScale.B2:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_2;
                    break;

                case Beaufort.BeaufortScale.B3:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_3;
                    break;

                case Beaufort.BeaufortScale.B4:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_4;
                    break;

                case Beaufort.BeaufortScale.B5:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_5;
                    break;

                case Beaufort.BeaufortScale.B6:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_6;
                    break;

                case Beaufort.BeaufortScale.B7:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_7;
                    break;

                case Beaufort.BeaufortScale.B8:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_8;
                    break;

                case Beaufort.BeaufortScale.B9:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_9;
                    break;

                case Beaufort.BeaufortScale.B10:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_10;
                    break;

                case Beaufort.BeaufortScale.B11:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_11;
                    break;

                case Beaufort.BeaufortScale.B12:
                    this.Icon = WeatherIcons.WIND_BEAUFORT_12;
                    break;
            }
        }

        public DetailItemViewModel(AirQuality aqi)
        {
            this.DetailsType = WeatherDetailsType.AirQuality;
            this.Label = SimpleLibrary.ResLoader.GetString("AQI_Label");
            this.Icon = WeatherIcons.CLOUDY_GUSTS;
            this.IconRotation = 0;

            if (aqi.index < 51)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_0_50");
            }
            else if (aqi.index < 101)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_51_100");
            }
            else if (aqi.index < 151)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_101_150");
            }
            else if (aqi.index < 201)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_151_200");
            }
            else if (aqi.index < 301)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_201_300");
            }
            else if (aqi.index >= 301)
            {
                this.Value = SimpleLibrary.ResLoader.GetString("AQI_Level_300");
            }
        }
    }
}