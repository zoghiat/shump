using SpaceShooterFinalWithSounds.Core;
using SpaceShooterFinalWithSounds.Data;
using SpaceShooterFinalWithSounds.Game;

namespace SpaceShooterFinalWithSounds.Forms;

public class ShopForm : Form
{
    private readonly Label coinsLabel = new Label();
    private readonly FlowLayoutPanel panel = new FlowLayoutPanel();

    public ShopForm()
    {
        Text = "Shop";
        ClientSize = new Size(740, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(12, 12, 28);

        coinsLabel.ForeColor = Color.Gold;
        coinsLabel.Font = new Font("Arial", 16, FontStyle.Bold);
        coinsLabel.Location = new Point(25, 20);
        coinsLabel.Size = new Size(680, 40);
        Controls.Add(coinsLabel);

        panel.Location = new Point(25, 75);
        panel.Size = new Size(690, 450);
        panel.AutoScroll = true;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        Controls.Add(panel);

        LoadShop();
    }

    private void LoadShop()
    {
        panel.Controls.Clear();
        PlayerSave save = DatabaseManager.LoadPlayerSave();
        coinsLabel.Text = $"Total Coins: {save.TotalCoins}";

        List<ShopItem> items = DatabaseManager.GetShopItems();
        foreach (ShopItem item in items)
        {
            Panel row = new Panel
            {
                Size = new Size(650, 65),
                BackColor = Color.FromArgb(25, 30, 55),
                Margin = new Padding(4, 4, 4, 8)
            };

            Label label = new Label
            {
                Text = $"{item.Name}\nType: {item.Type} | Price: {item.Price} | {(item.Purchased ? "Purchased" : "Not purchased")} {(item.Equipped ? "| Equipped" : "")}",
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(12, 9),
                Size = new Size(410, 50)
            };

            Button buy = new Button
            {
                Text = item.Purchased ? "Bought" : "Buy",
                Enabled = !item.Purchased,
                Location = new Point(430, 18),
                Size = new Size(90, 32)
            };
            buy.Click += (_, _) =>
            {
                if (DatabaseManager.BuyItem(item.Key, out string message)) SoundManager.PlayCoin();
                MessageBox.Show(message);
                LoadShop();
            };

            Button equip = new Button
            {
                Text = item.Equipped ? "Equipped" : "Equip",
                Enabled = item.Purchased && !item.Equipped,
                Location = new Point(530, 18),
                Size = new Size(90, 32)
            };
            equip.Click += (_, _) =>
            {
                if (DatabaseManager.EquipItem(item.Key, out string message)) SoundManager.PlayPowerUp();
                MessageBox.Show(message);
                LoadShop();
            };

            row.Controls.Add(label);
            row.Controls.Add(buy);
            row.Controls.Add(equip);
            panel.Controls.Add(row);
        }
    }
}
