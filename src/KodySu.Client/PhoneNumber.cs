
namespace KodySu.Client;

/// <summary>
/// Value object, представляющий телефонный номер
/// </summary>
public record class PhoneNumber
{
    /// <summary>
    /// Нормализованное значение номера телефона (только цифры)
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Исходное значение номера телефона
    /// </summary>
    public string Raw { get; }

    /// <summary>
    /// Создаёт экземпляр PhoneNumber
    /// </summary>
    /// <param name="value">Исходный номер телефона</param>
    public PhoneNumber(string value)
    {
        Raw = value ?? string.Empty;
        Value = Normalize(Raw);
    }

    /// <summary>
    /// Признак, что номер пустой
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Признак, что номер валидный (только цифры, длина 7-15)
    /// </summary>
    public bool IsValid => !IsEmpty && Value.Length >= 7 && Value.Length <= 15 && Value.All(char.IsDigit);

    /// <summary>
    /// Создаёт PhoneNumber из строки
    /// </summary>
    /// <param name="phoneNumber">Строковое представление номера</param>
    /// <returns>Экземпляр PhoneNumber</returns>
    public static PhoneNumber From(string phoneNumber) => new(phoneNumber);

    /// <summary>
    /// Неявное преобразование PhoneNumber в строку (нормализованное значение)
    /// </summary>
    public static implicit operator string(PhoneNumber? phoneNumber) => phoneNumber?.Value ?? string.Empty;

    /// <summary>
    /// Неявное преобразование строки в PhoneNumber
    /// </summary>
    public static implicit operator PhoneNumber(string phoneNumber) => From(phoneNumber);

    /// <summary>
    /// Возвращает нормализованное строковое представление номера
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Возвращает хеш-код на основе нормализованного значения
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Сравнивает номера по нормализованному значению
    /// </summary>
    public virtual bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    /// <summary>
    /// Нормализует номер телефона для сравнения и хранения
    /// </summary>
    /// <param name="phoneNumber">Исходный номер телефона</param>
    /// <returns>Нормализованный номер телефона (только цифры)</returns>
    private static string Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder(phoneNumber.Length);

        foreach (var c in phoneNumber)
        {
            if (char.IsDigit(c))
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
