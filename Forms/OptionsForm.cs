using SpaceShooterFinalWithSounds.Core;

namespace SpaceShooterFinalWithSounds.Forms;

public class OptionsForm : Form
{
    public OptionsForm()
    {
        Text = "Options";
        ClientSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(15, 15, 30);

        CheckBox music = new CheckBox
        {
            Text = "Mute Music",
            ForeColor = Color.White,
            Font = new Font("Arial", 13, FontStyle.Bold),
            Checked = SoundManager.IsMusicMuted,
            Location = new Point(45, 40),
            Size = new Size(300, 35)
        };
        CheckBox sfx = new CheckBox
        {
            Text = "Mute SFX",
            ForeColor = Color.White,
            Font = new Font("Arial", 13, FontStyle.Bold),
            Checked = SoundManager.IsSfxMuted,
            Location = new Point(45, 85),
            Size = new Size(300, 35)
        };

        music.CheckedChanged += (_, _) =>
        {
            SoundManager.IsMusicMuted = music.Checked;
            if (music.Checked) SoundManager.StopBackgroundMusic();
        };
        sfx.CheckedChanged += (_, _) => SoundManager.IsSfxMuted = sfx.Checked;

        Label guide = new Label
        {
            Text = "Controls:\n\nW A S D or Arrow Keys: Move\nSpace: Shoot\nEsc: Pause\n\nCollect coins and power-ups.\nClear 10 waves to win.",
            ForeColor = Color.White,
            Font = new Font("Arial", 12, FontStyle.Regular),
            Location = new Point(45, 150),
            Size = new Size(430, 220)
        };

        Controls.Add(music);
        Controls.Add(sfx);
        Controls.Add(guide);
    }
}
