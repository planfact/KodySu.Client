using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Planfact.KodySu.Client.Tests;

/// <summary>
/// Тесты для проверки отсутствия проблем с captured dependencies
/// </summary>
public class CapturedDependencyTests
{
    [Fact]
    public void KodySuClient_CanBeCreated_WithoutCapturedDependencies()
    {
        // Arrange - проверяем, что клиент может быть создан без captured dependencies
        var configDict = new Dictionary<string, string?>
        {
            ["KodySuOptions:ApiKey"] = "test-key",
            ["KodySuOptions:BaseUrl"] = "https://api.test.com",
            ["KodySuOptions:TimeoutSeconds"] = "30",
            ["KodySuOptions:UserAgent"] = "Test-Client/1.0"
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();

        // Добавляем клиент используя наш extension method
        services.AddKodySuClient(configuration);

        // Act & Assert - если есть captured dependency, то билд service provider выдаст исключение или клиент не создастся
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IKodySuClient client = serviceProvider.GetRequiredService<IKodySuClient>();

        // Проверяем, что клиент был корректно создан
        client.Should().BeOfType<KodySuClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void CachedKodySuClient_CanBeCreated_WithoutCapturedDependencies()
    {
        // Arrange - проверяем, что кешированный клиент может быть создан без captured dependencies
        var configDict = new Dictionary<string, string?>
        {
            ["KodySuOptions:ApiKey"] = "test-key",
            ["KodySuOptions:BaseUrl"] = "https://api.test.com",
            ["KodySuOptions:TimeoutSeconds"] = "30",
            ["KodySuOptions:UserAgent"] = "Test-Client/1.0"
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();

        // Добавляем кешированный клиент
        services.AddCachedKodySuClient(configuration);

        // Act & Assert - если есть captured dependency, то билд service provider выдаст исключение или клиент не создастся
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IKodySuClient client = serviceProvider.GetRequiredService<IKodySuClient>();

        // Проверяем, что клиент был корректно создан как кешированный
        client.Should().BeOfType<CachedKodySuClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void CachedKodySuClient_IsRegisteredAsScoped_NotSingleton()
    {
        // Arrange
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KodySuOptions:ApiKey"] = "test-key",
                ["KodySuOptions:BaseUrl"] = "https://api.test.com"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddCachedKodySuClient(configuration);

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert - проверяем, что каждый scope получает свой экземпляр
        using IServiceScope scope1 = serviceProvider.CreateScope();
        using IServiceScope scope2 = serviceProvider.CreateScope();

        IKodySuClient client1 = scope1.ServiceProvider.GetRequiredService<IKodySuClient>();
        IKodySuClient client2 = scope2.ServiceProvider.GetRequiredService<IKodySuClient>();

        // Должны быть разные экземпляры (Scoped lifetime)
        client1.Should().NotBeSameAs(client2);

        // Но в пределах одного scope - тот же экземпляр
        IKodySuClient client1_again = scope1.ServiceProvider.GetRequiredService<IKodySuClient>();
        client1.Should().BeSameAs(client1_again);
    }
}
