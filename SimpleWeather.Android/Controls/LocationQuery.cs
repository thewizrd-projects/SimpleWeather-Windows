﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace SimpleWeather.Droid.Controls
{
    public class LocationQuery : LinearLayout
    {
        private View viewLayout;
        private TextView locationNameView;
        private TextView locationCountryView;

        public LocationQuery(Context context) :
            base(context)
        {
            Initialize(context);
        }

        public LocationQuery(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public LocationQuery(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            LayoutInflater inflater = LayoutInflater.From(context);
            viewLayout = inflater.Inflate(Resource.Layout.location_query_view, this);

            locationNameView = (TextView)viewLayout.FindViewById(Resource.Id.location_name);
            locationCountryView = (TextView)viewLayout.FindViewById(Resource.Id.location_country);
        }

        public void SetLocation(SimpleWeather.Controls.LocationQueryView view)
        {
            locationNameView.Text = view.LocationName;
            locationCountryView.Text = view.LocationCountry;
        }
    }
}