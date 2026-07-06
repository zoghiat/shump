namespace SpaceShooterFinalWithSounds.Game;

public class HeavyTankEnemy : Enemy
{
    private float shootTimer;

    public HeavyTankEnemy(float x, float y, int wave)
        : base(x, y, 82, 72, 130 + wave * 6, 0.85f + wave * 0.03f, 340, 85, 18)
    {
    }

    public override void Move(Player player, float deltaTime)
    {
        Y += Speed * deltaTime * 60f;
        shootTimer += deltaTime;
    }

    public override List<Bullet> TryShoot(Player player, string style)
    {
        if (shootTimer < 2.1f) return new List<Bullet>();
        shootTimer = 0;
        float cx = X + Width / 2f;
        float cy = Y + Height / 2f;
        return new List<Bullet>
        {
            new Bullet(cx, cy, 0, -4.5f, 18, false),
            new Bullet(cx, cy, 0, 4.5f, 18, false),
            new Bullet(cx, cy, -4.5f, 0, 18, false),
            new Bullet(cx, cy, 4.5f, 0, 18, false),
            new Bullet(cx, cy, -3.5f, -3.5f, 18, false),
            new Bullet(cx, cy, 3.5f, -3.5f, 18, false),
            new Bullet(cx, cy, -3.5f, 3.5f, 18, false),
            new Bullet(cx, cy, 3.5f, 3.5f, 18, false)
        };
    }

    public override void Draw(Graphics g)
    {
        g.FillRectangle(Brushes.DarkRed, X, Y, Width, Height);
        g.DrawRectangle(Pens.White, X, Y, Width, Height);
        g.FillEllipse(Brushes.Black, X + 20, Y + 18, 42, 34);
        DrawMiniHealthBar(g);
    }
}
