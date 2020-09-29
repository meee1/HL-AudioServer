using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Android.App;
using Android.Media;
using Android.Net.Rtp;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace AudioServer
{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            new Thread(() =>
            {

                var endpoint = new IPEndPoint(IPAddress.Parse("10.1.1.26"), 5060);
                var listener = new UdpClient(5060);

                // Get buffer size
                int SampleRate = 8000;
                var BufferSize =
                    AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit);

                var audioin = new AudioRecord(AudioSource.Mic, SampleRate, ChannelIn.Mono,
                    Android.Media.Encoding.Pcm16bit, BufferSize);

                audioin.StartRecording();

                AudioTrack audio = new AudioTrack(Stream.Music, SampleRate, ChannelConfiguration.Mono, Encoding.Pcm16bit,
                    BufferSize, AudioTrackMode.Stream);

                audio.Play();


                //https://developer.android.com/reference/android/media/MediaFormat#MIMETYPE_AUDIO_RAW
                //var codec = MediaCodec.CreateByCodecName("audio/raw");
                //codec.Configure(MediaFormat.CreateAudioFormat("audio/raw", 8000, 1), null, null,MediaCodecConfigFlags.None);

                //codec.SetCallback(new CallBacks());

                //codec.Start();

                var frames = new byte[100];

                while (true)
                {
                    int read = 0;
                    if ((read = audioin.Read(frames, 0, frames.Length)) > 0)
                    {
                        listener.Send(frames, read, endpoint);
                    }

                    while (listener.Available > 0)
                    {
                        IPEndPoint endpointfrom = null;
                        var data = listener.Receive(ref endpointfrom);
                        audio.Write(data, 0, data.Length);
                    }
                }
            }).Start();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
