﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWeather.Icons
{
    public partial interface IWeatherIconsProvider
    {
        bool IsFontIcon { get; }
    }
}
