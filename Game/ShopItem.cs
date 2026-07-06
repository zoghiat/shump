namespace SpaceShooterFinalWithSounds.Game;

public class ShopItem
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public ShopItemType Type { get; set; }
    public bool Purchased { get; set; }
    public bool Equipped { get; set; }
}
