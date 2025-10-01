using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Reliable.HttpClient;
using Reliable.HttpClient.Caching.Extensions;
using Reliable.HttpClient.Caching.Generic;

namespace Planfact.KodySu.Client;

/// <summary>
/// Методы-расширения для регистрации клиентов KodySu в DI-контейнере
/// </summary>
public static class KodySuServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует обычный HTTP-клиент для работы с KodySu API
    /// </summary>
    /// <param name="services">Коллекция сервисов DI</param>
    /// <param name="config">Конфигурация приложения</param>
    /// <param name="sectionName">Имя секции с настройками клиента</param>
    /// <returns>Коллекция сервисов DI</returns>
    public static IServiceCollection AddKodySuClient(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName = KodySuClientOptions.SectionName)
    {
        IConfigurationSection section = config.GetSection(sectionName);
        services.Configure<KodySuClientOptions>(section);
        services.AddSingleton<IHttpResponseHandler<KodySuSearchResponse>, KodySuHttpResponseHandler>();

        services.AddHttpClient<IKodySuClient, KodySuClient>()
            .ConfigureHttpClient(ConfigureKodySuHttpClient)
            .AddResilience(); // Добавляем resilience для retry-политик и circuit breaker

        return services;
    }

    /// <summary>
    /// Регистрирует кэширующий HTTP-клиент для работы с KodySu API.
    /// Использует средне-срочное кеширование (10 минут TTL, 1000 записей) - оптимально для API поиска телефонов.
    /// </summary>
    /// <param name="services">Коллекция сервисов DI</param>
    /// <param name="config">Конфигурация приложения</param>
    /// <param name="sectionName">Имя секции с настройками клиента</param>
    /// <returns>Коллекция сервисов DI</returns>
    public static IServiceCollection AddCachedKodySuClient(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName = KodySuClientOptions.SectionName)
    {
        IConfigurationSection section = config.GetSection(sectionName);
        services.Configure<KodySuClientOptions>(section);
        services.AddSingleton<IHttpResponseHandler<KodySuSearchResponse>, KodySuHttpResponseHandler>();

        // Используем preset для среднесрочного кеширования (10 минут TTL, 1000 записей)
        // Оптимально для API поиска телефонов - баланс между актуальностью и производительностью
        services.AddHttpClient<CachedHttpClient<KodySuSearchResponse>>("KodySuCached")
            .ConfigureHttpClient(ConfigureKodySuHttpClient)
            .AddMediumTermCache<KodySuSearchResponse>();

        // Регистрируем как Scoped (не Singleton!) чтобы избежать проблем с captured dependencies
        services.AddScoped<IKodySuClient, CachedKodySuClient>();

        return services;
    }

    /// <summary>
    /// Конфигурирует HttpClient для KodySu API без captured dependencies.
    /// Поддерживает все настройки из HttpClientOptions: BaseUrl, Timeout, UserAgent.
    /// Примечание: Retry-политики из Reliable.HttpClient настраиваются автоматически через AddResilience().
    /// </summary>
    /// <param name="serviceProvider">Service provider для резолюции зависимостей</param>
    /// <param name="httpClient">HttpClient для конфигурации</param>
    private static void ConfigureKodySuHttpClient(IServiceProvider serviceProvider, HttpClient httpClient)
    {
        // Резолвим Options на каждый вызов - избегаем captured dependency
        KodySuClientOptions options = serviceProvider.GetRequiredService<IOptions<KodySuClientOptions>>().Value;

        // Настройки HTTP клиента
        httpClient.BaseAddress = new Uri(options.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        }

        // Примечание: Retry-политики (options.Retry.MaxRetries, options.Retry.BaseDelay)
        // настраиваются автоматически через Reliable.HttpClient при вызове .AddResilience()
        // или через встроенные presets (.AddMediumTermCache<>() уже включает resilience)
    }
}
