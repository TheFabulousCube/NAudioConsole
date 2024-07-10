using System;

public class AudioRecorder
{
    private WaveInEvent waveIn;
    private WebSocketService webSocketService;

    public AudioRecorder(WebSocketService webSocketService)
    {
        this.webSocketService = webSocketService;
    }

    public void StartRecording()
    {
        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 100
        };

        waveIn.DataAvailable += OnDataAvailable;
        waveIn.StartRecording();
    }

    public void StopRecording()
    {
        if (waveIn != null)
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn = null;
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        var audioChunk = new
        {
            @object = "audio_chunk",
            payload = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded),
            stream_id = "doctor_stream"
        };

        var message = JsonConvert.SerializeObject(audioChunk);
        webSocketService.SendMessage(message);
    }
}

public class WebSocketService
{
    private WebSocket websocket;

    public void SetupWebSocket()
    {
        // WebSocket setup code
    }

    public void SendMessage(string message)
    {
        websocket.Send(message);
    }

    // Other WebSocket-related methods
    // from websocket: 

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
}
