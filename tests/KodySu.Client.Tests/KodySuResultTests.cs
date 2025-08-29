using Xunit;
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

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        // Arrange: тестируем равенство record объектов
        var result1 = new KodySuResult
        {
            PhoneNumber = "79991234567",
            Success = true,
            Operator = "МТС"
        };

        var result2 = new KodySuResult
        {
            PhoneNumber = "79991234567",
            Success = true,
            Operator = "МТС"
        };

        var result3 = new KodySuResult
        {
            PhoneNumber = "79991234567",
            Success = true,
            Operator = "Билайн"
        };

        // Act & Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
        result1.GetHashCode().Should().Be(result2.GetHashCode());
        result1.GetHashCode().Should().NotBe(result3.GetHashCode());
    }

    [Fact]
    public void AllProperties_DefaultValues_AreCorrect()
    {
        // Arrange & Act: проверяем значения по умолчанию для всех свойств
        var result = new KodySuResult();

        // Assert
        result.PhoneNumber.Should().Be(string.Empty);
        result.Success.Should().BeFalse();
        result.NumberType.Should().BeNull();
        result.NumberTypeString.Should().BeNull();
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.DefCode.Should().BeNull();
        result.Number.Should().BeNull();
        result.CodeStart.Should().BeNull();
        result.CodeEnd.Should().BeNull();
        result.Operator.Should().BeNull();
        result.OperatorFull.Should().BeNull();
        result.Region.Should().BeNull();
        result.Time.Should().BeNull();
        result.IsBdpn.Should().BeNull();
        result.BdpnOperator.Should().BeNull();
        result.CityCode.Should().BeNull();
        result.City.Should().BeNull();
        result.CountryCode.Should().BeNull();
        result.InternationalCityCode.Should().BeNull();
        result.Country.Should().BeNull();
    }
}
