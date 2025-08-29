using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Reliable.HttpClient;
using Reliable.HttpClient.Caching;
using Reliable.HttpClient.Caching.Extensions;

namespace KodySu.Client;

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
    public static IServiceCollection AddKodySuClient(this IServiceCollection services, IConfiguration config, string sectionName = KodySuClientOptions.SectionName)
    {
        IConfigurationSection section = config.GetSection(sectionName);
        services.Configure<KodySuClientOptions>(section);
        services.AddSingleton<IHttpResponseHandler<KodySuSearchResponse>, KodySuHttpResponseHandler>();
        services.AddHttpClient<IKodySuClient, KodySuClient>((sp, http) =>
        {
            KodySuClientOptions opts = sp.GetRequiredService<IOptions<KodySuClientOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(opts.UserAgent))
                http.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
        });
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
    public static IServiceCollection AddCachedKodySuClient(this IServiceCollection services, IConfiguration config, string sectionName = KodySuClientOptions.SectionName)
    {
        IConfigurationSection section = config.GetSection(sectionName);
        services.Configure<KodySuClientOptions>(section);
        services.AddSingleton<IHttpResponseHandler<KodySuSearchResponse>, KodySuHttpResponseHandler>();

        // Используем preset для среднесрочного кеширования (10 минут TTL, 1000 записей)
        // Оптимально для API поиска телефонов - баланс между актуальностью и производительностью
        services.AddHttpClient<CachedHttpClient<KodySuSearchResponse>>("KodySuCached", (sp, http) =>
        {
            KodySuClientOptions opts = sp.GetRequiredService<IOptions<KodySuClientOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(opts.UserAgent))
                http.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
        })
        .AddMediumTermCache<KodySuSearchResponse>();

        services.AddSingleton<IKodySuClient, CachedKodySuClient>();
        return services;
    }
}
