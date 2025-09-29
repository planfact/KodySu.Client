using System.Net;
using System.Text;
using System.Text.Json;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace Planfact.KodySu.Client.Tests;

public class KodySuHttpResponseHandlerTests
{
    private readonly KodySuHttpResponseHandler _handler;
    private readonly Mock<ILogger<KodySuHttpResponseHandler>> _loggerMock;

    public KodySuHttpResponseHandlerTests()
    {
        _loggerMock = new Mock<ILogger<KodySuHttpResponseHandler>>();
        _handler = new KodySuHttpResponseHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulResponse_ReturnsKodySuSearchResponse()
    {
        // Arrange: Подготовка успешного ответа
        var expectedResponse = new KodySuSearchResponse
        {
            Quota = 100,
            Numbers =
            [
                new KodySuResult
                {
                    PhoneNumber = "+79161234567",
                    Success = true,
                    NumberType = 1,
                    Operator = "МТС"
                }
            ],
            ErrorCode = null,
            ErrorMessage = null
        };

        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/json")
        };

        // Act: Вызов тестируемого метода
        KodySuSearchResponse result = await _handler.HandleAsync(httpResponse);

        // Assert: Проверка результата
        result.Should().NotBeNull();
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.Quota.Should().Be(100);
        result.Numbers.Should().HaveCount(1);
        result.Numbers[0].PhoneNumber.Should().Be("+79161234567");
    }

    [Fact]
    public async Task HandleAsync_ApiErrorResponse_ThrowsKodySuValidationException()
    {
        // Arrange: Подготовка ответа с ошибкой API
        var errorResponse = new KodySuSearchResponse
        {
            Quota = 100,
            Numbers = [],
            ErrorCode = "INVALID_PHONE",
            ErrorMessage = "Invalid phone number format"
        };

        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuValidationException>()
            .WithMessage("*Invalid phone number format*");
    }

    [Fact]
    public async Task HandleAsync_AuthErrorResponse_ThrowsKodySuValidationException()
    {
        // Arrange: Подготовка ответа с ошибкой авторизации
        var errorResponse = new KodySuSearchResponse
        {
            Quota = 100,
            Numbers = [],
            ErrorCode = "AUTH_REQUIRED",
            ErrorMessage = "API key is required"
        };

        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuAuthenticationException>()
            .WithMessage("*API key is required*");
    }

    [Fact]
    public async Task HandleAsync_UnauthorizedHttpStatus_ThrowsKodySuAuthenticationException()
    {
        // Arrange: Подготовка ответа с кодом 401
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuAuthenticationException>()
            .WithMessage("*Проверьте API ключ*");
    }

    [Fact]
    public async Task HandleAsync_ForbiddenHttpStatus_ThrowsKodySuAuthenticationException()
    {
        // Arrange: Подготовка ответа с кодом 403
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("Forbidden", Encoding.UTF8, "text/plain")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuAuthenticationException>()
            .WithMessage("*права доступа*");
    }

    [Fact]
    public async Task HandleAsync_BadRequestHttpStatus_ThrowsKodySuHttpException()
    {
        // Arrange: Подготовка ответа с кодом 400
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuHttpException>()
            .WithMessage("*Bad Request*");
    }

    [Fact]
    public async Task HandleAsync_EmptyResponse_ThrowsKodySuHttpException()
    {
        // Arrange: Подготовка пустого ответа
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuHttpException>()
            .WithMessage("*пустой ответ*");
    }

    [Fact]
    public async Task HandleAsync_InvalidJson_ThrowsKodySuHttpException()
    {
        // Arrange: Подготовка ответа с некорректным JSON
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuHttpException>()
            .WithMessage("*обработке ответа*");
    }

    [Fact]
    public async Task HandleAsync_NullResponse_ThrowsKodySuHttpException()
    {
        // Arrange: Подготовка ответа с null
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        // Act & Assert: Проверка выбрасывания исключения
        await FluentActions.Invoking(() => _handler.HandleAsync(httpResponse))
            .Should().ThrowAsync<KodySuHttpException>()
            .WithMessage("*null ответ*");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert: Проверка выбрасывания исключения при null logger
        FluentActions.Invoking(() => new KodySuHttpResponseHandler(null!))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("*logger*");
    }
}
