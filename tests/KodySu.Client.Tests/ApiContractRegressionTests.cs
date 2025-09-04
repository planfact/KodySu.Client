namespace KodySu.Client.Tests;

/// <summary>
/// Тесты для проверки расширенной структуры ответов API KodySu.
/// Содержит специализированные тесты для проверки полных метаданных и дополнительных полей,
/// которые могут присутствовать в ответах API, включая счётчики, коды регионов и международные данные.
/// </summary>
public class ApiContractRegressionTests
{
    [Fact]
    public Task Should_Verify_Extended_Response_Fields()
    {
        // Тест для проверки полной структуры ответа API включая все метаданные:
        // коды операторов, диапазоны номеров, региональную информацию и международные поля
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
