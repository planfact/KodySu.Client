namespace Planfact.KodySu.Client;

/// <summary>
/// Тип номера телефона, возвращаемый KodySu API
/// </summary>
public enum KodySuPhoneType
{
    /// <summary>
    /// Неизвестный тип номера
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Мобильный телефон России
    /// </summary>
    RussianMobile = 1,

    /// <summary>
    /// Стационарный телефон России
    /// </summary>
    RussianFixed = 2,

    /// <summary>
    /// Международный или иной номер
    /// </summary>
    Other = 3
}

/// <summary>
/// Методы-расширения для работы с типами номеров телефонов KodySuPhoneType
/// </summary>
public static class KodySuPhoneTypeExtensions
{
    /// <summary>
    /// Преобразует строковое представление типа номера в <see cref="KodySuPhoneType"/>
    /// </summary>
    /// <param name="numberTypeStr">Строка типа номера из API</param>
    /// <returns>Тип номера телефона</returns>
    public static KodySuPhoneType FromString(string? numberTypeStr)
    {
        return numberTypeStr switch
        {
            "Мобильный РФ" => KodySuPhoneType.RussianMobile,
            "Стационарный РФ" => KodySuPhoneType.RussianFixed,
            "ru_mobile" => KodySuPhoneType.RussianMobile,
            "ru_fixed" => KodySuPhoneType.RussianFixed,
            "ua_mobile" => KodySuPhoneType.Other, // Украинские мобильные относим к "другим"
            "other" => KodySuPhoneType.Other,
            "Другие" => KodySuPhoneType.Other,
            _ => KodySuPhoneType.Unknown
        };
    }

    /// <summary>
    /// Возвращает человекочитаемое описание типа номера
    /// </summary>
    /// <param name="type">Тип номера</param>
    /// <returns>Описание типа номера</returns>
    public static string GetDescription(this KodySuPhoneType type)
    {
        return type switch
        {
            KodySuPhoneType.RussianMobile => "Мобильный телефон России",
            KodySuPhoneType.RussianFixed => "Стационарный телефон России",
            KodySuPhoneType.Other => "Международный номер",
            KodySuPhoneType.Unknown => "Неизвестный тип",
            _ => "Неопределенный тип"
        };
    }
}
