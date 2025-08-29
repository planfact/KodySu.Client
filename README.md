# KodySu.Client

Типобезопасный .NET клиент для работы с [KodySu API v2.1](https://www.kody.su/api?docs) с поддержкой кеширования.

## Особенности

- **Типобезопасность** - используются strongly typed модели вместо магических строк
- **Надежность** - политики повтора (retry), circuit breaker для обработки сбоев
- **HTTP-кеширование** - прозрачное кеширование ответов API (10 минут TTL)
- **Мульти-таргетинг** - поддержка .NET 6.0, 8.0 и 9.0
- **Логирование** - структурированное логирование всех операций
- **Конфигурируемость** - настройка через appsettings.json или код
- **DI интеграция** - готовые расширения для ASP.NET Core

## Установка

```bash
dotnet add package KodySu.Client
```

## Быстрый старт

### 1. Регистрация в DI контейнере

```csharp
// Базовый клиент без кеширования
services.AddKodySuClient(Configuration);

// Кешированный клиент (рекомендуется)
services.AddCachedKodySuClient(Configuration);
```

### 2. Конфигурация через appsettings.json

```json
{
  "KodySuOptions": {
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30
  }
}
```

### 3. Использование

```csharp
public class PhoneService
{
    private readonly IKodySuClient _kodySuClient;

    public PhoneService(IKodySuClient kodySuClient)
    {
        _kodySuClient = kodySuClient;
    }

    public async Task<string> GetOperatorAsync(string phoneNumber)
    {
        var result = await _kodySuClient.SearchPhoneAsync(phoneNumber);

        if (result?.Success == true)
        {
            return result.Operator ?? "Неизвестный оператор";
        }

        return "Номер не найден";
    }

    public async Task<IReadOnlyList<KodySuResult>> SearchMultipleAsync(
        IEnumerable<string> phoneNumbers)
    {
        return await _kodySuClient.SearchPhonesAsync(phoneNumbers);
    }
}
```

## Структура проекта

```text
├── IKodySuClient.cs                      # Интерфейс клиента
├── KodySuClientBase.cs                   # Базовый класс с общей функциональностью
├── KodySuClient.cs                       # Основная реализация HTTP клиента
├── CachedKodySuClient.cs                 # Реализация с HTTP-кэшированием
├── KodySuServiceCollectionExtensions.cs  # Регистрация в DI
├── KodySuClientOptions.cs                # Настройки клиента
├── KodySuHttpResponseHandler.cs          # Обработчик HTTP ответов
├── KodySuResult.cs                       # Модель результата поиска
├── KodySuSearchResponse.cs               # Модель ответа API
├── KodySuPhoneType.cs                    # Перечисление типов номеров
├── KodySuExceptions.cs                   # Исключения клиента
├── KodySuErrorCodes.cs                   # Коды ошибок API
└── PhoneNumber.cs                        # Модель телефонного номера
```

## Конфигурация

### Опции клиента

```csharp
public class KodySuClientOptions : HttpClientOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public int CacheExpiryMinutes { get; set; } = 60;

    // Унаследованные настройки:
    // public string BaseUrl { get; set; } = "https://www.kody.su";
    // public int TimeoutSeconds { get; set; } = 30;
    // public string UserAgent { get; set; } = "Planfact-KodySu-Client/1.0";
}
```

### Расширенная конфигурация

```json
{
  "KodySuOptions": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://www.kody.su",
    "TimeoutSeconds": 60,
    "UserAgent": "MyApp-KodySu-Client/1.0",
    "CacheExpiryMinutes": 60
  }
}
```

## Модели данных

### KodySuResult

```csharp
public class KodySuResult
{
    public string PhoneNumber { get; set; }
    public bool Success { get; set; }
    public string? Operator { get; set; }
    public string? OperatorFull { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DefCode { get; set; }
    // ... другие свойства
}
```

### Типы номеров

```csharp
public enum KodySuPhoneType
{
    RussianMobile,
    RussianFixed,
    Other
}
```

## Кеширование

Кешированный клиент (`CachedKodySuClient`) автоматически кеширует ответы API:

- **TTL**: 10 минут (MediumTerm preset)
- **Размер кеша**: 1,000 записей
- **Автоматическое управление**: очистка старых записей
- **Прозрачность**: кеширование на уровне HTTP ответов

## Обработка ошибок

Клиент использует типизированные исключения для различных типов ошибок:

```csharp
try
{
    var result = await _kodySuClient.SearchPhoneAsync("+79001234567");
}
catch (KodySuAuthenticationException ex)
{
    // Ошибки аутентификации (401, 403)
    Console.WriteLine($"Проблемы с API ключом: {ex.Message}");
}
catch (KodySuValidationException ex)
{
    // Ошибки валидации от API
    Console.WriteLine($"Ошибка валидации: {ex.ErrorCode} - {ex.Message}");
}
catch (KodySuHttpException ex)
{
    // Прочие HTTP ошибки
    Console.WriteLine($"HTTP ошибка: {ex.StatusCode}");
}
```

### Автоматические политики надежности

- **Retry**: автоматические повторы при транзиентных ошибках
- **Circuit Breaker**: защита от каскадных сбоев
- **Rate Limiting**: обработка ответов 429 (Too Many Requests)

## Лицензия

Этот проект лицензирован под [MIT License](LICENSE).
