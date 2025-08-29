using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Reliable.HttpClient;

namespace KodySu.Client;


/// <summary>
/// Обработчик HTTP-ответов от KodySu API
/// Выполняет десериализацию, проверку ошибок и выбрасывает исключения при необходимости
/// </summary>
/// <param name="logger">Логгер для внутреннего логирования</param>
public sealed class KodySuHttpResponseHandler(ILogger<KodySuHttpResponseHandler> logger) : HttpResponseHandlerBase<KodySuSearchResponse>(logger)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Обрабатывает HTTP-ответ, выполняет десериализацию и выбрасывает исключения при ошибках
    /// </summary>
    /// <param name="response">HTTP-ответ от сервера</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Десериализованный объект ответа KodySuSearchResponse</returns>
    public override async Task<KodySuSearchResponse> HandleAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var responseContent = await ReadResponseContentAsync(response, cancellationToken);

        LogHttpResponse(response, responseContent, "KodySu");

        // Проверка HTTP-статуса
        if (!IsSuccessStatusCode(response))
        {
            HandleHttpError(response, responseContent);
        }

        // Проверка на пустой ответ
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new KodySuHttpException("Получен пустой ответ от API", (int)response.StatusCode);
        }

        // Десериализация ответа
        KodySuSearchResponse searchResponse;
        try
        {
            searchResponse = JsonSerializer.Deserialize<KodySuSearchResponse>(responseContent, _jsonOptions) ??
                           throw new KodySuHttpException("Получен null ответ от API", (int)response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new KodySuHttpException("Ошибка при обработке ответа от API", ex, (int)response.StatusCode);
        }

        // Проверка успешности на уровне API
        if (!string.IsNullOrWhiteSpace(searchResponse.ErrorCode) || !string.IsNullOrWhiteSpace(searchResponse.ErrorMessage))
        {
            HandleApiError(searchResponse);
        }

        return searchResponse;
    }

    /// <summary>
    /// Обрабатывает HTTP-ошибки (статус-коды)
    /// </summary>
    /// <param name="response">HTTP-ответ</param>
    /// <param name="responseContent">Содержимое ответа</param>
    private static void HandleHttpError(HttpResponseMessage response, string responseContent)
    {
        var statusCode = (int)response.StatusCode;
        var errorMessage = GetStatusCodeDescription(response.StatusCode);

        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            errorMessage += $". Содержимое ответа: {responseContent}";
        }

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new KodySuAuthenticationException(
                "Ошибка аутентификации. Проверьте API ключ."),
            HttpStatusCode.Forbidden => new KodySuAuthenticationException(
                "Доступ запрещен. Проверьте права доступа API ключа."),
            // TooManyRequests обрабатывается Polly, если дошли сюда - все попытки исчерпаны
            _ => new KodySuHttpException(errorMessage, statusCode)
        };
    }

    /// <summary>
    /// Обрабатывает ошибки, возвращённые API (в JSON-ответе)
    /// </summary>
    /// <param name="response">Десериализованный ответ API</param>
    private static void HandleApiError(KodySuSearchResponse response)
    {
        var errorCode = response.ErrorCode ?? "UNKNOWN_ERROR";
        var errorMessage = response.ErrorMessage ?? "Неизвестная ошибка";

        throw errorCode switch
        {
            KodySuErrorCodes.AuthRequired or KodySuErrorCodes.AuthFailed => new KodySuAuthenticationException($"Ошибка аутентификации: {errorMessage}"),
            KodySuErrorCodes.LimitExceeded => new KodySuValidationException(errorCode, $"Превышен лимит запросов: {errorMessage}"),
            _ => new KodySuValidationException(errorCode, errorMessage)
        };
    }
}
