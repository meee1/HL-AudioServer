using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AudioTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("10.1.1.107"), 5060);
            var listener = new UdpClient(5060);

            int SampleRate = 8000;

            var wavein = new WaveInEvent();
            wavein.DeviceNumber = 0;
            wavein.WaveFormat = new WaveFormat(SampleRate, 16, 1);

            wavein.StartRecording();

            wavein.DataAvailable += (sender, eventArgs) =>
            {
                listener.Send(eventArgs.Buffer, eventArgs.BytesRecorded, endpoint);
            };


            BufferedWaveProvider bufferedWave = new BufferedWaveProvider(new WaveFormat(SampleRate, 16, 1));
            bufferedWave.BufferDuration = TimeSpan.FromMilliseconds(200);
            bufferedWave.DiscardOnBufferOverflow = true;

            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.DesiredLatency = 60;
                outputDevice.Init(bufferedWave);
                outputDevice.Play();
                IPEndPoint from = null;

                while (true)
                {
                    var data = listener.Receive(ref from);
                    bufferedWave.AddSamples(data, 0, data.Length);

                    Console.WriteLine(DateTime.Now.Millisecond + " " + listener.Available + " " + bufferedWave.BufferedBytes);
                }
            }
        }
    }
}
