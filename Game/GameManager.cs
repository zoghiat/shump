using System;
using System.Drawing;
using System.Windows.Forms;
using SpaceShooterFinalWithSounds.Core;
using SpaceShooterFinalWithSounds.Data;

namespace SpaceShooterFinalWithSounds.Game;

public class GameManager
{
    // sakhtan bazi kon asli
    public Player Player { get; private set; }
    // yek list az tamat obj ha
    public List<Enemy> ActiveEnemies { get; } = new List<Enemy>();
    public List<Bullet> PlayerBullets { get; } = new List<Bullet>();
    public List<Bullet> EnemyBullets { get; } = new List<Bullet>();
    public List<Coin> ActiveCoins { get; } = new List<Coin>();
    public List<PowerUp> ActivePowerUps { get; } = new List<PowerUp>();
    // var hae komaki
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
    // tanha sazande GameManager ke size safhe ro migeer
    public GameManager(Size screenSize)
    {
        this.screenSize = screenSize;
        // estefade az tabee haye DatabaseManager
        save = DatabaseManager.LoadPlayerSave();

        int extraLives = save.ExtraLifeBooster > 0 ? 1 : 0;

        if (extraLives > 0)
        {
            DatabaseManager.ConsumeExtraLifeBooster();
        }
        Player = new Player(
            screenSize.Width / 2f - 22,
            screenSize.Height - 85,
            extraLives,
            save.EquippedShipSkin
        );
        // bara gereftan tir bazikon 
        bulletStyle = save.EquippedBulletStyle switch
        {
            "green_laser" => "green_laser",
            "plasma" => "plasma",
            _ => "default"
        };
        // az motegher aval shroo beshe 
        StartWave(1);
        // seda background play beseh vaghti shroo shode
        SoundManager.PlayBackgroundMusic();
    }
    // bara tagher mokhtasate bazikon yek kelid va var badii ke key feshorde sohde ya na
    public void SetInput(Keys key, bool isDown)
    {
        if (key == Keys.Left || key == Keys.A) moveLeft = isDown;
        if (key == Keys.Right || key == Keys.D) moveRight = isDown;
        if (key == Keys.Up || key == Keys.W) moveUp = isDown;
        if (key == Keys.Down || key == Keys.S) moveDown = isDown;
        if (key == Keys.Space) isShooting = isDown;
        if (isDown && key == Keys.Escape)
        {
            IsPaused = !IsPaused;
        }
    }
    // game loop ghalbe tapande bazii
    public void Update(float deltaTime)
    {
        // delta time is 0.016 cuse of 1 / 60(60 frame)==0.016
        // اگز این دو حالت بود اپدیت نشود 
        if (IsPaused || IsGameOver) return;
        // bara modat zamani ke bazi kon bazi kaede
        elapsedGameTime += deltaTime;
        if (centerMessageTimer > 0)
        {
            centerMessageTimer -= deltaTime;
        }
        // bara zaman ke ro safhe neshoon mede wave chand hastim
        if (waveDelay > 0)
        {
            waveDelay -= deltaTime;

            if (waveDelay <= 0)
            {
                StartWave(CurrentWave);
            }

            return;
        }
        // متد هایی که برای بازی کن مهم هست و برای 
        //ApplyInput برای تغییر  جهت ان
        Player.ApplyInput(moveLeft, moveRight, moveUp, moveDown);
        // برای حرکت کردن ان Update
        Player.Update(deltaTime);
        // برای این که در صفحه بماند و بیرون صفحه نرود با توسط متد clamp این کار انجام شده در player
        Player.KeepInside(screenSize);
        // این متد ها باید هر فریم اجرا شوند زیرا با خروجی ها ی ان متد ها قرار است بازی رو مدریت کنیم
        // متد هندل شوتینگ برای مدریت تیر زدن بازیکن هست 
        HandleShooting(deltaTime);
        // این متد برای مدریت اسپان انمی هست 
        SpawnEnemies(deltaTime);
        // روی تمام اشیا متد update صدا زده شود 
        UpdateObjects(deltaTime);
        // برای چک کردن برخورد ها  و برای هر برخورد  اتفاق خاص خود بی افته 
        CheckCollisions();
        // تمام اشیا که مرده اند و یا از کادر بیرون رفته اند حذف شوند 
        RemoveDeadObjects();
        CheckWaveEnd();
        
    }

    private void HandleShooting(float deltaTime)
    {
        // برای اینکه بین تیر زدن بازیکن مدت زمان حداقل 0.2 ثانیه باشذ 
        shootTimer += deltaTime;
        // اگر پاور اپ فعال بود سرعت تیر ها بیشتر بشه به همین منظور مدت زمان بین تیر زدن رو کم می کنیم
        float fireDelay = Player.IsFireRateBoostActive ? 0.10f : 0.20f;
        // اگر صلاحیت تیر زدن رو نداشت خب ریترن می کنه
        if (!isShooting || shootTimer < fireDelay)
        {
            return;
        }
        // دوباره تایمر صفر می شود و تا زمانی که از حد مجاز نگذزرد قابلیت تیر زدن ندارد
        shootTimer = 0;
        // برای اینکه تیر از وسط سفینه بزنه بیرون
        float cx = Player.X + Player.Width / 2f - 3;
        PlayerBullets.Add(new Bullet(cx, Player.Y - 16, 0, -11f, 18, true, bulletStyle));
        // اگر این پاور اپ فعال بود علاوه بر تیر عادی سه تا تیر دیگه هم شلیک شود 
        if (Player.IsTripleShotActive)
        {
            PlayerBullets.Add(new Bullet(cx - 12, Player.Y - 10, -2.7f, -10f, 18, true, bulletStyle));
            PlayerBullets.Add(new Bullet(cx + 12, Player.Y - 10, 2.7f, -10f, 18, true, bulletStyle));
        }
        // زمانی که تیر شلیک شود تیر بزند 
        SoundManager.PlayShoot();
    }
    // متد بسیاز مهم اسپان 
    private void SpawnEnemies(float deltaTime)
    {
        // spawnedEnemies انمی های فعلی که اسپان شده اند
        //enemiesToSpawn انمی های که باید اسپن شده باشند  وحد مجاز هست
        // برای سخت تر کردن بازی توی startwave انمی هایی که قراره اسپان شن بیشتر میشه 
        if (spawnedEnemies >= enemiesToSpawn) return;
        // برای اینکه متوجه بشویم چقدر زمان گذشته تا انمی اسپان بشه
        spawnTimer += deltaTime;
        // این متغییر برای این هست که  حداقل سه دهم ثانیه دشمنان اسپان بشه 
        float delay = Math.Max(0.30f, 0.9f - CurrentWave * 0.04f);
        // اگه از حداقل زمان نگذشته باشد اجازه ی اسپان انمی رو ندهد 
        if (spawnTimer < delay) return;
        // حال اگر اسپان شد زمان صفرشود 

        spawnTimer = 0;
        // تعداد انمی های اضافه شده زیاد شود 
        spawnedEnemies++;
        // برای اینکه این x که یه جای زندوم از صفحه اسپان بشه
        // اون عدد 30 رو گذاشیتم که مطمن باشیم داخل صفحه باشد 
        int x = random.Next(30, screenSize.Width - 100);
        // fghat yek bar dar marhale 10 va akharin doshman
        if (CurrentWave == 10 && spawnedEnemies == enemiesToSpawn)
        {
            ActiveEnemies.Add(new HeavyTankEnemy(x, -90, CurrentWave));
            return;
        }
        // add random bara in ke har bar bazi ejra shode taze bashe
        int roll = random.Next(100);
        // in ghesmat gharar bood bashe vlali head ta goftan boss dar wave 10 biad
        //if (CurrentWave >= 7 && roll < 15)
        //{
        //    ActiveEnemies.Add(new HeavyTankEnemy(x, -85, CurrentWave));
        //}
        // برای اینکه شوتر انمی موج 5 بیاد و 35 درصد  اسپان بشه 
         if (CurrentWave >= 5 && roll < 35)
        {
            ActiveEnemies.Add(new ShooterEnemy(x, -60, CurrentWave));
        }
        // موج 3 65 دردصد
        else if (CurrentWave >= 3 && roll < 60)
        {
            ActiveEnemies.Add(new TerroristEnemy(x, -60, CurrentWave));
        }
        else if (roll < 80)
        {
            ActiveEnemies.Add(new ScoutEnemy(x, -50, CurrentWave));
        }
        else
        {
            ActiveEnemies.Add(new StandardEnemy(x, -50, CurrentWave));
        }
    }

    private void UpdateObjects(float deltaTime)
    {
        foreach (Bullet bullet in PlayerBullets)
        {
            bullet.Update(deltaTime);
        }

        foreach (Bullet bullet in EnemyBullets)
        {
            bullet.Update(deltaTime);
        }

        foreach (Coin coin in ActiveCoins)
        {
            coin.Update(deltaTime);
        }

        foreach (PowerUp powerUp in ActivePowerUps)
        {
            powerUp.Update(deltaTime);
        }
        // برای این قسمت باید موو صدا زدهبشه
        
        foreach (Enemy enemy in ActiveEnemies)
        {
            enemy.Move(Player, deltaTime);
            // با استفاده از چند ریختی  بدون اینکه بدونیم انمی چی هست میام متد رو صدا می زنمی اگه تیر داشته باشع میاد  توی تیرهای 
            // انمی جای گذلری می کنه 
            List<Bullet> newBullets = enemy.TryShoot(Player, bulletStyle);

            if (newBullets.Count > 0)
            {
                EnemyBullets.AddRange(newBullets);
            }
        }
    }

    private void CheckCollisions()
    {   // برای چک کردن برخورد تیر با دشمن 
        for (int i = PlayerBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = PlayerBullets[i];

            for (int j = ActiveEnemies.Count - 1; j >= 0; j--)
            {
                Enemy enemy = ActiveEnemies[j];
                // اگر تیر بهش نخورد خب ریترن کنه 
                if (!bullet.Bounds.IntersectsWith(enemy.Bounds)) continue;
                enemy.TakeDamage(bullet.Damage);
                bullet.IsAlive = false;
                if (!enemy.IsAlive)
                {
                    RewardForEnemy(enemy);
                }
                // این بریک هم مهمه 
                //اگه بریک نکنه ممکنه یک تیر به چند انمی بخوره
                break;
            }
        }
        // بریا برخورد  تیر انمی با  بازیکن
        for (int i = EnemyBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = EnemyBullets[i];

            if (!bullet.Bounds.IntersectsWith(Player.Bounds)) continue;

            bullet.IsAlive = false;

            bool damaged = Player.TakeDamage(bullet.Damage);

            if (damaged)
            {
                SoundManager.PlayHit();
            }
        }
        // برای برخورد بدنه ی انمی با خود بازیکن
        foreach (Enemy enemy in ActiveEnemies)
        {
            if (!enemy.Bounds.IntersectsWith(Player.Bounds)) continue;

            enemy.IsAlive = false;

            bool damaged = Player.TakeDamage(enemy is TerroristEnemy ? 35 : 22);

            if (damaged)
            {
                SoundManager.PlayExplosion();
            }
        }
        // برخورد بازیکن با  بازیکن
        foreach (Coin coin in ActiveCoins)
        {
            if (!coin.Bounds.IntersectsWith(Player.Bounds)) continue;

            coin.IsAlive = false;
            SessionCoins += coin.Value;

            SoundManager.PlayCoin();
        }
        // برخورد بازیکن با پاور اپ
        foreach (PowerUp powerUp in ActivePowerUps)
        {
            if (!powerUp.Bounds.IntersectsWith(Player.Bounds)) continue;

            powerUp.IsAlive = false;
            Player.ApplyPowerUp(powerUp.Type);

            SoundManager.PlayPowerUp();
        }
        // اگر بازیکن ببمیرد حذف شود 
        if (!Player.IsAlive)
        {
            EndGame(false);
        }
    }
    // بخش انتیاز دهی 
    private void RewardForEnemy(Enemy enemy)
    {   
        // در برخورد ها این متد صدا زده میشه و مقدار امتیاز انمی به سکور کلی اضافه میشه
        Score += enemy.ScoreValue;
        // صدای پوکیئن بیاد
        SoundManager.PlayExplosion();
        // یه عدد رندوم از صفر تا 99 میاد اگر کوچک تر از شانس خوردن پاور اپ با بازیکن باشد قبول نیست 
        if (random.Next(100) < enemy.PowerUpDropChance)
        {
            // یک عدد رندوم برای چهار تا پائر اپ ها 
            PowerUpType type = (PowerUpType)random.Next(0, 4);

            ActivePowerUps.Add(
                new PowerUp(
                    enemy.X + enemy.Width / 2f,
                    enemy.Y + enemy.Height / 2f,
                    type
                )
            );
            // اینجا ریترن اماده جون اگه پاور اپ افتاد دیگه کوین نیوفته 
            return;
        }

        if (random.Next(100) < enemy.CoinDropChance)
        {
            int value = random.Next(100) < 30 ? 5 : 1;

            ActiveCoins.Add(
                new Coin(
                    enemy.X + enemy.Width / 2f,
                    enemy.Y + enemy.Height / 2f,
                    value
                )
            );
        }
    }

    private void RemoveDeadObjects()
    {// اگه رفتن بیرون یا مردن حذف بشن 
        PlayerBullets.RemoveAll(b => !b.IsAlive || b.IsOut(screenSize));
        EnemyBullets.RemoveAll(b => !b.IsAlive || b.IsOut(screenSize));
        ActiveCoins.RemoveAll(c => !c.IsAlive || c.Y > screenSize.Height + 30);
        ActivePowerUps.RemoveAll(p => !p.IsAlive || p.Y > screenSize.Height + 30);
        ActiveEnemies.RemoveAll(e => !e.IsAlive || e.Y > screenSize.Height + 110);
    }

    private void CheckWaveEnd()
    {
        // چون ممکنه هست زود تر از  موعد تموم بشه بازی باید این باشد نه این ActiveEnemies.Count > 0
        //نه این ActiveEnemies.Count==0
        
        if (spawnedEnemies < enemiesToSpawn || ActiveEnemies.Count > 0) return;
        // این ین بازی تمومشده هست 
        
        if (CurrentWave >= 10)
        {
            EndGame(true);
            // و برگردد 
            return;
        }
        // عدد ویو اضافع شود 
        CurrentWave++;
        // این پیام رو بنویسه 
        CenterMessage = "Wave " + CurrentWave + " starts soon";
        centerMessageTimer = 2.0f;
        waveDelay = 2.0f;
    }

    private void StartWave(int wave)
    {
        CurrentWave = wave;
        spawnedEnemies = 0;
        // هر بار تعداد انمی های گه اسچان شده بیتر بشه 
        enemiesToSpawn = 5 + wave * 3;
        spawnTimer = 0;
        CenterMessage = "Wave " + wave;
        centerMessageTimer = 2.0f;
    }

    private void EndGame(bool won)
    {
        // اگه گیم اور ردست باشه در بیا 
        if (IsGameOver) return;

        IsGameOver = true;
        // پیام نهایی این شود 
        CenterMessage = won ? "YOU WIN" : "GAME OVER";
        // دیتا بیس اپدیت شود 
        DatabaseManager.AddCoinsAndHighScore(SessionCoins, Score);
        // صدا ها متناسبا اپدیت شن 
        SoundManager.PlayGameOver();
        SoundManager.StopBackgroundMusic();
        // ایونت صدا زده شود 
        GameEnded?.Invoke(Score, SessionCoins);
    }
    // متد های کشیدن 
    public void Draw(Graphics g)
    {
        DrawBackground(g);
        /// روی هر کدوم متد کشیدت صدا زده میشه 
        foreach (Coin coin in ActiveCoins)
        {
            coin.Draw(g);
        }

        foreach (PowerUp powerUp in ActivePowerUps)
        {
            powerUp.Draw(g);
        }

        foreach (Bullet bullet in PlayerBullets)
        {
            bullet.Draw(g);
        }

        foreach (Bullet bullet in EnemyBullets)
        {
            bullet.Draw(g);
        }

        foreach (Enemy enemy in ActiveEnemies)
        {
            enemy.Draw(g);
        }

        Player.Draw(g);
        // قمسمت هد اپ  دیزپلی صدا زده بشع 
        DrawHud(g);

        if (centerMessageTimer > 0 || IsGameOver)
        {   // اگر بازی تموم نشده بود مستطیل زیبای خودم رو بکشه 
            if (!IsGameOver)
            {
                DrawWaveRectangles(g);
            }
            // اگه تموم بشه این کار رو بکن 
            else
            {
                using Font font = new Font("Arial", 30, FontStyle.Bold);
                SizeF size = g.MeasureString(CenterMessage, font);

                g.DrawString(
                    CenterMessage,
                    font,
                    Brushes.White,
                    screenSize.Width / 2f - size.Width / 2f,
                    screenSize.Height / 2f - 40
                );
            }
        }
        // اگر پاز بود اپدیت نمی شود دیگه و این رو می نویس ه 
        if (IsPaused)
        {
            using Font font = new Font("Arial", 32, FontStyle.Bold);

            g.DrawString(
                "PAUSED",
                font,
                Brushes.Yellow,
                screenSize.Width / 2f - 90,
                screenSize.Height / 2f
            );
        }
    }
    // متد برای کشدین بکگراند
    private void DrawBackground(Graphics g)
    {
        Color bg = save.EquippedBackground switch
        {
            "galaxy" => Color.FromArgb(45, 0, 90),
            "neon_city" => Color.FromArgb(5, 25, 70),
            _ => Color.FromArgb(2, 5, 18)
        };

        using Brush brush = new SolidBrush(bg);
        g.FillRectangle(brush, 0, 0, screenSize.Width, screenSize.Height);
    }
    // اون مستطیل زیبا 

    private void DrawWaveRectangles(Graphics g)
    {
        string text = "WAVE " + CurrentWave;

        using Font font = new Font("Arial", 34, FontStyle.Bold);
        SizeF textSize = g.MeasureString(text, font);

        float textX = screenSize.Width / 2f - textSize.Width / 2f;
        float textY = screenSize.Height / 2f - textSize.Height / 2f;

        int count = CurrentWave;

        int baseWidth = (int)textSize.Width + 150;
        int baseHeight = (int)textSize.Height + 90;

        int startX = screenSize.Width / 2 - baseWidth / 2;
        int startY = screenSize.Height / 2 - baseHeight / 2;

        for (int i = 0; i < count; i++)
        {
            int offset = i * 18;

            int x = startX - offset;
            int y = startY - offset / 2;
            int width = baseWidth + offset * 2;
            int height = baseHeight + offset;

            Color color = GetWaveColor(i, count);

            using Pen pen = new Pen(color, 2);
            g.DrawRectangle(pen, x, y, width, height);
        }

        using Brush textBrush = new SolidBrush(Color.Yellow);
        g.DrawString(text, font, textBrush, textX, textY);
    }

    private Color GetWaveColor(int index, int total)
    {
        if (total <= 1)
        {
            return Color.Cyan;
        }

        int r = index * 220 / total;
        int g = 220 - index * 120 / total;
        int b = 255;

        return Color.FromArgb(r, g, b);
    }
    // قسمت مهم کشدیت hud
    private void DrawHud(Graphics g)
    {
        using Font font = new Font("Arial", 11, FontStyle.Bold);
        // برای دادن اطلاعات بیشتر برای بازیکن 
        g.DrawString("Score: " + Score, font, Brushes.White, 10, 10);
        g.DrawString("Coins: " + SessionCoins, font, Brushes.Gold, 125, 10);
        g.DrawString("Wave: " + CurrentWave + "/10", font, Brushes.Cyan, 240, 10);
        g.DrawString("Lives: " + Player.Lives, font, Brushes.Lime, 355, 10);
        // این قسمت برای اینکه مدت زمان بعد از شروع بازی رو به حالت عدد عادی نگه داره 
        // * خوردش رو نشون نده 
        TimeSpan elapsed = TimeSpan.FromSeconds(elapsedGameTime);

        string timeText = elapsed.TotalHours >= 1
        // D2 =   که  عددع اینطوری باشع 02 
        // اگر 12 بود 12 
        // اگر 2 ب.د 02
            ? $"Time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
            : $"Time: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

        g.DrawString(timeText, font, Brushes.White, 10, 34);
        // دورش جون بازیکن خط  سفید 
        // pen bara keshidan khat dor DrawRectangle
        g.DrawRectangle(Pens.White, 455, 12, 145, 14);
        // brush bra por kardan daron 
        g.FillRectangle(Brushes.DarkRed, 456, 13, 143, 12);
        // کشیدن جون بازیکن 
        g.FillRectangle(Brushes.LimeGreen, 456, 13, 143 * Player.HP / (float)Player.MaxHP, 12);

        int powerUpX = 10;
        int powerUpY = 58;
        // این قسمت باری اینکه hud چشم نواز تز
        // و بازیکن بدونه چقدر وقت داره هر کدوم از چاور اپ ها 
        if (Player.IsTripleShotActive)
        {
            DrawPowerUpIndicator(g, "3", Brushes.Orange, Player.TripleShotTimeLeft, powerUpX, powerUpY);
            // این برای اینکخه اگه چند تا پاور اپ شد مختصات ایکس به علاوهی 35 بشه 
            // که نره تو هم 
            powerUpX += 95;
        }

        if (Player.IsShieldActive)
        {
            DrawPowerUpIndicator(g, "S", Brushes.DeepSkyBlue, Player.ShieldTimeLeft, powerUpX, powerUpY);
            powerUpX += 95;
        }

        if (Player.IsFireRateBoostActive)
        {
            DrawPowerUpIndicator(g, "F", Brushes.Violet, Player.FireRateBoostTimeLeft, powerUpX, powerUpY);
            powerUpX += 95;
        }
    }
    // این هم متد کمکی
    // این هم چون نیاز نیست public باشع 
    // private
    // می زنیم 
    private void DrawPowerUpIndicator(Graphics g, string iconText, Brush iconBrush, float timeLeft, int x, int y)
    {
        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
        using Font iconFont = new Font("Arial", 12, FontStyle.Bold);
        using Font timerFont = new Font("Arial", 10, FontStyle.Bold);

        g.FillRectangle(backgroundBrush, x - 3, y - 3, 82, 31);
        g.DrawRectangle(Pens.White, x - 3, y - 3, 82, 31);

        g.FillEllipse(iconBrush, x, y, 24, 24);
        g.DrawEllipse(Pens.White, x, y, 24, 24);

        SizeF iconSize = g.MeasureString(iconText, iconFont);
        g.DrawString(iconText, iconFont, Brushes.Black, x + 12 - iconSize.Width / 2f, y + 3);

        string timerText = Math.Ceiling(timeLeft).ToString("0") + "s";
        g.DrawString(timerText, timerFont, Brushes.White, x + 31, y + 5);
    }
}
