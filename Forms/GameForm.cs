using System.Diagnostics;
using SpaceShooterFinalWithSounds.Game;

namespace SpaceShooterFinalWithSounds.Forms;

public class GameForm : Form
{
    private readonly System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();
    private readonly Stopwatch stopwatch = new Stopwatch();
    private readonly GameManager gameManager;

    public GameForm()
    {
        Text = "Space Shooter - Game";
        ClientSize = new Size(820, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        DoubleBuffered = true;
        KeyPreview = true;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        gameManager = new GameManager(ClientSize);
        gameManager.GameEnded += OnGameEnded;

        gameTimer.Interval = 16;
        gameTimer.Tick += GameTimer_Tick;
        stopwatch.Start();
        gameTimer.Start();

        KeyDown += GameForm_KeyDown;
        KeyUp += GameForm_KeyUp;
        Paint += GameForm_Paint;
        FormClosing += (_, _) => SpaceShooterFinalWithSounds.Core.SoundManager.StopBackgroundMusic();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        float deltaTime = Math.Min(0.05f, (float)stopwatch.Elapsed.TotalSeconds);
        stopwatch.Restart();
        gameManager.Update(deltaTime);
        Invalidate();
    }

    private void GameForm_KeyDown(object? sender, KeyEventArgs e)
    {
        gameManager.SetInput(e.KeyCode, true);
    }

    private void GameForm_KeyUp(object? sender, KeyEventArgs e)
    {
        gameManager.SetInput(e.KeyCode, false);
    }

    private void GameForm_Paint(object? sender, PaintEventArgs e)
    {
        gameManager.Draw(e.Graphics);
    }

    private void OnGameEnded(int score, int coins)
    {
        gameTimer.Stop();
        MessageBox.Show($"Score: {score}\nCoins Collected: {coins}\nData saved in SQLite.", "Game Finished");
        Close();
    }
}
