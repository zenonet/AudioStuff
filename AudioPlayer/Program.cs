using System.Runtime.InteropServices;
using NAudio.Wave;


if (args.Length == 0)
{
    // Show help
    Console.WriteLine("Use this command with an audio file as the first argument to play it");
}
else if (args.Length == 1)
{
    string path = Path.GetFullPath(args[0]);
    if (!File.Exists(path))
    {
        Console.WriteLine("File does not exist");
        return;
    }

    await PlayAudioFile(Path.GetFullPath(args[0]));
}


async Task PlayAudioFile(string path)
{
    int volumeDisplayCooldown = 0;

    AudioFileReader audioFile;
    using WaveOutEvent outputDevice = new();
    try
    {
        audioFile = new(path);
        outputDevice.Init(audioFile);
    }
    catch (COMException)
    {
        Console.WriteLine("Unsupported file format");
        return;
    }

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
            Console.Write($"\nVolume: {outputDevice.Volume*100:0}   ");
        else
            Console.Write("\n" + new string(' ', 120));

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
                    if (outputDevice.Volume > 0.99f) break;
                    outputDevice.Volume += 0.01f;
                    volumeDisplayCooldown = 150;
                    break;
                case ConsoleKey.DownArrow:
                    if (outputDevice.Volume < 0.01f) break;
                    outputDevice.Volume -= 0.01f;
                    volumeDisplayCooldown = 150;
                    break;

                case ConsoleKey.RightArrow:
                    if (audioFile.TotalTime.TotalSeconds - audioFile.CurrentTime.TotalSeconds < 5f) audioFile.CurrentTime = audioFile.TotalTime;
                    audioFile.CurrentTime += TimeSpan.FromSeconds(5);
                    break;
                case ConsoleKey.LeftArrow:
                    if (audioFile.CurrentTime.TotalSeconds < 5f) audioFile.CurrentTime = TimeSpan.Zero;
                    audioFile.CurrentTime -= TimeSpan.FromSeconds(5);
                    break;
                
                case ConsoleKey.Q:
                    Console.WriteLine("\nQuitting...");
                    await audioFile.DisposeAsync();
                    return;
            }
        }
    }

    Console.CursorVisible = true;
    await audioFile.DisposeAsync();
}