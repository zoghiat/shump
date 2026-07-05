namespace SpaceShooterFinalWithSounds.Game;

public class ScoutEnemy : Enemy
{
    private readonly float startX;
    private float time;

    public ScoutEnemy(float x, float y, int wave)
        : base(x, y, 34, 34, 20 + wave * 2, 2.7f + wave * 0.20f, 80, 35, 8)
    {
        startX = x;
    }
    // azz sin estefade shode ke harekat zig zagi bashe
    // nemoedar zarb dar 5 shode ke zigzagi tar va geher ghable pishbine tar 
    public override void Move(Player player, float deltaTime)
    {
        time += deltaTime * 5f;
        Y += Speed * deltaTime * 60f;
        X = startX + MathF.Sin(time) * 55f;
    }

    public override void Draw(Graphics g)
    {
        g.FillEllipse(Brushes.Yellow, X, Y, Width, Height);
        g.DrawEllipse(Pens.White, X, Y, Width, Height);
    }
}
