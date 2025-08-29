namespace KodySu.Client;

/// <summary>
/// Коды ошибок, возвращаемые KodySu API
/// </summary>
public static class KodySuErrorCodes
{
    /// <summary>
    /// Требуется аутентификация пользователя
    /// </summary>
    public const string AuthRequired = "AUTH_REQUIRED";

    /// <summary>
    /// Неверный ключ или ошибка аутентификации
    /// </summary>
    public const string AuthFailed = "AUTH_FAILED";

    /// <summary>
    /// Превышен лимит запросов для текущего ключа
    /// </summary>
    public const string LimitExceeded = "LIMIT_EXCEEDED";

    /// <summary>
    /// Переданы недопустимые параметры запроса
    /// </summary>
    public const string InvalidParams = "INVALID_PARAMS";
}
