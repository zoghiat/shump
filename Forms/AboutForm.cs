namespace SpaceShooterFinalWithSounds.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About";
        ClientSize = new Size(520, 360);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(10, 14, 30);

        Label label = new Label
        {
            Text = "Space Shooter Final Project\n\nAdvanced Programming\nC# Windows Forms + SQLite\n\nStudent 1: Amir Zoghi - 404400016\nStudent 2: Hamed Foroutti - 404522196\n\nArchitecture: OOP, Inheritance, Polymorphism\nDatabase: SQLite through DatabaseManager\nAudio: WAV files through SoundManager",
            ForeColor = Color.White,
            Font = new Font("Arial", 12, FontStyle.Bold),
            Location = new Point(35, 35),
            Size = new Size(455, 280)
        };
        
        Controls.Add(label);
    }
}
