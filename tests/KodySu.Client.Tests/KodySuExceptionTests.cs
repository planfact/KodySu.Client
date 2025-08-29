using FluentAssertions;
using Xunit;

namespace KodySu.Client.Tests;

public class KodySuExceptionTests
{
    [Fact]
    public void KodySuException_AllExceptionTypes_HaveCorrectInheritance()
    {
        // Arrange & Act & Assert: Проверяем наследование всех типов исключений
        var configException = new KodySuConfigurationException("Тест конфигурации");
        configException.Should().BeAssignableTo<KodySuException>();
        configException.Should().BeAssignableTo<Exception>();

        var httpException = new KodySuHttpException("HTTP ошибка", 500);
        httpException.Should().BeAssignableTo<KodySuException>();
        httpException.StatusCode.Should().Be(500);

        var authException = new KodySuAuthenticationException("Ошибка аутентификации");
        authException.Should().BeAssignableTo<KodySuException>();

        var validationException = new KodySuValidationException("ERROR_CODE", "Ошибка валидации");
        validationException.Should().BeAssignableTo<KodySuException>();
        validationException.ErrorCode.Should().Be("ERROR_CODE");
    }

    [Fact]
    public void KodySuException_WithInnerException_PreservesInnerException()
    {
        // Arrange: создаём исключение с внутренним исключением
        const string message = "Основная ошибка";
        var innerException = new ArgumentException("Внутренняя ошибка");

        // Act
        var exception = new KodySuAuthenticationException(message, innerException);

        // Assert: Проверяем, что внутреннее исключение сохранилось
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
