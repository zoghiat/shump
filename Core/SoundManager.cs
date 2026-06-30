using NAudio.Wave;

namespace SpaceShooterFinalWithSounds.Core;

public static class SoundManager
{
    public static bool IsMusicMuted { get; set; }
    public static bool IsSfxMuted { get; set; }

    private static WaveOutEvent? backgroundOutput;
    private static AudioFileReader? backgroundReader;
    private static LoopStream? backgroundLoop;

    private static readonly List<WaveOutEvent> activeSfx = new List<WaveOutEvent>();

    private static string SoundPath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", fileName);
    }

    public static void PlayShoot()
    {
        PlaySfx("shoot.wav");
    }

    public static void PlayCoin()
    {
        PlaySfx("coin.wav");
    }

    public static void PlayExplosion()
    {
        PlaySfx("explosion.wav");
    }

    public static void PlayPowerUp()
    {
        PlaySfx("powerup.wav");
    }

    public static void PlayGameOver()
    {
        PlaySfx("gameover.wav");
    }

    public static void PlayHit()
    {
        PlaySfx("hit.wav");
    }

    private static void PlaySfx(string fileName)
    {
        if (IsSfxMuted) return;

        string path = SoundPath(fileName);
        if (!File.Exists(path)) return;

        try
        {
            AudioFileReader reader = new AudioFileReader(path);
            WaveOutEvent output = new WaveOutEvent();

            output.Init(reader);

            output.PlaybackStopped += (s, e) =>
            {
                output.Dispose();
                reader.Dispose();
                activeSfx.Remove(output);
            };

            activeSfx.Add(output);
            output.Play();
        }
        catch
        {
        }
    }

    public static void PlayBackgroundMusic()
    {
        if (IsMusicMuted) return;

        string path = SoundPath("background.wav");
        if (!File.Exists(path)) return;

        try
        {
            StopBackgroundMusic();

            backgroundReader = new AudioFileReader(path);
            backgroundLoop = new LoopStream(backgroundReader);
            backgroundOutput = new WaveOutEvent();

            backgroundOutput.Init(backgroundLoop);
            backgroundOutput.Play();
        }
        catch
        {
        }
    }

    public static void StopBackgroundMusic()
    {
        try
        {
            backgroundOutput?.Stop();
            backgroundOutput?.Dispose();
            backgroundLoop?.Dispose();
            backgroundReader?.Dispose();

            backgroundOutput = null;
            backgroundLoop = null;
            backgroundReader = null;
        }
        catch
        {
        }
    }

    private class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
        }

        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        public override long Length
        {
            get { return sourceStream.Length; }
        }

        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    sourceStream.Position = 0;
                }
                else
                {
                    totalBytesRead += bytesRead;
                }
            }

            return totalBytesRead;
        }
    }
}
