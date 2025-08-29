using Xunit;
using FluentAssertions;

namespace KodySu.Client.Tests;

public class KodySuPhoneTypeTests
{
    [Fact]
    public void FromString_RussianMobile_ReturnsCorrectType()
    {
        // Arrange & Act & Assert: Проверяем корректное определение российского мобильного номера
        KodySuPhoneTypeExtensions.FromString("ru_mobile").Should().Be(KodySuPhoneType.RussianMobile);
    }

    [Fact]
    public void FromString_RussianFixed_ReturnsCorrectType()
    {
        // Arrange & Act & Assert: Проверяем корректное определение российского стационарного номера
        KodySuPhoneTypeExtensions.FromString("ru_fixed").Should().Be(KodySuPhoneType.RussianFixed);
    }

    [Fact]
    public void FromString_Other_ReturnsCorrectType()
    {
        // Arrange & Act & Assert: Проверяем корректное определение других типов номеров
        KodySuPhoneTypeExtensions.FromString("other").Should().Be(KodySuPhoneType.Other);
        KodySuPhoneTypeExtensions.FromString("ua_mobile").Should().Be(KodySuPhoneType.Other);
    }

    [Fact]
    public void FromString_UnknownValues_ReturnsUnknown()
    {
        // Arrange & Act & Assert: Проверяем обработку неизвестных значений
        KodySuPhoneTypeExtensions.FromString("unknown").Should().Be(KodySuPhoneType.Unknown);
        KodySuPhoneTypeExtensions.FromString("international").Should().Be(KodySuPhoneType.Unknown);
        KodySuPhoneTypeExtensions.FromString("").Should().Be(KodySuPhoneType.Unknown);
        KodySuPhoneTypeExtensions.FromString("   ").Should().Be(KodySuPhoneType.Unknown);
    }

    [Fact]
    public void FromString_NullValue_ReturnsUnknown()
    {
        // Arrange & Act & Assert: Проверяем обработку null значения
        KodySuPhoneTypeExtensions.FromString(null).Should().Be(KodySuPhoneType.Unknown);
    }

    [Fact]
    public void GetDescription_AllTypes_ReturnCorrectDescriptions()
    {
        // Arrange & Act & Assert: Проверяем корректность описаний для всех типов
        KodySuPhoneType.RussianMobile.GetDescription().Should().Be("Мобильный телефон России");
        KodySuPhoneType.RussianFixed.GetDescription().Should().Be("Стационарный телефон России");
        KodySuPhoneType.Other.GetDescription().Should().Be("Международный номер");
        KodySuPhoneType.Unknown.GetDescription().Should().Be("Неизвестный тип");
    }

    [Fact]
    public void EnumValues_HaveCorrectIntegerValues()
    {
        // Arrange & Act & Assert: Проверяем корректность числовых значений перечисления
        ((int)KodySuPhoneType.RussianMobile).Should().Be(1);
        ((int)KodySuPhoneType.RussianFixed).Should().Be(2);
        ((int)KodySuPhoneType.Other).Should().Be(3);
        ((int)KodySuPhoneType.Unknown).Should().Be(0);
    }
}
