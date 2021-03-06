﻿using SimpleWeather.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SimpleWeather.WeatherData
{
    public partial class Weather
    {
        public Weather(HERE.Rootobject root)
        {
            var now = root.feedCreation;
            Forecast todaysForecast = null;
            TextForecast todaysTxtForecast = null;

            location = new Location(root.observations.location[0]);
            update_time = now;
            forecast = new List<Forecast>(root.dailyForecasts.forecastLocation.forecast.Length);
            txt_forecast = new List<TextForecast>(root.dailyForecasts.forecastLocation.forecast.Length);
            foreach (HERE.Forecast fcast in root.dailyForecasts.forecastLocation.forecast)
            {
                var dailyFcast = new Forecast(fcast);
                var txtFcast = new TextForecast(fcast);

                forecast.Add(dailyFcast);
                txt_forecast.Add(txtFcast);

                if (todaysForecast == null && dailyFcast.date.Date == now.UtcDateTime.Date)
                {
                    todaysForecast = dailyFcast;
                    todaysTxtForecast = txtFcast;
                }
            }
            hr_forecast = new List<HourlyForecast>(root.hourlyForecasts.forecastLocation.forecast.Length);
            foreach (HERE.Forecast1 forecast1 in root.hourlyForecasts.forecastLocation.forecast)
            {
                if (forecast1.utcTime.UtcDateTime < now.UtcDateTime.Trim(TimeSpan.TicksPerHour))
                    continue;

                hr_forecast.Add(new HourlyForecast(forecast1));
            }

            var observation = root.observations.location[0].observation[0];

            condition = new Condition(observation, todaysForecast, todaysTxtForecast);
            atmosphere = new Atmosphere(observation);
            astronomy = new Astronomy(root.astronomy.astronomy);
            precipitation = new Precipitation(observation, todaysForecast);
            ttl = 180;

            source = WeatherAPI.Here;
        }
    }

    public partial class Location
    {
        public Location(HERE.Location location)
        {
            // Use location name from location provider
            name = null;
            latitude = location.latitude;
            longitude = location.longitude;
            tz_long = null;
        }
    }

    public partial class Forecast
    {
        public Forecast(HERE.Forecast forecast)
        {
            date = forecast.utcTime.UtcDateTime;
            if (float.TryParse(forecast.highTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float highF))
            {
                high_f = highF;
                high_c = ConversionMethods.FtoC(highF);
            }
            if (float.TryParse(forecast.lowTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float lowF))
            {
                low_f = lowF;
                low_c = ConversionMethods.FtoC(lowF);
            }
            condition = forecast.description.ToPascalCase();
            icon = WeatherManager.GetProvider(WeatherAPI.Here)
                   .GetWeatherIcon(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", forecast.daylight, forecast.iconName));

            // Extras
            extras = new ForecastExtras();
            if (float.TryParse(forecast.comfort, NumberStyles.Float, CultureInfo.InvariantCulture, out float comfortTempF))
            {
                extras.feelslike_f = comfortTempF;
                extras.feelslike_c = ConversionMethods.FtoC(comfortTempF);
            }
            if (int.TryParse(forecast.humidity, NumberStyles.Integer, CultureInfo.InvariantCulture, out int humidity))
            {
                extras.humidity = humidity;
            }
            if (float.TryParse(forecast.dewPoint, NumberStyles.Float, CultureInfo.InvariantCulture, out float dewpointF))
            {
                extras.dewpoint_f = dewpointF;
                extras.dewpoint_c = ConversionMethods.FtoC(dewpointF);
            }
            if (int.TryParse(forecast.precipitationProbability, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pop))
            {
                extras.pop = pop;
            }
            if (float.TryParse(forecast.rainFall, NumberStyles.Float, CultureInfo.InvariantCulture, out float rain_in))
            {
                extras.qpf_rain_in = rain_in;
                extras.qpf_rain_mm = ConversionMethods.InToMM(rain_in);
            }
            if (float.TryParse(forecast.snowFall, NumberStyles.Float, CultureInfo.InvariantCulture, out float snow_in))
            {
                extras.qpf_snow_in = snow_in;
                extras.qpf_snow_cm = ConversionMethods.InToMM(snow_in / 10);
            }
            if (float.TryParse(forecast.barometerPressure, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressureIN))
            {
                extras.pressure_in = pressureIN;
                extras.pressure_mb = ConversionMethods.InHgToMB(pressureIN);
            }
            if (int.TryParse(forecast.windDirection, NumberStyles.Integer, CultureInfo.InvariantCulture, out int windDegrees))
            {
                extras.wind_degrees = windDegrees;
            }
            if (float.TryParse(forecast.windSpeed, NumberStyles.Float, CultureInfo.InvariantCulture, out float windSpeed))
            {
                extras.wind_mph = windSpeed;
                extras.wind_kph = ConversionMethods.MphToKph(windSpeed);
            }
            if (float.TryParse(forecast.uvIndex, NumberStyles.Float, CultureInfo.InvariantCulture, out float uv_index))
            {
                extras.uv_index = uv_index;
            }
        }
    }

    public partial class HourlyForecast
    {
        public HourlyForecast(HERE.Forecast1 hr_forecast)
        {
            date = hr_forecast.utcTime;
            if (float.TryParse(hr_forecast.temperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float highF))
            {
                high_f = highF;
                high_c = ConversionMethods.FtoC(highF);
            }
            condition = hr_forecast.description.ToPascalCase();

            icon = WeatherManager.GetProvider(WeatherAPI.Here)
                   .GetWeatherIcon(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", hr_forecast.daylight, hr_forecast.iconName));

            if (int.TryParse(hr_forecast.windDirection, NumberStyles.Integer, CultureInfo.InvariantCulture, out int windDeg))
                wind_degrees = windDeg;
            if (float.TryParse(hr_forecast.windSpeed, NumberStyles.Float, CultureInfo.InvariantCulture, out float windSpeed))
            {
                wind_mph = windSpeed;
                wind_kph = ConversionMethods.MphToKph(windSpeed);
            }

            // Extras
            extras = new ForecastExtras();
            if (float.TryParse(hr_forecast.comfort, NumberStyles.Float, CultureInfo.InvariantCulture, out float comfortTemp_f))
            {
                extras.feelslike_f = comfortTemp_f;
                extras.feelslike_c = ConversionMethods.FtoC(comfortTemp_f);
            }
            if (int.TryParse(hr_forecast.humidity, NumberStyles.Integer, CultureInfo.InvariantCulture, out int humidity))
            {
                extras.humidity = humidity;
            }
            if (float.TryParse(hr_forecast.dewPoint, NumberStyles.Float, CultureInfo.InvariantCulture, out float dewpointF))
            {
                extras.dewpoint_f = dewpointF;
                extras.dewpoint_c = ConversionMethods.FtoC(dewpointF);
            }
            if (float.TryParse(hr_forecast.visibility, NumberStyles.Float, CultureInfo.InvariantCulture, out float visibilityMI))
            {
                extras.visibility_mi = visibilityMI;
                extras.visibility_km = ConversionMethods.MiToKm(visibilityMI);
            }
            if (int.TryParse(hr_forecast.precipitationProbability, NumberStyles.Integer, CultureInfo.InvariantCulture, out int PoP))
            {
                extras.pop = PoP;
            }
            if (float.TryParse(hr_forecast.rainFall, NumberStyles.Float, CultureInfo.InvariantCulture, out float rain_in))
            {
                extras.qpf_rain_in = rain_in;
                extras.qpf_rain_mm = ConversionMethods.InToMM(rain_in);
            }
            if (float.TryParse(hr_forecast.snowFall, NumberStyles.Float, CultureInfo.InvariantCulture, out float snow_in))
            {
                extras.qpf_snow_in = snow_in;
                extras.qpf_snow_cm = ConversionMethods.InToMM(snow_in / 10);
            }
            if (float.TryParse(hr_forecast.barometerPressure, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressureIN))
            {
                extras.pressure_in = pressureIN;
                extras.pressure_mb = ConversionMethods.InHgToMB(pressureIN);
            }
            extras.wind_degrees = wind_degrees;
            extras.wind_mph = wind_mph;
            extras.wind_kph = wind_kph;
        }
    }

    public partial class TextForecast
    {
        public TextForecast(HERE.Forecast forecast)
        {
            date = forecast.utcTime;
            fcttext = String.Format(CultureInfo.InvariantCulture, "{0} - {1} {2}",
                forecast.weekday,
                forecast.description.ToPascalCase(), forecast.beaufortDescription.ToPascalCase());
            fcttext_metric = fcttext;
        }
    }

    public partial class Condition
    {
        public Condition(HERE.Observation observation, Forecast todaysForecast, TextForecast todaysTxtForecast)
        {
            weather = observation.description.ToPascalCase();
            if (float.TryParse(observation.temperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float tempF))
            {
                temp_f = tempF;
                temp_c = ConversionMethods.FtoC(tempF);
            }

            if (float.TryParse(observation.highTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float hiTempF) &&
                float.TryParse(observation.lowTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out float loTempF))
            {
                high_f = hiTempF;
                high_c = ConversionMethods.FtoC(hiTempF);
                low_f = loTempF;
                low_c = ConversionMethods.FtoC(loTempF);
            }
            else
            {
                high_f = todaysForecast?.high_f;
                high_c = todaysForecast?.high_c;
                low_f = todaysForecast?.low_f;
                low_c = todaysForecast?.low_c;
            }

            if (int.TryParse(observation.windDirection, NumberStyles.Integer, CultureInfo.InvariantCulture, out int windDegrees))
                wind_degrees = windDegrees;
            else
                wind_degrees = 0;

            if (float.TryParse(observation.windSpeed, NumberStyles.Float, CultureInfo.InvariantCulture, out float wind_Speed))
            {
                wind_mph = wind_Speed;
                wind_kph = ConversionMethods.MphToKph(wind_Speed);
                beaufort = new Beaufort(WeatherUtils.GetBeaufortScale((int)Math.Round(wind_Speed)));
            }

            if (float.TryParse(observation.comfort, NumberStyles.Float, CultureInfo.InvariantCulture, out float comfortTempF))
            {
                feelslike_f = comfortTempF;
                feelslike_c = ConversionMethods.FtoC(comfortTempF);
            }

            icon = WeatherManager.GetProvider(WeatherAPI.Here)
                   .GetWeatherIcon(string.Format("{0}_{1}", observation.daylight, observation.iconName));

            if (todaysForecast?.extras?.uv_index.HasValue == true)
                uv = new UV(todaysForecast.extras.uv_index.Value);

            observation_time = observation.utcTime;

            if (todaysForecast != null && todaysTxtForecast != null)
            {
                var culture = CultureUtils.UserCulture;
                var resLoader = SimpleLibrary.GetInstance().ResLoader;

                var summaryStr = new StringBuilder();
                summaryStr.Append(todaysTxtForecast.fcttext); // fcttext & fcttextMetric are the same

                if (todaysForecast?.extras?.pop.HasValue == true)
                {
                    summaryStr.AppendFormat(" {0}: {1}%",
                        resLoader.GetString("Label_Chance/Text"),
                        todaysForecast.extras.pop.Value);
                }

                summary = summaryStr.ToString();
            }
        }
    }

    public partial class Atmosphere
    {
        public Atmosphere(HERE.Observation observation)
        {
            if (int.TryParse(observation.humidity, NumberStyles.Integer, CultureInfo.InvariantCulture, out int Humidity))
            {
                humidity = Humidity;
            }

            if (float.TryParse(observation.barometerPressure, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressureIN))
            {
                pressure_in = pressureIN;
                pressure_mb = ConversionMethods.InHgToMB(pressureIN);
            }
            pressure_trend = observation.barometerTrend;

            if (float.TryParse(observation.visibility, NumberStyles.Float, CultureInfo.InvariantCulture, out float visibilityMI))
            {
                visibility_mi = visibilityMI;
                visibility_km = ConversionMethods.MiToKm(visibilityMI);
            }

            if (float.TryParse(observation.dewPoint, NumberStyles.Float, CultureInfo.InvariantCulture, out float dewpointF))
            {
                dewpoint_f = dewpointF;
                dewpoint_c = ConversionMethods.FtoC(dewpointF);
            }
        }
    }

    public partial class Astronomy
    {
        public Astronomy(HERE.Astronomy1[] astronomy)
        {
            var astroData = astronomy[0];

            if (DateTime.TryParse(astroData.sunrise, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sunrise))
                this.sunrise = sunrise;
            if (DateTime.TryParse(astroData.sunset, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sunset))
                this.sunset = sunset;
            if (DateTime.TryParse(astroData.moonrise, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime moonrise))
                this.moonrise = moonrise;
            if (DateTime.TryParse(astroData.moonset, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime moonset))
                this.moonset = moonset;

            // If the sun won't set/rise, set time to the future
            if (sunrise == null)
            {
                sunrise = DateTime.Now.Date.AddYears(1).AddTicks(-1);
            }
            if (sunset == null)
            {
                sunset = DateTime.Now.Date.AddYears(1).AddTicks(-1);
            }
            if (moonrise == null)
            {
                moonrise = DateTime.MinValue;
            }
            if (moonset == null)
            {
                moonset = DateTime.MinValue;
            }

            switch (astroData.iconName)
            {
                case "cw_new_moon":
                default:
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.NewMoon);
                    break;

                case "cw_waxing_crescent":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.WaxingCrescent);
                    break;

                case "cw_first_qtr":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.FirstQtr);
                    break;

                case "cw_waxing_gibbous":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.WaxingGibbous);
                    break;

                case "cw_full_moon":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.FullMoon);
                    break;

                case "cw_waning_gibbous":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.WaningGibbous);
                    break;

                case "cw_last_quarter":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.LastQtr);
                    break;

                case "cw_waning_crescent":
                    this.moonphase = new MoonPhase(MoonPhase.MoonPhaseType.WaningCrescent);
                    break;
            }
        }
    }

    public partial class Precipitation
    {
        public Precipitation(HERE.Observation observation, Forecast todaysForecast)
        {
            pop = todaysForecast?.extras?.pop;

            if (float.TryParse(observation.precipitation1H, NumberStyles.Float, CultureInfo.InvariantCulture, out float precipitation1H))
            {
                qpf_rain_in = precipitation1H;
                qpf_rain_mm = ConversionMethods.InToMM(precipitation1H);
            }
            else if (float.TryParse(observation.precipitation3H, NumberStyles.Float, CultureInfo.InvariantCulture, out float precipitation3H))
            {
                qpf_rain_in = precipitation3H;
                qpf_rain_mm = ConversionMethods.InToMM(precipitation3H);
            }
            else if (float.TryParse(observation.precipitation6H, NumberStyles.Float, CultureInfo.InvariantCulture, out float precipitation6H))
            {
                qpf_rain_in = precipitation6H;
                qpf_rain_mm = ConversionMethods.InToMM(precipitation6H);
            }
            else if (float.TryParse(observation.precipitation12H, NumberStyles.Float, CultureInfo.InvariantCulture, out float precipitation12H))
            {
                qpf_rain_in = precipitation12H;
                qpf_rain_mm = ConversionMethods.InToMM(precipitation12H);
            }
            else if (float.TryParse(observation.precipitation24H, NumberStyles.Float, CultureInfo.InvariantCulture, out float precipitation24H))
            {
                qpf_rain_in = precipitation24H;
                qpf_rain_mm = ConversionMethods.InToMM(precipitation24H);
            }
            else if (todaysForecast?.extras != null)
            {
                qpf_rain_in = todaysForecast?.extras?.qpf_rain_in;
                qpf_rain_mm = todaysForecast?.extras?.qpf_rain_mm;
            }

            if (float.TryParse(observation.snowCover, NumberStyles.Float, CultureInfo.InvariantCulture, out float snowCover))
            {
                qpf_snow_in = snowCover;
                qpf_snow_cm = ConversionMethods.InToMM(snowCover) / 10;
            }
            else if (todaysForecast?.extras != null)
            {
                qpf_snow_in = todaysForecast?.extras?.qpf_snow_in;
                qpf_snow_cm = todaysForecast?.extras?.qpf_snow_cm;
            }
        }
    }
}