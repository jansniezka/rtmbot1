using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Collections.Concurrent;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public class AudioCaptureService : IAudioCaptureService
{
    private readonly ILogger<AudioCaptureService> _logger;
    private readonly ConcurrentQueue<AudioFrame> _audioBuffer;
    private readonly object _bufferLock = new object();
    private readonly int _maxBufferSize = 1000; // Maksymalna liczba klatek w buforze

    public AudioCaptureService(ILogger<AudioCaptureService> logger)
    {
        _logger = logger;
        _audioBuffer = new ConcurrentQueue<AudioFrame>();
    }

    public async Task ProcessAudioFrameAsync(byte[] audioData)
    {
        try
        {
            if (audioData != null && audioData.Length > 0)
            {
                var audioFrame = new AudioFrame
                {
                    AudioData = audioData,
                    Timestamp = DateTime.UtcNow,
                    CallId = "unknown"
                };

                await ProcessAudioFrameAsync(audioFrame);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania klatki audio");
        }
    }

    public async Task ProcessAudioFrameAsync(AudioFrame audioFrame)
    {
        try
        {
            if (audioFrame.AudioData != null && audioFrame.AudioData.Length > 0)
            {
                // Dodawanie do bufora
                lock (_bufferLock)
                {
                    if (_audioBuffer.Count >= _maxBufferSize)
                    {
                        // Usuwanie najstarszej klatki jeśli bufor jest pełny
                        if (_audioBuffer.TryDequeue(out _))
                        {
                            _logger.LogDebug("Usunięto najstarszą klatkę audio z bufora");
                        }
                    }
                    
                    _audioBuffer.Enqueue(audioFrame);
                }

                _logger.LogDebug("Dodano klatkę audio do bufora. CallId: {CallId}, Rozmiar: {Size} bajtów, Bufor: {BufferCount} klatek", 
                    audioFrame.CallId, audioFrame.AudioData.Length, _audioBuffer.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania klatki audio");
        }
    }

    public async Task SaveAudioBufferAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Zapisywanie bufora audio do pliku: {FilePath}", filePath);

            var audioFrames = new List<AudioFrame>();
            
            // Kopiowanie klatek z bufora
            lock (_bufferLock)
            {
                audioFrames.AddRange(_audioBuffer);
            }

            if (audioFrames.Count == 0)
            {
                _logger.LogWarning("Bufor audio jest pusty - nic do zapisania");
                return;
            }

            // Zapis do pliku WAV
            await SaveAsWavFileAsync(audioFrames, filePath);
            
            _logger.LogInformation("Pomyślnie zapisano {FrameCount} klatek audio do pliku", audioFrames.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania bufora audio");
            throw;
        }
    }

    private async Task SaveAsWavFileAsync(List<AudioFrame> audioFrames, string filePath)
    {
        await Task.Run(() =>
        {
            // Ustawienia audio (dostosuj do formatu otrzymywanego z Teams)
            const int sampleRate = 16000; // 16 kHz
            const int bitsPerSample = 16;
            const int channels = 1; // Mono

            using var waveFileWriter = new WaveFileWriter(filePath, new WaveFormat(sampleRate, bitsPerSample, channels));

            foreach (var frame in audioFrames)
            {
                waveFileWriter.Write(frame.AudioData, 0, frame.AudioData.Length);
            }
        });
    }

    public void ClearBuffer()
    {
        lock (_bufferLock)
        {
            while (_audioBuffer.TryDequeue(out _)) { }
            _logger.LogInformation("Bufor audio został wyczyszczony");
        }
    }

    public int GetBufferSize()
    {
        lock (_bufferLock)
        {
            return _audioBuffer.Count;
        }
    }

    // Nowe metody dla lepszego zarządzania audio
    public async Task SaveAudioForCallAsync(string callId, string filePath)
    {
        try
        {
            _logger.LogInformation("Zapisywanie audio dla połączenia {CallId} do pliku: {FilePath}", callId, filePath);

            var callAudioFrames = new List<AudioFrame>();
            
            // Filtruj klatki audio dla konkretnego połączenia
            lock (_bufferLock)
            {
                callAudioFrames.AddRange(_audioBuffer.Where(f => f.CallId == callId));
            }

            if (callAudioFrames.Count == 0)
            {
                _logger.LogWarning("Brak audio dla połączenia {CallId}", callId);
                return;
            }

            await SaveAsWavFileAsync(callAudioFrames, filePath);
            
            _logger.LogInformation("Pomyślnie zapisano {FrameCount} klatek audio dla połączenia {CallId}", 
                callAudioFrames.Count, callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania audio dla połączenia {CallId}", callId);
            throw;
        }
    }

    public void ClearBufferForCall(string callId)
    {
        lock (_bufferLock)
        {
            var framesToRemove = _audioBuffer.Where(f => f.CallId == callId).ToList();
            foreach (var frame in framesToRemove)
            {
                _audioBuffer.TryDequeue(out _);
            }
            _logger.LogInformation("Wyczyszczono {Count} klatek audio dla połączenia {CallId}", framesToRemove.Count, callId);
        }
    }

    public int GetBufferSizeForCall(string callId)
    {
        lock (_bufferLock)
        {
            return _audioBuffer.Count(f => f.CallId == callId);
        }
    }
}

// Model klatki audio
public class AudioFrame
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
    public string CallId { get; set; } = string.Empty;
}
