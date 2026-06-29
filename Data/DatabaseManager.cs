using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using SpaceShooterFinalWithSounds.Game;

namespace SpaceShooterFinalWithSounds.Data;

public static class DatabaseManager
{
    private const string ConnectionString = "Data Source=game.db";
    private const string Salt = "IUST_AP_4042";

    public static void Init()
    {
        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();

        Execute(connection,
        """
        CREATE TABLE IF NOT EXISTS PlayerData
        (
            Id INTEGER PRIMARY KEY,
            TotalCoins INTEGER NOT NULL,
            HighScore INTEGER NOT NULL,
            EquippedShipSkin TEXT NOT NULL,
            EquippedBulletStyle TEXT NOT NULL,
            EquippedBackground TEXT NOT NULL,
            ExtraLifeBooster INTEGER NOT NULL,
            coin_verification_hash TEXT NOT NULL
        );
        """);

        Execute(connection,
        """
        CREATE TABLE IF NOT EXISTS ShopItems
        (
            ItemKey TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            Price INTEGER NOT NULL,
            ItemType TEXT NOT NULL,
            Purchased INTEGER NOT NULL,
            Equipped INTEGER NOT NULL
        );
        """);

        using SqliteCommand check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM PlayerData WHERE Id = 1";
        long count = (long)(check.ExecuteScalar() ?? 0L);
        if (count == 0)
        {
            using SqliteCommand insert = connection.CreateCommand();
            insert.CommandText =
            """
            INSERT INTO PlayerData(Id, TotalCoins, HighScore, EquippedShipSkin, EquippedBulletStyle, EquippedBackground, ExtraLifeBooster, coin_verification_hash)
            VALUES(1, 0, 0, 'default_ship', 'default_bullet', 'default_space', 0, $hash)
            """;
            insert.Parameters.AddWithValue("$hash", ComputeCoinHash(0));
            insert.ExecuteNonQuery();
        }

        InsertDefaultShopItems(connection);
    }

    private static void InsertDefaultShopItems(SqliteConnection connection)
    {
        AddItem(connection, "red_eagle", "Red Eagle Ship Skin", 80, ShopItemType.ShipSkin, false, false);
        AddItem(connection, "cyber_ghost", "Cyber Ghost Ship Skin", 140, ShopItemType.ShipSkin, false, false);
        AddItem(connection, "green_laser", "Green Laser Bullet", 60, ShopItemType.BulletStyle, false, false);
        AddItem(connection, "plasma", "Plasma Bullet", 120, ShopItemType.BulletStyle, false, false);
        AddItem(connection, "galaxy", "Deep Galaxy Background", 100, ShopItemType.BackgroundTheme, false, false);
        AddItem(connection, "neon_city", "Neon City Background", 160, ShopItemType.BackgroundTheme, false, false);
        AddItem(connection, "extra_life", "Extra Life Booster", 150, ShopItemType.ExtraLifeBooster, false, false);
    }

    private static void AddItem(SqliteConnection connection, string key, string name, int price, ShopItemType type, bool purchased, bool equipped)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText =
        """
        INSERT OR IGNORE INTO ShopItems(ItemKey, Name, Price, ItemType, Purchased, Equipped)
        VALUES($key, $name, $price, $type, $purchased, $equipped)
        """;
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$price", price);
        cmd.Parameters.AddWithValue("$type", type.ToString());
        cmd.Parameters.AddWithValue("$purchased", purchased ? 1 : 0);
        cmd.Parameters.AddWithValue("$equipped", equipped ? 1 : 0);
        cmd.ExecuteNonQuery();
    }

    public static PlayerSave LoadPlayerSave()
    {
        Init();
        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT TotalCoins, HighScore, EquippedShipSkin, EquippedBulletStyle, EquippedBackground, ExtraLifeBooster, coin_verification_hash FROM PlayerData WHERE Id = 1";
        using SqliteDataReader reader = cmd.ExecuteReader();
        if (!reader.Read()) return new PlayerSave();

        int coins = reader.GetInt32(0);
        string hash = reader.GetString(6);
        if (hash != ComputeCoinHash(coins))
        {
            coins = 0;
            SavePlayerSave(new PlayerSave { TotalCoins = 0, HighScore = reader.GetInt32(1) });
        }

        return new PlayerSave
        {
            TotalCoins = coins,
            HighScore = reader.GetInt32(1),
            EquippedShipSkin = reader.GetString(2),
            EquippedBulletStyle = reader.GetString(3),
            EquippedBackground = reader.GetString(4),
            ExtraLifeBooster = reader.GetInt32(5)
        };
    }

    public static void SavePlayerSave(PlayerSave save)
    {
        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText =
        """
        UPDATE PlayerData
        SET TotalCoins = $coins,
            HighScore = $score,
            EquippedShipSkin = $ship,
            EquippedBulletStyle = $bullet,
            EquippedBackground = $background,
            ExtraLifeBooster = $extra,
            coin_verification_hash = $hash
        WHERE Id = 1
        """;
        cmd.Parameters.AddWithValue("$coins", save.TotalCoins);
        cmd.Parameters.AddWithValue("$score", save.HighScore);
        cmd.Parameters.AddWithValue("$ship", save.EquippedShipSkin);
        cmd.Parameters.AddWithValue("$bullet", save.EquippedBulletStyle);
        cmd.Parameters.AddWithValue("$background", save.EquippedBackground);
        cmd.Parameters.AddWithValue("$extra", save.ExtraLifeBooster);
        cmd.Parameters.AddWithValue("$hash", ComputeCoinHash(save.TotalCoins));
        cmd.ExecuteNonQuery();
    }

    public static void AddCoinsAndHighScore(int sessionCoins, int score)
    {
        PlayerSave save = LoadPlayerSave();
        save.TotalCoins += sessionCoins;
        if (score > save.HighScore) save.HighScore = score;
        SavePlayerSave(save);
    }

    public static List<ShopItem> GetShopItems()
    {
        Init();
        List<ShopItem> items = new List<ShopItem>();
        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT ItemKey, Name, Price, ItemType, Purchased, Equipped FROM ShopItems ORDER BY Price";
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new ShopItem
            {
                Key = reader.GetString(0),
                Name = reader.GetString(1),
                Price = reader.GetInt32(2),
                Type = Enum.Parse<ShopItemType>(reader.GetString(3)),
                Purchased = reader.GetInt32(4) == 1,
                Equipped = reader.GetInt32(5) == 1
            });
        }
        return items;
    }

    public static bool BuyItem(string key, out string message)
    {
        PlayerSave save = LoadPlayerSave();
        ShopItem? item = GetShopItems().FirstOrDefault(x => x.Key == key);
        if (item == null)
        {
            message = "Item not found.";
            return false;
        }
        if (item.Purchased)
        {
            message = "This item has already been purchased.";
            return false;
        }
        if (save.TotalCoins < item.Price)
        {
            message = "Not enough coins.";
            return false;
        }

        save.TotalCoins -= item.Price;
        SavePlayerSave(save);

        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE ShopItems SET Purchased = 1 WHERE ItemKey = $key";
        cmd.Parameters.AddWithValue("$key", key);
        cmd.ExecuteNonQuery();

        message = "Purchased successfully.";
        return true;
    }

    public static bool EquipItem(string key, out string message)
    {
        PlayerSave save = LoadPlayerSave();
        ShopItem? item = GetShopItems().FirstOrDefault(x => x.Key == key);
        if (item == null || !item.Purchased)
        {
            message = "You must buy this item first.";
            return false;
        }

        using SqliteConnection connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();

        using SqliteCommand clear = connection.CreateCommand();
        clear.Transaction = transaction;
        clear.CommandText = "UPDATE ShopItems SET Equipped = 0 WHERE ItemType = $type";
        clear.Parameters.AddWithValue("$type", item.Type.ToString());
        clear.ExecuteNonQuery();

        using SqliteCommand equip = connection.CreateCommand();
        equip.Transaction = transaction;
        equip.CommandText = "UPDATE ShopItems SET Equipped = 1 WHERE ItemKey = $key";
        equip.Parameters.AddWithValue("$key", key);
        equip.ExecuteNonQuery();

        transaction.Commit();

        switch (item.Type)
        {
            case ShopItemType.ShipSkin:
                save.EquippedShipSkin = item.Key;
                break;
            case ShopItemType.BulletStyle:
                save.EquippedBulletStyle = item.Key;
                break;
            case ShopItemType.BackgroundTheme:
                save.EquippedBackground = item.Key;
                break;
            case ShopItemType.ExtraLifeBooster:
                save.ExtraLifeBooster = 1;
                break;
        }

        SavePlayerSave(save);
        message = "Equipped successfully.";
        return true;
    }

    public static void ConsumeExtraLifeBooster()
    {
        PlayerSave save = LoadPlayerSave();
        if (save.ExtraLifeBooster <= 0) return;
        save.ExtraLifeBooster = 0;
        SavePlayerSave(save);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static string ComputeCoinHash(int coins)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(coins + Salt));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
