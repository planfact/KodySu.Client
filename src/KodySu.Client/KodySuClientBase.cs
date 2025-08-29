using Microsoft.Extensions.Logging;

namespace KodySu.Client;

/// <summary>
/// Абстрактный базовый класс для клиентов KodySu, предоставляющий общие методы и логику.
/// </summary>
/// <remarks>
/// Конструктор базового класса клиента.
/// </remarks>
/// <param name="options">Настройки клиента</param>
/// <param name="logger">Логгер</param>
public abstract class KodySuClientBase(KodySuClientOptions options, ILogger logger)
{
    /// <summary>
    /// Настройки клиента (ключ, базовый URL и пр.)
    /// </summary>
    protected readonly KodySuClientOptions Options = options ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// Логгер для внутреннего логирования событий и ошибок.
    /// </summary>
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Формирует URI для поиска информации по номеру телефона.
    /// </summary>
    /// <param name="phoneNumber">Нормализованный номер телефона</param>
    /// <returns>URI для запроса к API</returns>
    protected Uri BuildSearchUri(string phoneNumber)
    {
        var baseUri = new Uri(Options.BaseUrl);
        var uriBuilder = new UriBuilder(baseUri)
        {
            Path = "/api/v2.1/search.json",
            Query = $"q={Uri.EscapeDataString(phoneNumber)}&key={Uri.EscapeDataString(Options.ApiKey)}"
        };
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Нормализует номер телефона (убирает лишние символы, приводит к стандартному виду).
    /// </summary>
    /// <param name="phoneNumber">Исходный номер телефона</param>
    /// <returns>Нормализованный номер телефона</returns>
    protected static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;
        return ((PhoneNumber)phoneNumber).Value;
    }

    /// <summary>
    /// Нормализует и удаляет дубликаты из списка номеров телефонов.
    /// </summary>
    /// <param name="phoneNumbers">Коллекция исходных номеров</param>
    /// <returns>Список уникальных нормализованных номеров</returns>
    protected static IReadOnlyList<string> NormalizePhoneNumbers(IEnumerable<string> phoneNumbers)
    {
        return [.. phoneNumbers
            .Select(NormalizePhoneNumber)
            .Where(static phone => !string.IsNullOrWhiteSpace(phone))
            .Distinct()];
    }

    /// <summary>
    /// Извлекает результат для конкретного номера из ответа API.
    /// </summary>
    /// <param name="response">Ответ API</param>
    /// <param name="normalizedPhoneNumber">Нормализованный номер</param>
    /// <returns>Результат поиска или null</returns>
    protected static KodySuResult? ExtractPhoneResult(KodySuSearchResponse response, string normalizedPhoneNumber)
        => response.Numbers.FirstOrDefault(n => n.Success && ((PhoneNumber)n.PhoneNumber).Value == normalizedPhoneNumber);

    /// <summary>
    /// Логирует результат поиска одного номера.
    /// </summary>
    /// <param name="normalizedPhoneNumber">Нормализованный номер</param>
    /// <param name="phoneDetails">Результат поиска</param>
    private protected void LogSingleSearchResult(string normalizedPhoneNumber, KodySuResult? phoneDetails)
    {
        if (phoneDetails is not null)
        {
            Logger.LogDebug("Найдена информация о номере {Phone}: {Operator}", normalizedPhoneNumber, phoneDetails.Operator);
        }
        else
        {
            Logger.LogDebug("Информация о номере {Phone} не найдена", normalizedPhoneNumber);
        }
    }

    /// <summary>
    /// Логирует начало пакетного поиска.
    /// </summary>
    /// <param name="normalizedCount">Количество уникальных номеров</param>
    /// <param name="inputCount">Количество переданных номеров</param>
    private protected void LogBatchSearchStart(int normalizedCount, int inputCount)
        => Logger.LogDebug("Начинаем поиск {TotalCount} уникальных номеров из {InputCount} переданных", normalizedCount, inputCount);

    /// <summary>
    /// Логирует завершение пакетного поиска.
    /// </summary>
    /// <param name="foundCount">Найдено номеров</param>
    /// <param name="totalCount">Всего уникальных номеров</param>
    private protected void LogBatchSearchResult(int foundCount, int totalCount)
        => Logger.LogDebug("Завершен поиск номеров: найдено {Found} из {Total} уникальных номеров", foundCount, totalCount);

    /// <summary>
    /// Логирует успешный ответ от API.
    /// </summary>
    /// <param name="quota">Оставшаяся квота</param>
    /// <param name="numbersCount">Количество найденных номеров</param>
    private protected void LogRequestSuccess(int quota, int numbersCount)
        => Logger.LogDebug("Запрос успешен. Квота: {Quota}, найдено номеров: {Count}", quota, numbersCount);
}
