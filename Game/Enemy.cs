namespace SpaceShooterFinalWithSounds.Game;

public abstract class Enemy : GameObject
{
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public float Speed { get; set; }
    public int ScoreValue { get; set; }
    public int CoinDropChance { get; set; }
    public int PowerUpDropChance { get; set; }

    protected Enemy(float x, float y, int width, int height, int hp, float speed, int score, int coinChance, int powerChance)
        : base(x, y, width, height)
    {
        HP = hp;
        MaxHP = hp;
        Speed = speed;
        ScoreValue = score;
        CoinDropChance = coinChance;
        PowerUpDropChance = powerChance;
    }

    public abstract void Move(Player player, float deltaTime);

    public virtual List<Bullet> TryShoot(Player player, string style)
    {
        return new List<Bullet>();
    }

    public override void Update(float deltaTime)
    {
        Y += Speed * deltaTime * 60f;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0) IsAlive = false;
    }

    protected void DrawMiniHealthBar(Graphics g, bool rainbow = false)
    {
        g.DrawRectangle(Pens.White, X, Y - 10, Width, 6);
        g.FillRectangle(Brushes.DarkRed, X + 1, Y - 9, Width - 2, 4);
        float ratio = Math.Clamp(HP / (float)MaxHP, 0, 1);

        Brush brush = Brushes.Lime;
        if (rainbow)
        {
            int tick = Environment.TickCount / 6;
            Color color = ColorFromHsl((tick % 360) / 360.0, 0.9, 0.55);
            brush = new SolidBrush(color);
        }

        g.FillRectangle(brush, X + 1, Y - 9, (Width - 2) * ratio, 4);
        if (rainbow) brush.Dispose();
    }

    private static Color ColorFromHsl(double h, double s, double l)
    {
        double r, g, b;
        if (s == 0) r = g = b = l;
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }
        return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
}
