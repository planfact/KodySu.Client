namespace KodySu.Client;

/// <summary>
/// Контракт клиента для работы с KodySu API (поиск информации по телефонным номерам).
/// Номера принимаются в произвольном формате; конкретные реализации заботятся о нормализации,
/// обработке ошибок и, при необходимости, об использовании кэширования HTTP-ответов.
/// </summary>
public interface IKodySuClient
{
    /// <summary>
    /// Асинхронный поиск информации о телефонном номере.
    /// </summary>
    /// <param name="phoneNumber">Номер в произвольном формате. Будет нормализован внутренне.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о номере телефона или <c>null</c>, если не найден.</returns>
    ValueTask<KodySuResult?> SearchPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Асинхронный поиск информации по нескольким номерам.
    /// </summary>
    /// <param name="phoneNumbers">Коллекция номеров для поиска. <c>null</c> недопустим.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных результатов. При пустой коллекции возвращает пустой список.</returns>
    Task<IReadOnlyList<KodySuResult>> SearchPhonesAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удобная перегрузка для точечных вызовов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="phoneNumbers">Массив номеров. Может быть пустым.</param>
    /// <returns>То же, что и основная перегрузка.</returns>
    Task<IReadOnlyList<KodySuResult>> SearchPhonesAsync(
        CancellationToken cancellationToken = default,
        params string[] phoneNumbers);
}
