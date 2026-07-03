namespace SpaceShooterFinalWithSounds.Game;

public class Coin : GameObject
{
    public int Value { get; set; }

    public Coin(float x, float y, int value) : base(x, y, 18, 18)
    {
        Value = value;
    }
    
    // THAT WILL MOVE THE COIN TO BOTTUN OF THE BOARD ;
    public override void Update(float deltaTime)
    {
        Y += 2.6f * deltaTime * 60f;
    }
    // GONNA DRAW A CIRCLE FOR OUR COINS
    public override void Draw(Graphics g)
    {
        Brush brush = Value == 5 ? Brushes.Gold : Brushes.Silver;
        g.FillEllipse(brush, X, Y, Width, Height);
        g.DrawEllipse(Pens.White, X, Y, Width, Height);
        using Font font = new Font("Arial", 7, FontStyle.Bold);
        g.DrawString(Value.ToString(), font, Brushes.Black, X + 5, Y + 4);
    }
}
