using System.Net;

using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Reliable.HttpClient;
using Xunit;

namespace KodySu.Client.Tests;

public class KodySuClientTests : IDisposable
{
    private readonly KodySuClient _client;
    private readonly HttpClient _httpClient;
    private readonly KodySuClientOptions _options;
    private readonly Mock<ILogger<KodySuClient>> _loggerMock;
    private readonly Mock<IHttpResponseHandler<KodySuSearchResponse>> _responseHandlerMock;

    public KodySuClientTests()
    {
        _options = new KodySuClientOptions
        {
            BaseUrl = "https://www.kody.su/api/v2.1/",
            ApiKey = "test-api-key",
            TimeoutSeconds = 30
        };

        _loggerMock = new Mock<ILogger<KodySuClient>>();
        _responseHandlerMock = new Mock<IHttpResponseHandler<KodySuSearchResponse>>();

        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };

        var optionsWrapperMock = new Mock<IOptions<KodySuClientOptions>>();
        optionsWrapperMock.SetupGet(x => x.Value).Returns(_options);
        _client = new KodySuClient(_httpClient, _responseHandlerMock.Object, optionsWrapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SearchPhoneAsync_ValidPhoneNumber_ReturnsResultAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "+79161234567";
        var expectedResult = new KodySuResult
        {
            PhoneNumber = phoneNumber,
            Success = true,
            NumberType = 1,
            NumberTypeString = "ru_mobile",
            Operator = "МТС",
            Region = "Москва и область",
            Time = "+3"
        };

        var response = new KodySuSearchResponse
        {
            Numbers = [expectedResult]
        };

        // Setup response handler mock to return expected response
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act: Вызов тестируемого метода
        KodySuResult? result = await _client.SearchPhoneAsync(phoneNumber);

        // Assert: Проверка результата
        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be(phoneNumber);
        result.Success.Should().BeTrue();
        result.NumberType.Should().Be(1);
        result.Operator.Should().Be("МТС");
        result.IsRussianMobile.Should().BeTrue();

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhoneAsync_InvalidPhoneNumber_ReturnsNullAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "invalid";
        var response = new KodySuSearchResponse
        {
            Numbers = []
        };

        // Setup response handler mock to return response with empty numbers
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act: Вызов тестируемого метода
        KodySuResult? result = await _client.SearchPhoneAsync(phoneNumber);

        // Assert: Проверка результата
        result.Should().BeNull();

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhoneAsync_ApiError_ReturnsUnsuccessfulResultAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "+79161234567";

        // Настраиваем мок обработчика ответа для выбрасывания ожидаемого исключения
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new KodySuValidationException("RATE_LIMIT_EXCEEDED", "API rate limit exceeded"));

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _client.SearchPhoneAsync(phoneNumber).AsTask())
            .Should().ThrowAsync<KodySuValidationException>()
            .WithMessage("*API rate limit exceeded*");

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhonesAsync_MultiplePhones_ReturnsAllResultsAsync()
    {
        // Arrange: Подготовка входных данных и моков
        var phoneNumbers = new[] { "+79161234567", "+79265551234", "+79153334455" };
        KodySuResult[] expectedResults = [.. phoneNumbers.Select((phone, index) => new KodySuResult
        {
            PhoneNumber = phone,
            Success = true,
            NumberType = 1,
            Operator = index % 2 == 0 ? "МТС" : "Мегафон",
            Region = "Москва и область",
            Time = "+3"
        })];

        var response = new KodySuSearchResponse
        {
            Numbers = expectedResults
        };

        // Настраиваем мок обработчика ответа для возврата ожидаемого результата
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act: Вызов тестируемого метода
        IReadOnlyList<KodySuResult> results = await _client.SearchPhonesAsync(phoneNumbers);

        // Assert: Проверка результата
        results.Should().HaveCount(3);
        results.Select(r => r.PhoneNumber).Should().BeEquivalentTo(phoneNumbers);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

        // Проверяем, что обработчик ответа был вызван для каждого номера (3 раза)
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SearchPhonesAsync_EmptyCollection_ReturnsEmptyListAsync()
    {
        // Arrange: Подготовка входных данных
        var phoneNumbers = Array.Empty<string>();

        // Act: Вызов тестируемого метода
        IReadOnlyList<KodySuResult> results = await _client.SearchPhonesAsync(phoneNumbers);

        // Assert: Проверка результата
        results.Should().BeEmpty();

        // Проверяем, что обработчик ответа не был вызван для пустой коллекции
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchPhoneAsync_HttpError_ThrowsKodySuHttpExceptionAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "+79161234567";

        // Настраиваем мок обработчика ответа для выбрасывания ожидаемого исключения
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new KodySuHttpException("Внутренняя ошибка сервера", (int)HttpStatusCode.InternalServerError));

        // Act & Assert: Проверка выбрасывания исключения
        ExceptionAssertions<KodySuHttpException> exception = await FluentActions.Invoking(() => _client.SearchPhoneAsync(phoneNumber).AsTask())
            .Should().ThrowAsync<KodySuHttpException>();
        exception.Which.Message.Should().Contain("Внутренняя ошибка сервера");

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhoneAsync_Unauthorized_ThrowsKodySuAuthenticationExceptionAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "+79161234567";

        // Настраиваем мок обработчика ответа для выбрасывания ожидаемого исключения
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new KodySuAuthenticationException("Invalid API key"));

        // Act & Assert: Проверка выбрасывания исключения
        ExceptionAssertions<KodySuAuthenticationException> exception = await FluentActions.Invoking(() => _client.SearchPhoneAsync(phoneNumber).AsTask())
            .Should().ThrowAsync<KodySuAuthenticationException>();
        exception.Which.Message.Should().Contain("Invalid API key");

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhoneAsync_BadRequest_ThrowsKodySuHttpExceptionAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "invalid";

        // Настраиваем мок обработчика ответа для выбрасывания ожидаемого исключения
        _responseHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new KodySuHttpException("Bad Request", (int)HttpStatusCode.BadRequest));

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _client.SearchPhoneAsync(phoneNumber).AsTask())
            .Should().ThrowAsync<KodySuHttpException>()
            .WithMessage("*Bad Request*");

        // Проверяем, что обработчик ответа был вызван
        _responseHandlerMock.Verify(x => x.HandleAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPhonesAsync_NullCollection_ThrowsArgumentNullExceptionAsync()
    {
        // Arrange & Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _client.SearchPhonesAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("*phoneNumbers*");
    }

    [Fact]
    public void Constructor_NullParameters_ThrowsArgumentNullException()
    {
        // Arrange: Подготовка входных данных и моков
        var optionsWrapperMock = new Mock<IOptions<KodySuClientOptions>>();
        optionsWrapperMock.SetupGet(x => x.Value).Returns(_options);

        FluentActions.Invoking(() => new KodySuClient(null!, _responseHandlerMock.Object, optionsWrapperMock.Object, _loggerMock.Object))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("*httpClient*");

        FluentActions.Invoking(() => new KodySuClient(_httpClient, null!, optionsWrapperMock.Object, _loggerMock.Object))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("*responseHandler*");

        FluentActions.Invoking(() => new KodySuClient(_httpClient, _responseHandlerMock.Object, null!, _loggerMock.Object))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("*options*");

        FluentActions.Invoking(() => new KodySuClient(_httpClient, _responseHandlerMock.Object, optionsWrapperMock.Object, null!))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("*logger*");
    }

    [Fact]
    public async Task CancellationToken_PropagatesCorrectlyAsync()
    {
        // Arrange: Подготовка входных данных и моков
        const string phoneNumber = "+79161234567";
        var phoneNumbers = new[] { phoneNumber, "+79265551234" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert: Проверяем корректную передачу отменённого токена
        await FluentActions.Invoking(() => _client.SearchPhoneAsync(phoneNumber, cts.Token).AsTask())
            .Should().ThrowAsync<OperationCanceledException>();

        await FluentActions.Invoking(() => _client.SearchPhonesAsync(phoneNumbers, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }
}
