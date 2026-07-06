namespace SpaceShooterFinalWithSounds.Game;

public class ShooterEnemy : Enemy
{
    private float shootTimer;

    public ShooterEnemy(float x, float y, int wave)
        : base(x, y, 46, 46, 48 + wave * 2, 1.35f + wave * 0.12f, 130, 45, 11)
    {
    }

    public override void Move(Player player, float deltaTime)
    {
        Y += Speed * deltaTime * 60f;
        shootTimer += deltaTime;
    }

    public override List<Bullet> TryShoot(Player player, string style)
    {
        if (shootTimer < 1.5f) return new List<Bullet>();
        shootTimer = 0;
        return new List<Bullet> { new Bullet(X + Width / 2f - 3, Y + Height, 0, 6.8f, 12, false) };
    }

    public override void Draw(Graphics g)
    {
        g.FillRectangle(Brushes.MediumPurple, X, Y, Width, Height);
        g.DrawRectangle(Pens.White, X, Y, Width, Height);
        DrawMiniHealthBar(g);
    }
}
