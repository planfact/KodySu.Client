using FluentAssertions;

namespace KodySu.Client.Tests;

public class KodySuResultTests
{
    [Fact]
    public void GetNumberType_RussianMobile_ReturnsCorrectType()
    {
        // Arrange: подготовка данных для тестирования российского мобильного номера
        var result = new KodySuResult
        {
            NumberTypeString = "ru_mobile",
            NumberType = 1
        };

        // Act & Assert
        result.GetNumberType().Should().Be(KodySuPhoneType.RussianMobile);
        result.IsRussianMobile.Should().BeTrue();
        result.IsRussianFixed.Should().BeFalse();
    }

    [Fact]
    public void GetNumberType_RussianFixed_ReturnsCorrectType()
    {
        // Arrange: подготовка данных для тестирования российского стационарного номера
        var result = new KodySuResult
        {
            NumberTypeString = "ru_fixed",
            NumberType = 2
        };

        // Act & Assert
        result.GetNumberType().Should().Be(KodySuPhoneType.RussianFixed);
        result.IsRussianMobile.Should().BeFalse();
        result.IsRussianFixed.Should().BeTrue();
    }

    [Fact]
    public void GetNumberType_Other_ReturnsCorrectType()
    {
        // Arrange: подготовка данных для тестирования других типов номеров
        var result = new KodySuResult
        {
            NumberTypeString = "other",
            NumberType = 3
        };

        // Act & Assert
        result.GetNumberType().Should().Be(KodySuPhoneType.Other);
        result.IsRussianMobile.Should().BeFalse();
        result.IsRussianFixed.Should().BeFalse();
    }

    [Fact]
    public void GetNumberType_UnknownString_ReturnsUnknown()
    {
        // Arrange: подготовка данных для тестирования неизвестного типа номера
        var result = new KodySuResult
        {
            NumberTypeString = "Неизвестный тип"
        };

        // Act & Assert
        result.GetNumberType().Should().Be(KodySuPhoneType.Unknown);
    }

    [Fact]
    public void GetDisplayOperator_BothOperatorAndOperatorFull_ReturnsOperatorFull()
    {
        // Arrange: тестируем случай когда заполнены оба поля оператора
        var result = new KodySuResult
        {
            Operator = "МТС",
            OperatorFull = "ПАО \"Мобильные ТелеСистемы\""
        };

        // Act & Assert
        result.GetDisplayOperator().Should().Be("ПАО \"Мобильные ТелеСистемы\"");
    }

    [Fact]
    public void GetDisplayOperator_OnlyOperator_ReturnsOperator()
    {
        // Arrange: тестируем случай когда заполнено только поле оператора
        var result = new KodySuResult
        {
            Operator = "МТС",
            OperatorFull = null
        };

        // Act & Assert
        result.GetDisplayOperator().Should().Be("МТС");
    }

    [Fact]
    public void GetDisplayOperator_EmptyOperatorFull_ReturnsOperator()
    {
        // Arrange: тестируем случай когда полное название оператора пустое
        var result = new KodySuResult
        {
            Operator = "МТС",
            OperatorFull = ""
        };

        // Act & Assert
        result.GetDisplayOperator().Should().Be("МТС");
    }

    [Fact]
    public void GetDisplayOperator_NoOperators_ReturnsEmptyString()
    {
        // Arrange: тестируем случай когда операторы не указаны
        var result = new KodySuResult
        {
            Operator = null,
            OperatorFull = null
        };

        // Act & Assert
        result.GetDisplayOperator().Should().Be(string.Empty);
    }

    [Fact]
    public void IsPortedNumber_BdpnTrue_ReturnsTrue()
    {
        // Arrange: тестируем портированный номер (БДПН = true)
        var result = new KodySuResult
        {
            IsBdpn = true,
            BdpnOperator = "Билайн"
        };

        // Act & Assert
        result.IsPortedNumber.Should().BeTrue();
    }

    [Fact]
    public void IsPortedNumber_BdpnFalse_ReturnsFalse()
    {
        // Arrange: тестируем не портированный номер
        var result = new KodySuResult
        {
            IsBdpn = false
        };

        // Act & Assert
        result.IsPortedNumber.Should().BeFalse();
    }

    [Fact]
    public void IsPortedNumber_BdpnNull_ReturnsFalse()
    {
        // Arrange: тестируем случай когда БДПН не определено
        var result = new KodySuResult
        {
            IsBdpn = null
        };

        // Act & Assert
        result.IsPortedNumber.Should().BeFalse();
    }
}
