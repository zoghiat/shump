using System.Media;

namespace SpaceShooterFinalWithSounds.Core;

public static class SoundManager
{
    public static bool IsMusicMuted { get; set; }
    public static bool IsSfxMuted { get; set; }

    private static SoundPlayer? backgroundPlayer;

    private static string SoundPath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", fileName);
    }

    public static void PlayShoot() => PlaySfx("shoot.wav");
    public static void PlayCoin() => PlaySfx("coin.wav");
    public static void PlayExplosion() => PlaySfx("explosion.wav");
    public static void PlayPowerUp() => PlaySfx("powerup.wav");
    public static void PlayGameOver() => PlaySfx("gameover.wav");
    public static void PlayHit() => PlaySfx("hit.wav");

    private static void PlaySfx(string fileName)
    {
        if (IsSfxMuted) return;
        string path = SoundPath(fileName);
        if (!File.Exists(path)) return;
        try
        {
            using SoundPlayer player = new SoundPlayer(path);
            player.Play();
        }
        catch { }
    }

    public static void PlayBackgroundMusic()
    {
        if (IsMusicMuted) return;
        string path = SoundPath("background.wav");
        if (!File.Exists(path)) return;
        try
        {
            backgroundPlayer?.Stop();
            backgroundPlayer = new SoundPlayer(path);
            backgroundPlayer.PlayLooping();
        }
        catch { }
    }

    public static void StopBackgroundMusic()
    {
        try { backgroundPlayer?.Stop(); } catch { }
    }
}
