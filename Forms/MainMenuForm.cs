using SpaceShooterFinalWithSounds.Data;

namespace SpaceShooterFinalWithSounds.Forms;

public class MainMenuForm : Form
{
    private Label infoLabel = new Label();

    public MainMenuForm()
    {
        Text = "Space Shooter - Main Menu";
        ClientSize = new Size(520, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(5, 8, 22);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        Label title = new Label
        {
            Text = "SPACE SHOOTER",
            ForeColor = Color.Cyan,
            Font = new Font("Arial", 30, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 105
        };
        Controls.Add(title);

        infoLabel.ForeColor = Color.White;
        infoLabel.Font = new Font("Arial", 11, FontStyle.Bold);
        infoLabel.TextAlign = ContentAlignment.MiddleCenter;
        infoLabel.Location = new Point(45, 110);
        infoLabel.Size = new Size(430, 40);
        Controls.Add(infoLabel);

        Button play = MakeButton("Play", 175);
        Button shop = MakeButton("Shop", 235);
        Button options = MakeButton("Options", 295);
        Button about = MakeButton("About", 355);
        Button quit = MakeButton("Quit", 415);

        play.Click += (_, _) =>
        {
            Hide();
            using GameForm gameForm = new GameForm();
            gameForm.ShowDialog();
            RefreshInfo();
            Show();
        };
        shop.Click += (_, _) => { using ShopForm f = new ShopForm(); f.ShowDialog(); RefreshInfo(); };
        options.Click += (_, _) => { using OptionsForm f = new OptionsForm(); f.ShowDialog(); };
        about.Click += (_, _) => { using AboutForm f = new AboutForm(); f.ShowDialog(); };
        quit.Click += (_, _) => Application.Exit();

        Controls.AddRange(new Control[] { play, shop, options, about, quit });
        RefreshInfo();
    }

    private Button MakeButton(string text, int y)
    {
        return new Button
        {
            Text = text,
            Font = new Font("Arial", 16, FontStyle.Bold),
            Size = new Size(230, 45),
            Location = new Point(145, y),
            BackColor = Color.FromArgb(20, 30, 55),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
    }

    private void RefreshInfo()
    {
        var save = DatabaseManager.LoadPlayerSave();
        infoLabel.Text = $"Total Coins: {save.TotalCoins}     High Score: {save.HighScore}";
    }
}
