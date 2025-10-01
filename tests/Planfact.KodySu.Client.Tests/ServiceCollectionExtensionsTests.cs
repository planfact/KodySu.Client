using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Reliable.HttpClient;
using Reliable.HttpClient.Caching.Abstractions;
using Reliable.HttpClient.Caching.Generic;

namespace Planfact.KodySu.Client.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["KodySuOptions:ApiKey"] = "test-api-key",
            ["KodySuOptions:BaseUrl"] = "https://api.test.com",
            ["KodySuOptions:TimeoutSeconds"] = "30",
            ["KodySuOptions:Retry:MaxRetries"] = "3",
            ["KodySuOptions:Retry:BaseDelay"] = "00:00:01"
        });
        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void ConfigurationBinding_AppliesSettingsCorrectly()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        IOptions<KodySuClientOptions> options = serviceProvider.GetRequiredService<IOptions<KodySuClientOptions>>();
        options.Value.ApiKey.Should().Be("test-api-key");
        options.Value.BaseUrl.Should().Be("https://api.test.com");
        options.Value.TimeoutSeconds.Should().Be(30);
        options.Value.Retry.MaxRetries.Should().Be(3);
        options.Value.Retry.BaseDelay.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithoutCaching_ResolvesToBaseClient()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IKodySuClient>().Should().BeOfType<KodySuClient>();
    }

    [Fact]
    public void HttpResponseHandler_IsRegistered()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        _services.AddCachedKodySuClient(_configuration);
        serviceProvider.GetRequiredService<IHttpResponseHandler<KodySuSearchResponse>>()
            .Should().BeOfType<KodySuHttpResponseHandler>();
    }

    [Fact]
    public void WithCaching_ResolvesToCachedClient()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        _services.AddCachedKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        _services.AddCachedKodySuClient(_configuration);
        // Assert
        serviceProvider.GetRequiredService<IKodySuClient>().Should().BeOfType<CachedKodySuClient>();
    }

    [Fact]
    public void WithCaching_RegistersCachedHttpClient()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        _services.AddCachedKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        serviceProvider.GetRequiredService<CachedHttpClient<KodySuSearchResponse>>().Should().NotBeNull();
    }

    [Fact]
    public void WithCachingCustomOptions_AppliesCorrectly()
    {
        // Act
        _services.AddKodySuClient(_configuration);
        _services.AddCachedKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IKodySuClient>().Should().BeOfType<CachedKodySuClient>();
        serviceProvider.GetRequiredService<CachedHttpClient<KodySuSearchResponse>>().Should().NotBeNull();

        // Проверяем, что HttpCacheOptions правильно настроены для MediumTerm preset
        // Используем IOptionsSnapshot для получения настроек именованного клиента "KodySuCached"
        IOptionsSnapshot<HttpCacheOptions> optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpCacheOptions>>();
        HttpCacheOptions cacheOptions = optionsSnapshot.Get("KodySuCached");

        // Проверяем точные значения MediumTerm preset (10 минут TTL, 1000 записей)
        cacheOptions.DefaultExpiry.Should().Be(TimeSpan.FromMinutes(10));
        cacheOptions.MaxCacheSize.Should().Be(1_000);
    }

    [Fact]
    public void ScopedLifetime_ReturnsSameInstanceInScope()
    {
        // Arrange
        _services.AddCachedKodySuClient(_configuration);
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Act
        using IServiceScope scope = serviceProvider.CreateScope();
        IKodySuClient client1 = scope.ServiceProvider.GetRequiredService<IKodySuClient>();
        IKodySuClient client2 = scope.ServiceProvider.GetRequiredService<IKodySuClient>();

        // Assert
        client1.Should().BeSameAs(client2); // Scoped lifetime
    }
}
