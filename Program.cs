using SpaceShooterFinalWithSounds.Data;
using SpaceShooterFinalWithSounds.Forms;

namespace SpaceShooterFinalWithSounds;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseManager.Init();
        Application.Run(new MainMenuForm());
    }
}
