namespace SpaceShooterFinalWithSounds.Game;

public class TerroristEnemy : Enemy
{
    public TerroristEnemy(float x, float y, int wave)
        : base(x, y, 42, 42, 75 + wave * 3, 2.25f + wave * 0.1f, 220, 70, 12)
    {
    }

    public override void Move(Player player, float deltaTime)
    {
        float dx = player.X - X;
        float dy = player.Y - Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);
        if (length > 0)
        {
            X += dx / length * Speed * deltaTime * 60f;
            Y += dy / length * Speed * deltaTime * 60f;
        }
    }

    public override void Draw(Graphics g)
    {
        g.FillEllipse(Brushes.Red, X, Y, Width, Height);
        g.DrawEllipse(Pens.White, X, Y, Width, Height);
        using Font font = new Font("Arial", 18, FontStyle.Bold);
        g.DrawString("!", font, Brushes.White, X + 14, Y + 7);
        DrawMiniHealthBar(g);
    }
}
