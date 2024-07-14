using NAudio.Wave;
using NAudio.Wave.SampleProviders;

/*
var sine20Seconds = new SignalGenerator
    {
        Gain = 0.2,
        Frequency = 500,
        Type = SignalGeneratorType.Sin
    }
    .Take(TimeSpan.FromSeconds(20));
using (var wo = new WaveOutEvent())
{
    wo.Init(sine20Seconds);
    wo.Play();
    while (wo.PlaybackState == PlaybackState.Playing)
    {
        Thread.Sleep(500);
    }
}*/


WaveInEvent recorder;
BufferedWaveProvider bufferedWaveProvider = null!;
SavingWaveProvider savingWaveProvider;
WaveOutEvent player;

// set up the recorder
recorder = new WaveInEvent();
recorder.BufferMilliseconds = 10;
recorder.DataAvailable += RecorderOnDataAvailable;

// set up our signal chain
bufferedWaveProvider = new BufferedWaveProvider(recorder.WaveFormat);
//savingWaveProvider = new SavingWaveProvider(bufferedWaveProvider, "temp.wav");

// set up playback
player = new WaveOutEvent();
player.Init(bufferedWaveProvider);

// begin playback & record
player.Play();
recorder.StartRecording();


while (true)
{
    await Task.Delay(1);
}

void RecorderOnDataAvailable(object? sender, WaveInEventArgs args)
{
    byte[] buffer = args.Buffer;

    int bytesPerFrame = recorder.WaveFormat.BitsPerSample / 8 * recorder.WaveFormat.Channels;
    int frames = buffer.Length / bytesPerFrame;
    Console.WriteLine($"Frames: {frames}");
    for (int i = 0; i < frames; i+=bytesPerFrame)
    {
        buffer[i * bytesPerFrame+3] += 50;
    }
    
    bufferedWaveProvider.AddSamples(buffer, 0, buffer.Length);
}

return;


class SavingWaveProvider : IWaveProvider, IDisposable
{
    private readonly IWaveProvider sourceWaveProvider;
    private readonly WaveFileWriter writer;
    private bool isWriterDisposed;

    public SavingWaveProvider(IWaveProvider sourceWaveProvider, string wavFilePath)
    {
        this.sourceWaveProvider = sourceWaveProvider;
        writer = new WaveFileWriter(wavFilePath, sourceWaveProvider.WaveFormat);
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        var read = sourceWaveProvider.Read(buffer, offset, count);
        if (count > 0 && !isWriterDisposed)
        {
            writer.Write(buffer, offset, read);
        }

        if (count == 0)
        {
            Dispose(); // auto-dispose in case users forget
        }

        return read;
    }

    public WaveFormat WaveFormat
    {
        get { return sourceWaveProvider.WaveFormat; }
    }

    public void Dispose()
    {
        if (!isWriterDisposed)
        {
            isWriterDisposed = true;
            writer.Dispose();
        }
    }
}