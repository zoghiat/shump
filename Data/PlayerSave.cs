namespace SpaceShooterFinalWithSounds.Data;

public class PlayerSave
{
    public int TotalCoins { get; set; }
    public int HighScore { get; set; }
    public string EquippedShipSkin { get; set; } = "default_ship";
    public string EquippedBulletStyle { get; set; } = "default_bullet";
    public string EquippedBackground { get; set; } = "default_space";
    public int ExtraLifeBooster { get; set; }
}
