using SpaceShooterFinalWithSounds.Core;
using SpaceShooterFinalWithSounds.Data;

namespace SpaceShooterFinalWithSounds.Game;

public class GameManager
{
    public Player Player { get; private set; }
    public List<Enemy> ActiveEnemies { get; } = new List<Enemy>();
    public List<Bullet> PlayerBullets { get; } = new List<Bullet>();
    public List<Bullet> EnemyBullets { get; } = new List<Bullet>();
    public List<Coin> ActiveCoins { get; } = new List<Coin>();
    public List<PowerUp> ActivePowerUps { get; } = new List<PowerUp>();

    public int Score { get; private set; }
    public int SessionCoins { get; private set; }
    public int CurrentWave { get; private set; } = 1;
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; private set; }
    public string CenterMessage { get; private set; } = "Wave 1";

    private readonly Size screenSize;
    private readonly Random random = new Random();
    private readonly PlayerSave save;
    private bool moveLeft;
    private bool moveRight;
    private bool moveUp;
    private bool moveDown;
    private bool isShooting;
    private float spawnTimer;
    private float shootTimer;
    private float centerMessageTimer = 2.0f;
    private int spawnedEnemies;
    private int enemiesToSpawn;
    private float waveDelay;
    private string bulletStyle;
    private float elapsedGameTime;

    public event Action<int, int>? GameEnded;

    public GameManager(Size screenSize)
    {
        this.screenSize = screenSize;
        save = DatabaseManager.LoadPlayerSave();
        int extraLives = save.ExtraLifeBooster > 0 ? 1 : 0;
        if (extraLives > 0) DatabaseManager.ConsumeExtraLifeBooster();
        Player = new Player(screenSize.Width / 2f - 22, screenSize.Height - 85, extraLives);
        bulletStyle = save.EquippedBulletStyle switch
        {
            "green_laser" => "green_laser",
            "plasma" => "plasma",
            _ => "default"
        };
        StartWave(1);
        SoundManager.PlayBackgroundMusic();
    }

    public void SetInput(Keys key, bool isDown)
    {
        if (key == Keys.Left || key == Keys.A) moveLeft = isDown;
        if (key == Keys.Right || key == Keys.D) moveRight = isDown;
        if (key == Keys.Up || key == Keys.W) moveUp = isDown;
        if (key == Keys.Down || key == Keys.S) moveDown = isDown;
        if (key == Keys.Space) isShooting = isDown;
        if (isDown && key == Keys.Escape) IsPaused = !IsPaused;
    }

    public void Update(float deltaTime)
    {
        if (IsPaused || IsGameOver) return;

        elapsedGameTime += deltaTime;
        if (centerMessageTimer > 0) centerMessageTimer -= deltaTime;
        if (waveDelay > 0)
        {
            waveDelay -= deltaTime;
            if (waveDelay <= 0) StartWave(CurrentWave);
            return;
        }

        Player.ApplyInput(moveLeft, moveRight, moveUp, moveDown);
        Player.Update(deltaTime);
        Player.KeepInside(screenSize);

        HandleShooting(deltaTime);
        SpawnEnemies(deltaTime);
        UpdateObjects(deltaTime);
        CheckCollisions();
        RemoveDeadObjects();
        CheckWaveEnd();
    }

    private void HandleShooting(float deltaTime)
    {
        shootTimer += deltaTime;
        float fireDelay = Player.IsFireRateBoostActive ? 0.10f : 0.20f;
        if (!isShooting || shootTimer < fireDelay || !Player.CanShoot()) return;

        shootTimer = 0;
        Player.AddHeat();
        float cx = Player.X + Player.Width / 2f - 3;
        PlayerBullets.Add(new Bullet(cx, Player.Y - 16, 0, -11f, 18, true, bulletStyle));

        if (Player.IsTripleShotActive)
        {
            PlayerBullets.Add(new Bullet(cx - 12, Player.Y - 10, -2.7f, -10f, 18, true, bulletStyle));
            PlayerBullets.Add(new Bullet(cx + 12, Player.Y - 10, 2.7f, -10f, 18, true, bulletStyle));
        }

        SoundManager.PlayShoot();
    }

    private void SpawnEnemies(float deltaTime)
    {
        if (spawnedEnemies >= enemiesToSpawn) return;
        spawnTimer += deltaTime;
        float delay = Math.Max(0.30f, 0.9f - CurrentWave * 0.04f);
        if (spawnTimer < delay) return;
        spawnTimer = 0;
        spawnedEnemies++;
        int x = random.Next(30, screenSize.Width - 100);

        if (CurrentWave == 10 && spawnedEnemies == enemiesToSpawn)
        {
            ActiveEnemies.Add(new HeavyTankEnemy(x, -90, CurrentWave));
            return;
        }

        int roll = random.Next(100);
        if (CurrentWave >= 7 && roll < 15) ActiveEnemies.Add(new HeavyTankEnemy(x, -85, CurrentWave));
        else if (CurrentWave >= 5 && roll < 35) ActiveEnemies.Add(new ShooterEnemy(x, -60, CurrentWave));
        else if (CurrentWave >= 3 && roll < 60) ActiveEnemies.Add(new TerroristEnemy(x, -60, CurrentWave));
        else if (roll < 80) ActiveEnemies.Add(new ScoutEnemy(x, -50, CurrentWave));
        else ActiveEnemies.Add(new StandardEnemy(x, -50, CurrentWave));
    }

    private void UpdateObjects(float deltaTime)
    {
        foreach (Bullet bullet in PlayerBullets) bullet.Update(deltaTime);
        foreach (Bullet bullet in EnemyBullets) bullet.Update(deltaTime);
        foreach (Coin coin in ActiveCoins) coin.Update(deltaTime);
        foreach (PowerUp powerUp in ActivePowerUps) powerUp.Update(deltaTime);

        foreach (Enemy enemy in ActiveEnemies)
        {
            enemy.Move(Player, deltaTime);
            List<Bullet> newBullets = enemy.TryShoot(Player, bulletStyle);
            if (newBullets.Count > 0) EnemyBullets.AddRange(newBullets);
        }
    }

    private void CheckCollisions()
    {
        for (int i = PlayerBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = PlayerBullets[i];
            for (int j = ActiveEnemies.Count - 1; j >= 0; j--)
            {
                Enemy enemy = ActiveEnemies[j];
                if (!bullet.Bounds.IntersectsWith(enemy.Bounds)) continue;

                enemy.TakeDamage(bullet.Damage);
                bullet.IsAlive = false;
                if (!enemy.IsAlive) RewardForEnemy(enemy);
                break;
            }
        }

        for (int i = EnemyBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = EnemyBullets[i];
            if (!bullet.Bounds.IntersectsWith(Player.Bounds)) continue;
            bullet.IsAlive = false;
            bool damaged = Player.TakeDamage(bullet.Damage);
            if (damaged) SoundManager.PlayHit();
        }

        foreach (Enemy enemy in ActiveEnemies)
        {
            if (!enemy.Bounds.IntersectsWith(Player.Bounds)) continue;
            enemy.IsAlive = false;
            bool damaged = Player.TakeDamage(enemy is TerroristEnemy ? 35 : 22);
            if (damaged) SoundManager.PlayExplosion();
        }

        foreach (Coin coin in ActiveCoins)
        {
            if (!coin.Bounds.IntersectsWith(Player.Bounds)) continue;
            coin.IsAlive = false;
            SessionCoins += coin.Value;
            SoundManager.PlayCoin();
        }

        foreach (PowerUp powerUp in ActivePowerUps)
        {
            if (!powerUp.Bounds.IntersectsWith(Player.Bounds)) continue;
            powerUp.IsAlive = false;
            Player.ApplyPowerUp(powerUp.Type);
            SoundManager.PlayPowerUp();
        }

        if (!Player.IsAlive) EndGame(false);
    }

    private void RewardForEnemy(Enemy enemy)
    {
        Score += enemy.ScoreValue;
        SoundManager.PlayExplosion();

        if (random.Next(100) < enemy.PowerUpDropChance)
        {
            PowerUpType type = (PowerUpType)random.Next(0, 4);
            ActivePowerUps.Add(new PowerUp(enemy.X + enemy.Width / 2f, enemy.Y + enemy.Height / 2f, type));
            return;
        }

        if (random.Next(100) < enemy.CoinDropChance)
        {
            int value = random.Next(100) < 30 ? 5 : 1;
            ActiveCoins.Add(new Coin(enemy.X + enemy.Width / 2f, enemy.Y + enemy.Height / 2f, value));
        }
    }

    private void RemoveDeadObjects()
    {
        PlayerBullets.RemoveAll(b => !b.IsAlive || b.IsOut(screenSize));
        EnemyBullets.RemoveAll(b => !b.IsAlive || b.IsOut(screenSize));
        ActiveCoins.RemoveAll(c => !c.IsAlive || c.Y > screenSize.Height + 30);
        ActivePowerUps.RemoveAll(p => !p.IsAlive || p.Y > screenSize.Height + 30);
        ActiveEnemies.RemoveAll(e => !e.IsAlive || e.Y > screenSize.Height + 110);
    }

    private void CheckWaveEnd()
    {
        if (spawnedEnemies < enemiesToSpawn || ActiveEnemies.Count > 0) return;

        if (CurrentWave >= 10)
        {
            EndGame(true);
            return;
        }

        CurrentWave++;
        CenterMessage = "Wave " + CurrentWave + " starts soon";
        centerMessageTimer = 2.0f;
        waveDelay = 2.0f;
    }

    private void StartWave(int wave)
    {
        CurrentWave = wave;
        spawnedEnemies = 0;
        enemiesToSpawn = 5 + wave * 3;
        spawnTimer = 0;
        CenterMessage = "Wave " + wave;
        centerMessageTimer = 2.0f;
    }

    private void EndGame(bool won)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        CenterMessage = won ? "YOU WIN" : "GAME OVER";
        DatabaseManager.AddCoinsAndHighScore(SessionCoins, Score);
        SoundManager.PlayGameOver();
        SoundManager.StopBackgroundMusic();
        GameEnded?.Invoke(Score, SessionCoins);
    }

    public void Draw(Graphics g)
    {
        DrawBackground(g);
        foreach (Coin coin in ActiveCoins) coin.Draw(g);
        foreach (PowerUp powerUp in ActivePowerUps) powerUp.Draw(g);
        foreach (Bullet bullet in PlayerBullets) bullet.Draw(g);
        foreach (Bullet bullet in EnemyBullets) bullet.Draw(g);
        foreach (Enemy enemy in ActiveEnemies) enemy.Draw(g);
        Player.Draw(g);
        DrawHud(g);

        if (centerMessageTimer > 0 || IsGameOver)
        {
            using Font font = new Font("Arial", 30, FontStyle.Bold);
            SizeF size = g.MeasureString(CenterMessage, font);
            g.DrawString(CenterMessage, font, Brushes.White, screenSize.Width / 2f - size.Width / 2f, screenSize.Height / 2f - 40);
        }

        if (IsPaused)
        {
            using Font font = new Font("Arial", 32, FontStyle.Bold);
            g.DrawString("PAUSED", font, Brushes.Yellow, screenSize.Width / 2f - 90, screenSize.Height / 2f);
        }
    }

    private void DrawBackground(Graphics g)
    {
        Color bg = save.EquippedBackground switch
        {
            "galaxy" => Color.FromArgb(8, 0, 35),
            "neon_city" => Color.FromArgb(15, 12, 28),
            _ => Color.FromArgb(2, 5, 18)
        };
        using Brush brush = new SolidBrush(bg);
        g.FillRectangle(brush, 0, 0, screenSize.Width, screenSize.Height);

        using Brush starBrush = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
        for (int i = 0; i < 70; i++)
        {
            int x = (i * 97 + Environment.TickCount / 50) % Math.Max(1, screenSize.Width);
            int y = (i * 53 + Environment.TickCount / 20) % Math.Max(1, screenSize.Height);
            g.FillEllipse(starBrush, x, y, 2, 2);
        }
    }

    private void DrawHud(Graphics g)
    {
        using Font font = new Font("Arial", 11, FontStyle.Bold);
        g.DrawString("Score: " + Score, font, Brushes.White, 10, 10);
        g.DrawString("Coins: " + SessionCoins, font, Brushes.Gold, 125, 10);
        g.DrawString("Wave: " + CurrentWave + "/10", font, Brushes.Cyan, 240, 10);
        g.DrawString("Lives: " + Player.Lives, font, Brushes.Lime, 355, 10);

        TimeSpan elapsed = TimeSpan.FromSeconds(elapsedGameTime);
        string timeText = elapsed.TotalHours >= 1
            ? $"Time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
            : $"Time: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        g.DrawString(timeText, font, Brushes.White, 10, 34);

        g.DrawRectangle(Pens.White, 455, 12, 145, 14);
        g.FillRectangle(Brushes.DarkRed, 456, 13, 143, 12);
        g.FillRectangle(Brushes.LimeGreen, 456, 13, 143 * Player.HP / (float)Player.MaxHP, 12);

        g.DrawString("Heat", font, Brushes.White, 615, 10);
        g.DrawRectangle(Pens.White, 660, 12, 115, 14);
        Brush heatBrush = Player.IsOverheated ? Brushes.Red : Brushes.Orange;
        g.FillRectangle(heatBrush, 661, 13, 113 * Player.WeaponHeat / 100f, 12);

        int y = 58;
        if (Player.IsTripleShotActive) g.DrawString("Triple Shot", font, Brushes.Orange, 10, y);
        if (Player.IsShieldActive) g.DrawString("Shield", font, Brushes.DeepSkyBlue, 120, y);
        if (Player.IsFireRateBoostActive) g.DrawString("Fire Booster", font, Brushes.Violet, 210, y);
    }
}
