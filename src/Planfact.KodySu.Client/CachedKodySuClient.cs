using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Reliable.HttpClient;
using Reliable.HttpClient.Caching.Generic;

namespace Planfact.KodySu.Client;

/// <summary>
/// Кэшированная реализация KodySu клиента с использованием Reliable.HttpClient.Caching
/// </summary>
public sealed class CachedKodySuClient(
    CachedHttpClient<KodySuSearchResponse> cachedClient,
    IHttpResponseHandler<KodySuSearchResponse> responseHandler,
    IOptions<KodySuClientOptions> options,
    ILogger<CachedKodySuClient> logger) : KodySuClientBase(options?.Value ?? throw new ArgumentNullException(nameof(options)), logger), IKodySuClient
{
    private readonly CachedHttpClient<KodySuSearchResponse> _cachedClient = cachedClient ?? throw new ArgumentNullException(nameof(cachedClient));
    private readonly IHttpResponseHandler<KodySuSearchResponse> _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));

    /// <inheritdoc />
    public async ValueTask<KodySuResult?> SearchPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);

        try
        {
            // Создаем HTTP запрос и используем кеширующий клиент (ответы кешируются автоматически)
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildSearchUri(normalizedPhoneNumber));
            KodySuSearchResponse searchResponse = await _cachedClient.SendAsync(
                request,
                response => _responseHandler.HandleAsync(response, cancellationToken),
                cancellationToken);

            LogRequestSuccess(searchResponse.Quota, searchResponse.Numbers.Length);

            // Извлекаем результат для конкретного номера
            KodySuResult? phoneDetails = ExtractPhoneResult(searchResponse, normalizedPhoneNumber);

            LogSingleSearchResult(normalizedPhoneNumber, phoneDetails);

            return phoneDetails;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при поиске телефона {Phone}", normalizedPhoneNumber);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<KodySuResult>> SearchPhonesAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        if (phoneNumbers is null)
        {
            return Task.FromException<IReadOnlyList<KodySuResult>>(new ArgumentNullException(nameof(phoneNumbers)));
        }

        return SearchPhonesAsyncCore(phoneNumbers, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<KodySuResult>> SearchPhonesAsync(
        CancellationToken cancellationToken = default,
        params string[] phoneNumbers)
        => SearchPhonesAsync((IReadOnlyCollection<string>)phoneNumbers, cancellationToken);

    private async Task<IReadOnlyList<KodySuResult>> SearchPhonesAsyncCore(
        IEnumerable<string> phoneNumbers,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Нормализуем, убираем пустые и дубли
        var normalizedPhones = phoneNumbers
            .Select(NormalizePhoneNumber)
            .Where(static p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToArray();

        if (normalizedPhones.Length == 0)
        {
            return [];
        }

        LogBatchSearchStart(normalizedPhones.Length, normalizedPhones.Length);

        var tasks = new Task<KodySuResult?>[normalizedPhones.Length];
        for (var i = 0; i < normalizedPhones.Length; i++)
        {
            tasks[i] = SearchPhoneAsync(normalizedPhones[i], cancellationToken).AsTask();
        }

        KodySuResult?[] searchResults = await Task.WhenAll(tasks);
        var results = new List<KodySuResult>(searchResults.Length);
        foreach (KodySuResult? r in searchResults)
        {
            if (r is not null)
                results.Add(r);
        }

        LogBatchSearchResult(results.Count, normalizedPhones.Length);

        return results.AsReadOnly();
    }
}
