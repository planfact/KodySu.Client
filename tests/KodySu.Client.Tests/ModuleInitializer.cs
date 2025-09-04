using System.Runtime.CompilerServices;

namespace KodySu.Client.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Инициализация Verify для XUnit
        VerifierSettings.InitializePlugins();
    }
}
