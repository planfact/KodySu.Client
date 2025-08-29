using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Reliable.HttpClient;

namespace KodySu.Client;

/// <summary>
/// HTTP клиент для KodySu API без кеширования
/// </summary>
public sealed class KodySuClient(
    HttpClient httpClient,
    IHttpResponseHandler<KodySuSearchResponse> responseHandler,
    IOptions<KodySuClientOptions> options,
    ILogger<KodySuClient> logger) : KodySuClientBase(options?.Value ?? throw new ArgumentNullException(nameof(options)), logger), IKodySuClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IHttpResponseHandler<KodySuSearchResponse> _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));

    /// <inheritdoc />
    public async ValueTask<KodySuResult?> SearchPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);

        Uri requestUri = BuildSearchUri(normalizedPhoneNumber);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            KodySuSearchResponse searchResponse = await _responseHandler.HandleAsync(response, cancellationToken);

            if (searchResponse.Numbers.Any(n => !n.Success))
            {
                foreach (KodySuResult? number in searchResponse.Numbers.Where(n => !n.Success))
                {
                    Logger.LogWarning("Ошибка при определении номера {PhoneNumber}: {ErrorCode} - {ErrorMessage}",
                        number.PhoneNumber, number.ErrorCode, number.ErrorMessage);
                }
            }

            LogRequestSuccess(searchResponse.Quota, searchResponse.Numbers.Length);

            KodySuResult? phoneDetails = ExtractPhoneResult(searchResponse, normalizedPhoneNumber);
            LogSingleSearchResult(normalizedPhoneNumber, phoneDetails);

            return phoneDetails;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Операция поиска номера телефона была отменена");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при поиске номера телефона: {PhoneNumber}", normalizedPhoneNumber);
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

        var normalizedPhones = phoneNumbers
            .Where(static p => !string.IsNullOrWhiteSpace(p))
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
