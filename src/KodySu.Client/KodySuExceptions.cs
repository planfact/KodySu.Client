namespace KodySu.Client;

/// <summary>
/// Базовый тип исключения для ошибок, связанных с KodySu API
/// </summary>
public abstract class KodySuException : Exception
{
    /// <summary>
    /// Создаёт исключение с сообщением об ошибке
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    protected KodySuException(string message) : base(message) { }

    /// <summary>
    /// Создаёт исключение с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    protected KodySuException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Исключение, возникающее при ошибках конфигурации клиента KodySu API
/// </summary>
/// <param name="message">Сообщение об ошибке</param>
public sealed class KodySuConfigurationException(string message) : KodySuException(message)
{
}

/// <summary>
/// Исключение, возникающее при ошибках HTTP-запросов к KodySu API
/// </summary>
public sealed class KodySuHttpException : KodySuException
{
    /// <summary>
    /// HTTP-статус код ответа (если доступен)
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Создаёт исключение HTTP с сообщением и статус-кодом
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="statusCode">HTTP-статус код</param>
    public KodySuHttpException(string message, int? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Создаёт исключение HTTP с сообщением, внутренним исключением и статус-кодом
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    /// <param name="statusCode">HTTP-статус код</param>
    public KodySuHttpException(string message, Exception innerException, int? statusCode = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Исключение, возникающее при ошибках аутентификации в KodySu API
/// </summary>
public sealed class KodySuAuthenticationException : KodySuException
{
    /// <summary>
    /// Создаёт исключение аутентификации с сообщением
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public KodySuAuthenticationException(string message) : base(message) { }

    /// <summary>
    /// Создаёт исключение аутентификации с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public KodySuAuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Исключение, возникающее при ошибках валидации запроса к KodySu API
/// </summary>
/// <param name="errorCode">Код ошибки</param>
/// <param name="message">Сообщение об ошибке</param>
public sealed class KodySuValidationException(string errorCode, string message) : KodySuException(message)
{
    /// <summary>
    /// Код ошибки, возвращённый API
    /// </summary>
    public string ErrorCode { get; } = errorCode;
}
