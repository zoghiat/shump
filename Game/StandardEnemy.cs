namespace SpaceShooterFinalWithSounds.Game;

public class StandardEnemy : Enemy
{
    public StandardEnemy(float x, float y, int wave)
        : base(x, y, 38, 38, 22 + wave * 2, 2.1f + wave * 0.15f, 50, 30, 6)
    {
    }
    // faghat harekat be taraf paiin
    public override void Move(Player player, float deltaTime)
    {
        Y += Speed * deltaTime * 60f;
    }
    
    public override void Draw(Graphics g)
    {
        g.FillEllipse(Brushes.OrangeRed, X, Y, Width, Height);
        g.DrawEllipse(Pens.White, X, Y, Width, Height);
    }
}
