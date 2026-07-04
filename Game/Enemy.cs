namespace SpaceShooterFinalWithSounds.Game;

public abstract class Enemy : GameObject
{
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public float Speed { get; set; }
    public int ScoreValue { get; set; }
    public int CoinDropChance { get; set; }
    public int PowerUpDropChance { get; set; }

    protected Enemy(float x, float y, int width, int height, int hp, float speed, int score, int coinChance, int powerChance)
        : base(x, y, width, height)
    {
        HP = hp;
        MaxHP = hp;
        Speed = speed;
        ScoreValue = score;
        CoinDropChance = coinChance;
        PowerUpDropChance = powerChance;
    }

    public abstract void Move(Player player, float deltaTime);

    public virtual List<Bullet> TryShoot(Player player, string style)
    {
        return new List<Bullet>();
    }

    public override void Update(float deltaTime)
    {
        Y += Speed * deltaTime * 60f;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0) IsAlive = false;
    }

    protected void DrawMiniHealthBar(Graphics g)
    {
        g.DrawRectangle(Pens.White, X, Y - 10, Width, 6);
        g.FillRectangle(Brushes.DarkRed, X + 1, Y - 9, Width - 2, 4);
        float ratio = Math.Clamp(HP / (float)MaxHP, 0, 1);
        g.FillRectangle(Brushes.Lime, X + 1, Y - 9, (Width - 2) * ratio, 4);
    }
}
