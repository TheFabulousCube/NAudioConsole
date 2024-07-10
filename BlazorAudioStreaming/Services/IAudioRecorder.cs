using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAudioStreaming.Services;
public interface IAudioRecorder
{
    void StartRecording();
    void StopRecording();
    event EventHandler<AudioDataAvailableEventArgs> DataAvailable;
}

public class AudioDataAvailableEventArgs : EventArgs
{
    public byte[] Buffer { get; set; }
    public int BytesRecorded { get; set; }
}
