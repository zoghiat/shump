namespace SpaceShooterFinalWithSounds.Game;

public class Player : GameObject
{
    public int HP { get; set; } = 100;
    public int MaxHP { get; set; } = 100;
    public int Lives { get; set; } = 3;
    public float VX { get; set; }
    public float VY { get; set; }
    public float WeaponHeat { get; set; }
    public bool IsOverheated { get; set; }
    public bool IsShieldActive => shieldTime > 0;
    public bool IsTripleShotActive => tripleShotTime > 0;
    public bool IsFireRateBoostActive => fireRateBoostTime > 0;

    private float shieldTime;
    private float tripleShotTime;
    private float fireRateBoostTime;

    public Player(float x, float y, int extraLives) : base(x, y, 44, 48)
    {
        Lives = 3 + extraLives;
    }

    public void ApplyInput(bool left, bool right, bool up, bool down)
    {
        const float acceleration = 0.65f;
        if (left) VX -= acceleration;
        if (right) VX += acceleration;
        if (up) VY -= acceleration;
        if (down) VY += acceleration;

        VX = Math.Clamp(VX, -8.5f, 8.5f);
        VY = Math.Clamp(VY, -8.5f, 8.5f);
    }

    public override void Update(float deltaTime)
    {
        X += VX * deltaTime * 60f;
        Y += VY * deltaTime * 60f;

        VX *= 0.92f;
        VY *= 0.92f;
        if (Math.Abs(VX) < 0.05f) VX = 0;
        if (Math.Abs(VY) < 0.05f) VY = 0;

        if (tripleShotTime > 0) tripleShotTime -= deltaTime;
        if (shieldTime > 0) shieldTime -= deltaTime;
        if (fireRateBoostTime > 0) fireRateBoostTime -= deltaTime;

        if (WeaponHeat > 0)
        {
            WeaponHeat -= 25f * deltaTime;
            if (WeaponHeat < 0) WeaponHeat = 0;
        }

        if (IsOverheated && WeaponHeat <= 35f)
            IsOverheated = false;
    }

    public void KeepInside(Size screenSize)
    {
        X = Math.Clamp(X, 0, screenSize.Width - Width);
        Y = Math.Clamp(Y, 45, screenSize.Height - Height);
    }

    public bool CanShoot()
    {
        return !IsOverheated && WeaponHeat < 100f;
    }

    public void AddHeat()
    {
        WeaponHeat += 14.5f;
        if (WeaponHeat > 100f)
        {
            WeaponHeat = 100f;
            IsOverheated = true;
        }
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
        PointF[] ship =
        {
            new PointF(X + Width / 2f, Y),
            new PointF(X + 5, Y + Height),
            new PointF(X + Width / 2f, Y + Height - 12),
            new PointF(X + Width - 5, Y + Height)
        };

        g.FillPolygon(Brushes.Cyan, ship);
        g.DrawPolygon(Pens.White, ship);

        if (IsShieldActive)
        {
            using Pen pen = new Pen(Color.DeepSkyBlue, 3);
            g.DrawEllipse(pen, X - 7, Y - 7, Width + 14, Height + 14);
        }
    }
}
