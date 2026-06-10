namespace SpaceShooterFinalWithSounds.Game;

public abstract class GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsAlive { get; set; } = true;

    public Rectangle Bounds => new Rectangle((int)X, (int)Y, Width, Height);

    protected GameObject(float x, float y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public abstract void Update(float deltaTime);
    public abstract void Draw(Graphics g);
}
