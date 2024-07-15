using NAudio.Wave;
using NAudio.Wave.SampleProviders;


if (args.Length == 0)
{
    // Show help
    Console.WriteLine("Use this command with an audio file as the first argument to play it");
}
else if (args.Length == 1)
{
    await PlayAudioFile(Path.GetFullPath(args[0]));
}


async Task PlayAudioFile(string path)
{
    using AudioFileReader audioFile = new(path);
    using WaveOutEvent outputDevice = new();

    int volumeDisplayCooldown = 0;
    
    outputDevice.Init(audioFile);
    outputDevice.Play();

    const int barLength = 120;
    Console.CursorVisible = false;
    Console.WriteLine($"Playing {Path.GetFileName(audioFile.FileName)}...");
    while (outputDevice.PlaybackState != PlaybackState.Stopped)
    {
        volumeDisplayCooldown--;
        
        // Display progress
        await Task.Delay(1);
        float progress = (float) audioFile.Position / audioFile.Length;

        Console.SetCursorPosition(0, 1);
        // Console.Clear();
        Console.Write($"{audioFile.CurrentTime:mm\\:ss} ");
        for (int i = 0; i < barLength * progress; i++)
        {
            Console.Write('#');
        }

        for (int i = (int) (barLength * progress); i < barLength; i++)
        {
            Console.Write('-');
        }

        Console.Write($" {audioFile.TotalTime:mm\\:ss} ");

        if (volumeDisplayCooldown > 0)
        {
            Console.Write($"\nVolume: {outputDevice.Volume*100:0}   ");
        }
        else
        {
            Console.Write("\n" + new string(' ', 120));
        }

        if (outputDevice.PlaybackState == PlaybackState.Paused)
            Console.Write("\nPaused");
        else
            Console.Write("\n" + new string(' ', 120));

        

        if (Console.KeyAvailable)
        {
            // Receive input
            ConsoleKeyInfo c = Console.ReadKey();
            switch (c.Key)
            {
                case ConsoleKey.Spacebar when outputDevice.PlaybackState == PlaybackState.Playing:
                    outputDevice.Pause();
                    break;
                case ConsoleKey.Spacebar:
                    outputDevice.Play();
                    break;
                case ConsoleKey.UpArrow:
                    if(outputDevice.Volume > 0.99f) break;
                    outputDevice.Volume += 0.01f;
                    volumeDisplayCooldown = 150;
                    break;
                case ConsoleKey.DownArrow:
                    if(outputDevice.Volume < 0.01f) break;
                    outputDevice.Volume -= 0.01f;
                    volumeDisplayCooldown = 150;
                    break;
            }
        }
        //outputDevice.Pause();
    }

    Console.CursorVisible = true;
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