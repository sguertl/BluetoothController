﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Graphics.Drawables;

namespace BluetoothApplication
{
    [Activity(Label = "BluetoothApplication", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private BluetoothAdapter mBluetoothAdapter;
        private Button btPairedDevices;
        private Button btSearchDevices;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            init();

            //Check wether the device supports Bluetooth
            if (mBluetoothAdapter == null)
            {
                Toast.MakeText(ApplicationContext, "Bluetooth is not supported", 0).Show();
            }
            else
            {
                if (!mBluetoothAdapter.IsEnabled)
                {
                    turnBluetoothOn();
                }
            }
        }

        /// <summary>
        /// Intializes members and makes the style for ui
        /// </summary>
        private void init()
        {
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            btPairedDevices = FindViewById<Button>(Resource.Id.btPairedDevices);
            btSearchDevices = FindViewById<Button>(Resource.Id.btSearchDevices);

            // Text Color
            btPairedDevices.SetTextColor(Android.Graphics.Color.Black);
            btSearchDevices.SetTextColor(Android.Graphics.Color.Black);

            // Border
            GradientDrawable drawable = new GradientDrawable();
            drawable.SetShape(ShapeType.Rectangle);
            drawable.SetStroke(2, Android.Graphics.Color.Black);

            drawable.SetColor(Android.Graphics.Color.White);
            btPairedDevices.SetBackgroundDrawable(drawable);
            btSearchDevices.SetBackgroundDrawable(drawable);

            // Main Background
            LinearLayout line = FindViewById<LinearLayout>(Resource.Id.linear);
            line.SetBackgroundColor(Android.Graphics.Color.White);

            // Add mouse Clicked
            btPairedDevices.Click += delegate
            {
                StartActivity(typeof(PairedDevices));
            };

            btSearchDevices.Click += delegate
            {
                StartActivity(typeof(SearchDevices));
            };
        }

        private void turnBluetoothOn()
        {
            Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(intent, 1);
        }
    }
}
