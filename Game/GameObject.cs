namespace SpaceShooterFinalWithSounds.Game;
// the base object that all thing in game To inherit
public abstract class GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsAlive { get; set; } = true;
    // all obj in game are Rectangle
    public Rectangle Bounds => new Rectangle((int)X, (int)Y, Width, Height);
    // we use protected here to No one from outside can change that but the Descendants use that
    protected GameObject(float x, float y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    // the abstract method that Descendants will fill that
    public abstract void Update(float deltaTime);
    public abstract void Draw(Graphics g);
}
