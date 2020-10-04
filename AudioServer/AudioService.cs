using System.Net;
using System.Net.Sockets;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;

namespace AudioServer
{
    [Service]
    public class AudioService : Service
    {
        private bool running = false;

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (running)
                return StartCommandResult.Sticky;

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

                var frames = new byte[20 * 16];
                running = true;
                while (running)
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

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            running = false;
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}