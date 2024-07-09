using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NAudio.Wave;
using WebSocket4Net;
using WebSocket4Net.Common;

namespace NablaTranscriptionPOC
{
    class Program
    {
        private static WaveInEvent waveIn;
        private static WebSocket websocket;
        private static string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiI1ZDlkODAwMi05OWFiLTRlM2EtYjQ1MC1hYjkzY2EwNjhhYzUiLCJjbG91ZF9yZWdpb24iOiJ1cy1jZW50cmFsMSIsInNlc3Npb25fbG9naW5fbWV0aG9kIjoib3JnYW5pemF0aW9uX2luaXRpYXRlZCIsImlzcyI6Im5hYmxhLXByb2QiLCJ0eXAiOiJjb3BpbG90X2FwaV91c2VyX3JlZnJlc2giLCJuYWJsYV9yZWdpb24iOiJ1cyIsImV4cCI6MTcyMTA5MzEwNSwic2Vzc2lvbl91dWlkIjoiNzc3ZGE1M2YtYjMwYi00NjRlLTgxYjAtMTAzNDZlYTEyODQ3Iiwib3JnYW5pemF0aW9uU3RyaW5nSWQiOiJ0aGVmYWJ1bG91c2N1YmUtZjdiOWNiOSJ9.vcxOrS_W9ndwSsZLsP7nripUbWwBlMUjviyWfbH4LM4";

        static void Main(string[] args)
        {
            SetupWebSocket();
            StartAudioCapture();

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            StopAudioCapture();
            SendEndMessage();
        }

        private static void SetupWebSocket()
        {
            var uri = new Uri($"wss://api.nabla.com/v1/copilot-api/server/listen-ws");
            var headers = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Authorization", $"Bearer {token}")
                };

            websocket = new WebSocket(uri.ToString(), "copilot-listen-protocol", null, headers);

            websocket.Opened += WebSocketOpened;
            websocket.MessageReceived += WebSocketMessageReceived;
            websocket.Error += WebSocketError;
            websocket.Closed += WebSocketClosed;

            websocket.Open();
        }

        private static void WebSocketOpened(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket opened.");
            SendListenConfig();
        }

        private static void WebSocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received message: " + e.Message);
        }

        private static void WebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("WebSocket error: " + e.Exception.Message);
        }

        private static void WebSocketClosed(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket closed.");
        }

        private static void SendListenConfig()
        {
            var listenConfig = new
            {
                @object = "listen_config",
                output_objects = new[] { "transcript_item" },
                encoding = "pcm_s16le",
                sample_rate = 16000,
                language = "en-US",
                streams = new[]
                {
                    new { id = "doctor_stream", speaker_type = "doctor" },
                    new { id = "patient_stream", speaker_type = "patient" }
                },
                split_by_sentence = true
            };

            var message = JsonConvert.SerializeObject(listenConfig);
            websocket.Send(message);
        }

        private static void StartAudioCapture()
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 100
            };

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
        }

        private static void StopAudioCapture()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var audioChunk = new
            {
                @object = "audio_chunk",
                payload = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded),
                stream_id = "doctor_stream"
            };

            var message = JsonConvert.SerializeObject(audioChunk);
            websocket.Send(message);
        }

        private static void SendEndMessage()
        {
            var endMessage = new
            {
                @object = "end"
            };

            var message = JsonConvert.SerializeObject(endMessage);
            websocket.Send(message);
        }
    }
}
