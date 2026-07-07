namespace SpaceShooterFinalWithSounds.Game;

public class PowerUp : GameObject
{
    public PowerUpType Type { get; set; }

    public PowerUp(float x, float y, PowerUpType type) : base(x, y, 28, 28)
    {
        Type = type;
    }

    public override void Update(float deltaTime)
    {
        Y += 2.2f * deltaTime * 60f;
    }

    public override void Draw(Graphics g)
    {
        Brush brush = Type switch
        {
            PowerUpType.TripleShot => Brushes.Orange,
            PowerUpType.Shield => Brushes.DeepSkyBlue,
            PowerUpType.HealthPack => Brushes.LimeGreen,
            PowerUpType.FireRateBooster => Brushes.Violet,
            _ => Brushes.White
        };

        g.FillEllipse(brush, X, Y, Width, Height);
        g.DrawEllipse(Pens.White, X, Y, Width, Height);

        string text = Type switch
        {
            PowerUpType.TripleShot => "3",
            PowerUpType.Shield => "S",
            PowerUpType.HealthPack => "+",
            PowerUpType.FireRateBooster => "F",
            _ => "?"
        };

        using Font font = new Font("Arial", 13, FontStyle.Bold);
        g.DrawString(text, font, Brushes.Black, X + 7, Y + 4);
    }
}
