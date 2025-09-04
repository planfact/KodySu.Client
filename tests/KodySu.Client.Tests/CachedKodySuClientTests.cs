using System.Net;
using System.Text;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

using Reliable.HttpClient;
using Reliable.HttpClient.Caching;

namespace KodySu.Client.Tests;

public class CachedKodySuClientTests
{
    private readonly CachedKodySuClient _cachedClient;
    private readonly Mock<IHttpResponseHandler<KodySuSearchResponse>> _responseHandlerMock;
    private readonly Mock<ILogger<CachedKodySuClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public CachedKodySuClientTests()
    {
        _loggerMock = new Mock<ILogger<CachedKodySuClient>>();
        _responseHandlerMock = new Mock<IHttpResponseHandler<KodySuSearchResponse>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        // Создаем настройки для KodySu
        IOptions<KodySuClientOptions> kodySuOptions = Options.Create(new KodySuClientOptions
        {
            BaseUrl = "https://test.example.com",
            ApiKey = "test-api-key"
        });

        // Настраиваем mock HttpMessageHandler для возврата успешного ответа
        var responseContent = """{"numbers":[],"quota":100}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Создаем HttpClient с mock handler
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        var cacheMock = new Mock<Reliable.HttpClient.Caching.Abstractions.IHttpResponseCache<KodySuSearchResponse>>();
        var cacheOptionsMock = new Mock<IOptionsSnapshot<Reliable.HttpClient.Caching.Abstractions.HttpCacheOptions>>();
        var cachedLoggerMock = new Mock<ILogger<CachedHttpClient<KodySuSearchResponse>>>();

        var httpCacheOptions = new Reliable.HttpClient.Caching.Abstractions.HttpCacheOptions();
        cacheOptionsMock.SetupGet(x => x.Value).Returns(httpCacheOptions);

        var cachedHttpClient = new CachedHttpClient<KodySuSearchResponse>(httpClient, cacheMock.Object, cacheOptionsMock.Object, cachedLoggerMock.Object);
        _cachedClient = new CachedKodySuClient(cachedHttpClient, _responseHandlerMock.Object, kodySuOptions, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchPhoneAsync_NullOrEmptyPhoneNumber_ReturnsNull()
    {
        // Act & Assert
        KodySuResult? nullResult = await _cachedClient.SearchPhoneAsync(null!);
        KodySuResult? emptyResult = await _cachedClient.SearchPhoneAsync("");
        KodySuResult? whitespaceResult = await _cachedClient.SearchPhoneAsync("   ");

        nullResult.Should().BeNull();
        emptyResult.Should().BeNull();
        whitespaceResult.Should().BeNull();
    }

    [Fact]
    public async Task SearchPhonesAsync_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var phoneNumbers = Array.Empty<string>();

        // Act
        IReadOnlyList<KodySuResult> results = await _cachedClient.SearchPhonesAsync(phoneNumbers);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPhonesAsync_NullCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _cachedClient.SearchPhonesAsync(null!));
    }

    [Fact]
    public async Task SearchPhonesAsync_DuplicateNumbers_ProcessesOnlyUnique()
    {
        // Arrange - массив с дубликатами
        var phoneNumbers = new[] { "79991234567", "79991234567", "79992345678" };

        // Настраиваем responseHandler для возврата результатов
        var mockResponse = new KodySuSearchResponse
        {
            Numbers =
            [
                new KodySuResult { PhoneNumber = "79991234567", Success = true, Operator = "МТС" },
                new KodySuResult { PhoneNumber = "79992345678", Success = true, Operator = "Билайн" }
            ],
            Quota = 100
        };

        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        IReadOnlyList<KodySuResult> results = await _cachedClient.SearchPhonesAsync(phoneNumbers);

        // Assert
        results.Should().NotBeNull();
        results.Count.Should().Be(2); // Дубликаты должны быть исключены (3 -> 2 уникальных)

        // Проверяем, что HTTP запрос был сделан правильно (должно быть минимум 1 вызов)
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }
}
