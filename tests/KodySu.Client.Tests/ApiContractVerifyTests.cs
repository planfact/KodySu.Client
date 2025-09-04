using System.Text.Json;

using FluentAssertions;

namespace KodySu.Client.Tests;

/// <summary>
/// Тесты для проверки контракта API KodySu с использованием Verify для snapshot тестирования
/// </summary>
public class ApiContractVerifyTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true  // Для читаемых snapshot'ов
    };

    [Fact]
    public Task Should_Verify_Successful_Mobile_Search_Response()
    {
        // Arrange - создаём типичный успешный ответ для мобильного номера
        var response = new KodySuSearchResponse
        {
            Quota = 485,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79161234567",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    DefCode = "916",
                    Number = "1234567",
                    CodeStart = "9160000000",
                    CodeEnd = "9169999999",
                    Operator = "МТС",
                    OperatorFull = "ПАО \"Мобильные ТелеСистемы\"",
                    Region = "Москва и Московская область",
                    Time = "+3 MSK",
                    IsBdpn = false,
                    BdpnOperator = null
                }
            ]
        };

        // Act & Assert - Verify зафиксирует JSON-снимок в каталоге Fixtures/Snapshots
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_Number_Not_Found_Response()
    {
        // Arrange - ответ когда номер не найден
        var response = new KodySuSearchResponse
        {
            Quota = 484,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79999999999",
                    Success = false,
                    ErrorCode = "PHONE_NOT_FOUND",
                    ErrorMessage = "Номер не найден в базе данных"
                }
            ]
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_Empty_Numbers_Response()
    {
        // Arrange - ответ с пустым массивом номеров
        var response = new KodySuSearchResponse
        {
            Quota = 483,
            Numbers = []
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_Multiple_Numbers_Response()
    {
        // Arrange - ответ с несколькими номерами (успешные и с ошибками)
        var response = new KodySuSearchResponse
        {
            Quota = 481,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79161111111",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    DefCode = "916",
                    Operator = "МТС",
                    Region = "Москва"
                },
                new KodySuResult
                {
                    PhoneNumber = "79162222222",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    DefCode = "916",
                    Operator = "Билайн",
                    Region = "Московская область"
                },
                new KodySuResult
                {
                    PhoneNumber = "79163333333",
                    Success = false,
                    ErrorCode = "INVALID_NUMBER",
                    ErrorMessage = "Некорректный номер"
                }
            ]
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_Fixed_Line_Response()
    {
        // Arrange - ответ для стационарного номера
        var response = new KodySuSearchResponse
        {
            Quota = 480,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "74951234567",
                    Success = true,
                    NumberType = 2,
                    NumberTypeString = "Стационарный РФ",
                    CityCode = "495",
                    City = "Москва",
                    Operator = "МГТС",
                    OperatorFull = "ПАО \"Московская городская телефонная сеть\"",
                    Region = "Москва",
                    Time = "+3 MSK",
                    CountryCode = "7",
                    InternationalCityCode = "495",
                    Country = "Россия"
                }
            ]
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_Ported_Number_Response()
    {
        // Arrange - ответ для перенесённого номера (БДПН)
        var response = new KodySuSearchResponse
        {
            Quota = 479,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79161234567",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    DefCode = "916",
                    Operator = "Билайн",
                    OperatorFull = "ООО \"Вымпел-Коммуникации\"",
                    Region = "Москва и Московская область",
                    IsBdpn = true,
                    BdpnOperator = "МТС"
                }
            ]
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public Task Should_Verify_API_Error_Response()
    {
        // Arrange - ответ с ошибкой на уровне API
        var response = new KodySuSearchResponse
        {
            Quota = 0,
            Numbers = [],
            ErrorCode = "INVALID_PHONE",
            ErrorMessage = "Некорректный формат номера телефона"
        };

        // Act & Assert
        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }

    [Fact]
    public void Should_Deserialize_Response_From_Generated_Json()
    {
        // Этот тест проверяет, что сгенерированные Verify JSON-файлы
        // корректно десериализуются обратно в объекты

        // Arrange - создаём объект и сериализуем в JSON
        var originalResponse = new KodySuSearchResponse
        {
            Quota = 485,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79161234567",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    Operator = "МТС",
                    OperatorFull = "ПАО \"Мобильные ТелеСистемы\""
                }
            ]
        };

        var json = JsonSerializer.Serialize(originalResponse, _jsonOptions);

        // Act - десериализуем обратно
        KodySuSearchResponse? deserializedResponse = JsonSerializer.Deserialize<KodySuSearchResponse>(json, _jsonOptions);

        // Assert - проверяем, что данные совпадают
        deserializedResponse.Should().NotBeNull();
        deserializedResponse!.Quota.Should().Be(originalResponse.Quota);
        deserializedResponse.Numbers.Should().HaveCount(1);

        KodySuResult result = deserializedResponse.Numbers[0];
        KodySuResult originalResult = originalResponse.Numbers[0];

        result.PhoneNumber.Should().Be(originalResult.PhoneNumber);
        result.Success.Should().Be(originalResult.Success);
        result.NumberType.Should().Be(originalResult.NumberType);
        result.Operator.Should().Be(originalResult.Operator);
        result.OperatorFull.Should().Be(originalResult.OperatorFull);
    }

    [Fact]
    public void Should_Handle_Html_Escaped_Content()
    {
        // Arrange - JSON с HTML-экранированием (как в реальном API)
        var jsonWithEscaping = """
        {
          "quota": 485,
          "numbers": [
            {
              "number_current": "79161234567",
              "number_success": true,
              "operator": "МТС",
              "operator_full": "ПАО &quot;Мобильные ТелеСистемы&quot;"
            }
          ]
        }
        """;

        // Act
        KodySuSearchResponse? response = JsonSerializer.Deserialize<KodySuSearchResponse>(jsonWithEscaping, _jsonOptions);

        // Assert
        response.Should().NotBeNull();
        KodySuResult result = response!.Numbers[0];

        // HTML-декодирование не должно происходить автоматически - это задача UI
        result.OperatorFull.Should().Be("ПАО &quot;Мобильные ТелеСистемы&quot;");

        // Но метод GetDisplayOperator() должен возвращать полное имя
        result.GetDisplayOperator().Should().Be("ПАО &quot;Мобильные ТелеСистемы&quot;");
    }

    [Fact]
    public void Should_Ignore_Unknown_Json_Properties()
    {
        // Arrange - JSON с дополнительными полями, которых нет в модели
        var jsonWithExtraFields = """
        {
          "quota": 485,
          "some_new_field": "should_be_ignored",
          "numbers": [
            {
              "number_current": "79161234567",
              "number_success": true,
              "operator": "МТС",
              "future_field": {
                "nested": "data"
              },
              "extra_array": [1, 2, 3]
            }
          ],
          "api_version": "v2.1"
        }
        """;

        // Act & Assert - не должно падать с ошибкой
        KodySuSearchResponse? response = JsonSerializer.Deserialize<KodySuSearchResponse>(jsonWithExtraFields, _jsonOptions);

        response.Should().NotBeNull();
        response!.Quota.Should().Be(485);
        response.Numbers.Should().HaveCount(1);
        response.Numbers[0].PhoneNumber.Should().Be("79161234567");
        response.Numbers[0].Success.Should().BeTrue();
    }

    [Fact]
    public void Should_Handle_Json_With_Null_Values()
    {
        // Arrange - JSON с null значениями (как может прийти от API)
        var jsonWithNulls = """
        {
          "quota": 485,
          "numbers": [
            {
              "number_current": "79161234567",
              "number_success": true,
              "number_type": null,
              "number_type_str": null,
              "def": null,
              "operator": null,
              "operator_full": null,
              "region": null,
              "time": null,
              "bdpn": null,
              "bdpn_operator": null
            }
          ]
        }
        """;

        // Act
        KodySuSearchResponse? response = JsonSerializer.Deserialize<KodySuSearchResponse>(jsonWithNulls, _jsonOptions);

        // Assert
        response.Should().NotBeNull();
        KodySuResult result = response!.Numbers[0];
        result.PhoneNumber.Should().Be("79161234567");
        result.Success.Should().BeTrue();
        result.NumberType.Should().BeNull();
        result.DefCode.Should().BeNull();
        result.Operator.Should().BeNull();
        result.IsBdpn.Should().BeNull();
        result.IsPortedNumber.Should().BeFalse(); // null IsBdpn означает false
    }

    [Fact]
    public Task Should_Verify_Extended_Fields_Response()
    {
        // Специальный тест для проверки всех полей счётчиков и метаданных
        var response = new KodySuSearchResponse
        {
            Quota = 485,
            Numbers = [
                new KodySuResult
                {
                    PhoneNumber = "79161234567",
                    Success = true,
                    NumberType = 1,
                    NumberTypeString = "Мобильный РФ",
                    DefCode = "916",                    // DEF-код
                    Number = "1234567",                 // 7-значный номер
                    CodeStart = "9160000000",           // Начало диапазона
                    CodeEnd = "9169999999",             // Конец диапазона
                    Operator = "МТС",                   // Краткое имя оператора
                    OperatorFull = "ПАО \"МТС\"",       // Полное имя оператора
                    Region = "Москва",                  // Регион
                    Time = "+3 MSK",                    // Часовой пояс
                    IsBdpn = false,                     // БДПН признак
                    BdpnOperator = null,                // БДПН оператор
                    CityCode = null,                    // Код города (для стационарных)
                    City = null,                        // Город
                    CountryCode = "7",                  // Код страны
                    InternationalCityCode = null,       // Международный код города
                    Country = "Россия"                  // Страна
                }
            ]
        };

        return Verify(response)
            .UseDirectory("Fixtures/Snapshots");
    }
}
