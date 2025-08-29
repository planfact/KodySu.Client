using System.Text.Json.Serialization;

namespace KodySu.Client;

/// <summary>
/// Результат запроса к KodySu API для телефонного номера
/// </summary>
public sealed record KodySuResult
{
    /// <summary>
    /// Номер телефона, по которому был выполнен запрос
    /// </summary>
    [JsonPropertyName("number_current")]
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Признак успешности запроса по данному номеру
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Тип номера (1 — мобильный РФ, 2 — стационарный РФ, 3 — другие)
    /// </summary>
    [JsonPropertyName("number_type")]
    public int? NumberType { get; init; }

    /// <summary>
    /// Строковое представление типа номера
    /// </summary>
    [JsonPropertyName("number_type_str")]
    public string? NumberTypeString { get; init; }

    /// <summary>
    /// Код ошибки, если определение номера завершилось неудачно
    /// </summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Описание ошибки, если определение номера завершилось неудачно
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// DEF-код (первые 3 цифры мобильного номера РФ)
    /// </summary>
    [JsonPropertyName("def")]
    public string? DefCode { get; init; }

    /// <summary>
    /// 7-значный номер без DEF-кода (для мобильных РФ)
    /// </summary>
    [JsonPropertyName("number")]
    public string? Number { get; init; }

    /// <summary>
    /// Начало диапазона выделенных номеров
    /// </summary>
    [JsonPropertyName("code_start")]
    public string? CodeStart { get; init; }

    /// <summary>
    /// Конец диапазона выделенных номеров
    /// </summary>
    [JsonPropertyName("code_end")]
    public string? CodeEnd { get; init; }

    /// <summary>
    /// Оператор связи (краткое наименование)
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; init; }

    /// <summary>
    /// Оператор связи (полное наименование)
    /// </summary>
    [JsonPropertyName("operator_full")]
    public string? OperatorFull { get; init; }

    /// <summary>
    /// Регион, к которому относится номер
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; init; }

    /// <summary>
    /// Часовой пояс (сдвиг от GMT)
    /// </summary>
    [JsonPropertyName("time")]
    public string? Time { get; init; }

    /// <summary>
    /// Признак наличия номера в базе перенесённых номеров (БДПН)
    /// </summary>
    [JsonPropertyName("bdpn")]
    public bool? IsBdpn { get; init; }

    /// <summary>
    /// Оператор в базе перенесённых номеров (если номер там присутствует)
    /// </summary>
    [JsonPropertyName("bdpn_operator")]
    public string? BdpnOperator { get; init; }

    /// <summary>
    /// Код населённого пункта (для стационарных номеров)
    /// </summary>
    [JsonPropertyName("code")]
    public string? CityCode { get; init; }

    /// <summary>
    /// Название населённого пункта
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; init; }

    /// <summary>
    /// Код страны номера
    /// </summary>
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; init; }

    /// <summary>
    /// Код населённого пункта (для международных номеров)
    /// </summary>
    [JsonPropertyName("city_code")]
    public string? InternationalCityCode { get; init; }

    /// <summary>
    /// Страна номера
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>
    /// Возвращает тип номера как перечисление <see cref="KodySuPhoneType"/>
    /// </summary>
    /// <returns>Тип номера</returns>
    public KodySuPhoneType GetNumberType() => KodySuPhoneTypeExtensions.FromString(NumberTypeString);

    /// <summary>
    /// Возвращает отображаемое имя оператора (приоритет — полное название)
    /// </summary>
    /// <returns>Имя оператора</returns>
    public string GetDisplayOperator() => !string.IsNullOrEmpty(OperatorFull) ? OperatorFull : Operator ?? string.Empty;

    /// <summary>
    /// Признак, что номер — мобильный российский
    /// </summary>
    public bool IsRussianMobile => GetNumberType() == KodySuPhoneType.RussianMobile;

    /// <summary>
    /// Признак, что номер — стационарный российский
    /// </summary>
    public bool IsRussianFixed => GetNumberType() == KodySuPhoneType.RussianFixed;

    /// <summary>
    /// Признак, что номер перенесён (присутствует в БДПН)
    /// </summary>
    public bool IsPortedNumber => IsBdpn == true;
}
