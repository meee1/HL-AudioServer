using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net.Rtp;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace AudioServer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var status = await Permissions.CheckStatusAsync<AudioPermission>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<AudioPermission>();

            
            var serviceToStart = new Intent(this, typeof(AudioService));

            StartService(serviceToStart);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
