using Reliable.HttpClient;

namespace Planfact.KodySu.Client;

/// <summary>
/// Опции конфигурации для клиента KodySu API
/// </summary>
public sealed class KodySuClientOptions : HttpClientOptions
{
    /// <summary>
    /// Имя секции конфигурации для привязки через IConfiguration
    /// </summary>
    public const string SectionName = "KodySuOptions";

    /// <summary>
    /// Ключ аутентификации для доступа к KodySu API
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Время жизни кэшированных результатов (в минутах)
    /// Используется только в <see cref="CachedKodySuClient"/>
    /// </summary>
    public int CacheExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Создаёт экземпляр опций с дефолтными настройками для KodySu API
    /// </summary>
    public KodySuClientOptions()
    {
        // Настройки, специфичные для KodySu API
        BaseUrl = "https://www.kody.su";
        UserAgent = "Planfact-KodySu-Client/1.0";
        TimeoutSeconds = 30;
    }
}
