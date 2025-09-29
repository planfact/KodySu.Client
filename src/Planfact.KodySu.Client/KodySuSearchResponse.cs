
using System.Text.Json.Serialization;

namespace Planfact.KodySu.Client;

/// <summary>
/// Ответ от KodySu API при поиске телефонного номера
/// </summary>
public sealed record KodySuSearchResponse
{
    /// <summary>
    /// Оставшаяся квота запросов для текущего ключа
    /// </summary>
    [JsonPropertyName("quota")]
    public int Quota { get; init; }

    /// <summary>
    /// Массив результатов по номерам (даже если один)
    /// </summary>
    [JsonPropertyName("numbers")]
    public KodySuResult[] Numbers { get; init; } = [];

    /// <summary>
    /// Код ошибки, если запрос завершился неудачно
    /// </summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Сообщение об ошибке, если запрос завершился неудачно
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
}
