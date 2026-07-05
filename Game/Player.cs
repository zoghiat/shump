namespace SpaceShooterFinalWithSounds.Game;

public class Player : GameObject
{
    public int HP { get; set; } = 100;
    public int MaxHP { get; set; } = 100;
    public int Lives { get; set; } = 3;
    public float Speed { get; set; } = 5.7f;
    public string ShipSkin { get; set; } = "default_ship";

    public bool IsShieldActive => shieldTime > 0;
    public bool IsTripleShotActive => tripleShotTime > 0;
    public bool IsFireRateBoostActive => fireRateBoostTime > 0;
    public float ShieldTimeLeft => Math.Max(0, shieldTime);
    public float TripleShotTimeLeft => Math.Max(0, tripleShotTime);
    public float FireRateBoostTimeLeft => Math.Max(0, fireRateBoostTime);
    private bool moveLeft;
    private bool moveRight;
    private bool moveUp;
    private bool moveDown;
    private float shieldTime;
    private float tripleShotTime;
    private float fireRateBoostTime;
    // sazande
    public Player(float x, float y, int extraLives, string shipSkin) : base(x, y, 44, 48)
    {
        Lives = 3 + extraLives;
        ShipSkin = shipSkin;
    }

    public void ApplyInput(bool left, bool right, bool up, bool down)
    {
        moveLeft = left;
        moveRight = right;
        moveUp = up;
        moveDown = down;
    }
    // for movin   
    public override void Update(float deltaTime)
    {
        float distance = Speed * deltaTime * 60f;

        if (moveLeft) X -= distance;
        if (moveRight) X += distance;
        if (moveUp) Y -= distance;
        if (moveDown) Y += distance;

        if (tripleShotTime > 0) tripleShotTime = Math.Max(0, tripleShotTime - deltaTime);
        if (shieldTime > 0) shieldTime = Math.Max(0, shieldTime - deltaTime);
        if (fireRateBoostTime > 0) fireRateBoostTime = Math.Max(0, fireRateBoostTime - deltaTime);
    }

    public void KeepInside(Size screenSize)
    {
        // clamp will promise our var will be in the range 
        X = Math.Clamp(X, 0, screenSize.Width - Width);
        Y = Math.Clamp(Y, 45, screenSize.Height - Height);
    }

    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            
            case PowerUpType.TripleShot:
                tripleShotTime = 10f;
                break;

            case PowerUpType.Shield:
                shieldTime = 5f;
                break;

            case PowerUpType.HealthPack:
                HP = Math.Min(MaxHP, HP + 40);
                if (HP == MaxHP) Lives = Math.Min(5, Lives + 1);
                break;

            case PowerUpType.FireRateBooster:
                fireRateBoostTime = 10f;
                break;
        }
    }

    public bool TakeDamage(int damage)
    {
        // age shilde bashwe damage nemikhore
        if (IsShieldActive) return false;

        HP -= damage;

        if (HP <= 0)
        {
            Lives--;
            HP = MaxHP;

            if (Lives <= 0)
            {
                IsAlive = false;
            }
        }

        return true;
    }

    public override void Draw(Graphics g)
    {
        // bar asass chize ke kharede
        switch (ShipSkin)
        {
            case "red_eagle":
                DrawRedEagle(g);
                break;

            case "cyber_ghost":
                DrawCyberGhost(g);
                break;

            default:
                DrawDefaultShip(g);
                break;
        }
        // keshidan dayere abi

        if (IsShieldActive)
        {
            using Pen pen = new Pen(Color.DeepSkyBlue, 3);
            g.DrawEllipse(pen, X - 7, Y - 7, Width + 14, Height + 14);
        }
    }

    private void DrawDefaultShip(Graphics g)
    {
        PointF[] ship =
        {
            new PointF(X + Width / 2f, Y),
            new PointF(X + 5, Y + Height),
            new PointF(X + Width / 2f, Y + Height - 12),
            new PointF(X + Width - 5, Y + Height)
        };

        g.FillPolygon(Brushes.Cyan, ship);
        g.DrawPolygon(Pens.White, ship);

        g.FillEllipse(Brushes.White, X + Width / 2f - 4, Y + 17, 8, 8);
    }
    // oghab
    private void DrawRedEagle(Graphics g)
    {
        PointF[] body =
        {
            new PointF(X + Width / 2f, Y - 3),
            new PointF(X + 3, Y + Height),
            new PointF(X + Width / 2f, Y + Height - 15),
            new PointF(X + Width - 3, Y + Height)
        };

        PointF[] leftWing =
        {
            new PointF(X + 6, Y + 22),
            new PointF(X - 7, Y + Height - 4),
            new PointF(X + 15, Y + Height - 10)
        };

        PointF[] rightWing =
        {
            new PointF(X + Width - 6, Y + 22),
            new PointF(X + Width + 7, Y + Height - 4),
            new PointF(X + Width - 15, Y + Height - 10)
        };

        g.FillPolygon(Brushes.DarkRed, leftWing);
        g.FillPolygon(Brushes.DarkRed, rightWing);
        g.FillPolygon(Brushes.Red, body);

        using Pen orangePen = new Pen(Color.Orange, 3);
        g.DrawLine(orangePen, X + Width / 2f, Y + 7, X + Width / 2f, Y + Height - 10);

        g.DrawPolygon(Pens.White, body);
        g.DrawPolygon(Pens.Orange, leftWing);
        g.DrawPolygon(Pens.Orange, rightWing);

        g.FillEllipse(Brushes.Gold, X + Width / 2f - 4, Y + 15, 8, 8);
    }
    //CyberGhost
    private void DrawCyberGhost(Graphics g)
    {
        PointF[] body =
        {
            new PointF(X + Width / 2f, Y),
            new PointF(X + 7, Y + Height),
            new PointF(X + Width / 2f, Y + Height - 10),
            new PointF(X + Width - 7, Y + Height)
        };

        using SolidBrush ghostBrush = new SolidBrush(Color.FromArgb(170, 180, 255, 255));
        using Pen neonPen = new Pen(Color.MediumPurple, 2);
        using Pen glowPen = new Pen(Color.Cyan, 2);

        g.FillPolygon(ghostBrush, body);
        g.DrawPolygon(neonPen, body);

        g.DrawLine(glowPen, X + Width / 2f, Y + 5, X + 10, Y + Height - 5);
        g.DrawLine(glowPen, X + Width / 2f, Y + 5, X + Width - 10, Y + Height - 5);

        g.FillEllipse(Brushes.MediumPurple, X + Width / 2f - 5, Y + 16, 10, 10);
        g.FillRectangle(Brushes.Cyan, X + 12, Y + Height - 8, 6, 8);
        g.FillRectangle(Brushes.Cyan, X + Width - 18, Y + Height - 8, 6, 8);
    }
}
