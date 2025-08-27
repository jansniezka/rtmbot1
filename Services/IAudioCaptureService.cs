namespace RealTimeMediaBot.Services;

public interface IAudioCaptureService
{
    Task ProcessAudioFrameAsync(byte[] audioData);
    Task ProcessAudioFrameAsync(AudioFrame audioFrame);
    Task SaveAudioBufferAsync(string filePath);
    Task SaveAudioForCallAsync(string callId, string filePath);
    void ClearBuffer();
    void ClearBufferForCall(string callId);
    int GetBufferSize();
    int GetBufferSizeForCall(string callId);
}
