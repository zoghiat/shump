namespace SpaceShooterFinalWithSounds.Game;

public class Bullet : GameObject
{
    public float VX { get; set; }
    public float VY { get; set; }
    public int Damage { get; set; }
    // to sure its a player IsPlayerBullet or not 
    public bool IsPlayerBullet { get; set; }
    public string Style { get; set; }
    // the last pparamter is dufulet that if we dont write that it gonna be default
    public Bullet(float x, float y, float vx, float vy, int damage, bool isPlayerBullet, string style = "default")
        : base(x, y, 7, 16)
    {
        VX = vx;
        VY = vy;
        Damage = damage;
        IsPlayerBullet = isPlayerBullet;
        Style = style;
    }
    // from our game obj that we peromise that we have to do Draw and update
    public override void Update(float deltaTime)
    {
        X += VX * deltaTime * 60f;
        Y += VY * deltaTime * 60f;
    }
    // use is put to sure is in 
    public bool IsOut(Size screenSize)
    {
        return X < -80 || X > screenSize.Width + 80 || Y < -80 || Y > screenSize.Height + 80;
    }
    

    public override void Draw(Graphics g)
    {
        Brush brush;
        if (!IsPlayerBullet) brush = Brushes.Red;
        else if (Style == "green_laser") brush = Brushes.Lime;
        else if (Style == "plasma") brush = Brushes.DeepSkyBlue;
        else brush = Brushes.Gold;

        g.FillRectangle(brush, X, Y, Width, Height);
        g.DrawRectangle(Pens.White, X, Y, Width, Height);
    }
}
