# KodySu HTTP Client

Идиоматичный HTTP-клиент для работы с [KodySu API v2.1](https://www.kody.su/api?docs) на базе `Reliable.HttpClient` в .NET 6 (C# 10).

## Особенности

- **Типобезопасность** - используются strongly typed модели вместо магических строк
- **Надежность** - политики повтора (retry), circuit breaker для обработки сбоев из `Reliable.HttpClient`
- **HTTP-уровень кэширования** - использует `Reliable.HttpClient.Caching` для прозрачного кэширования
- **Логирование** - структурированное логирование всех операций
- **Нормализация номеров** - автоматическая нормализация телефонных номеров
- **Конфигурируемость** - настройка через appsettings.json или код
- **Наследование** - общий базовый класс `KodySuClientBase` для устранения дублирования кода

## Структура проекта

```text
├── IKodySuClient.cs              # Интерфейс клиента
├── KodySuClientBase.cs           # Базовый класс с общей функциональностью
├── KodySuClient.cs               # Основная реализация HTTP клиента
├── CachedKodySuClient.cs         # Реализация с HTTP-кэшированием
├── ServiceCollectionExtensions.cs # Регистрация в DI
├── KodySuClientOptions.cs        # Настройки клиента (наследует HttpClientOptions)
├── KodySuHttpResponseHandler.cs  # Обработчик HTTP ответов
├── KodySuResult.cs               # Модель результата поиска
├── KodySuSearchResponse.cs       # Обёртка ответа API
├── KodySuPhoneType.cs            # Типы номеров + расширения
├── KodySuExceptions.cs           # Исключения клиента
├── KodySuErrorCodes.cs           # Коды ошибок API
└── README.md                     # Документация
```

## Быстрый старт

### 1. Регистрация в DI контейнере

```csharp
// Простая регистрация без кэширования
services.AddKodySuClient(options =>
{
    options.ApiKey = "your-api-key";
    options.TimeoutSeconds = 30;
});

// С HTTP-кэшированием (рекомендуется)
services.AddKodySuClient(options =>
{
    options.ApiKey = "your-api-key";
    options.TimeoutSeconds = 30;
})
.AddKodySuClientCaching(); // Использует Reliable.HttpClient.Caching

// С настройкой параметров кэша
services.AddKodySuClient(Configuration)
.AddKodySuClientCaching(cacheOptions =>
{
    cacheOptions.DefaultExpiry = TimeSpan.FromHours(2);    // Время кэширования
    cacheOptions.MaxCacheSize = 50_000;                    // Максимум записей
});

// Регистрация через конфигурацию
services.AddKodySuClient(Configuration)
.AddKodySuClientCaching();
```

### 2. Использование

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
        var details = await _kodySuClient.SearchPhoneAsync(phoneNumber);

        if (details?.Success == true)
        {
            return details.GetDisplayOperator(); // Полное название оператора
        }

        return "Неизвестный оператор";
    }

    public async Task AnalyzeNumberAsync(string phoneNumber)
    {
        var details = await _kodySuClient.SearchPhoneAsync(phoneNumber);

        if (details?.Success == true)
        {
            Console.WriteLine($"Тип: {details.GetNumberType().GetDescription()}");
            Console.WriteLine($"Оператор: {details.GetDisplayOperator()}");

            if (details.IsRussianMobile)
            {
                Console.WriteLine($"DEF-код: {details.DefCode}");

                if (details.IsPortedNumber)
                {
                    Console.WriteLine($"Номер перенесен к: {details.BdpnOperator}");
                }
            }
        }
    }
}
```

## Типы номеров

Клиент поддерживает все типы номеров из API:

- **Мобильные России** (`KodySuPhoneType.RussianMobile`) - DEF-код, оператор, БДПН
- **Стационарные России** (`KodySuPhoneType.RussianFixed`) - код города, оператор
- **Международные** (`KodySuPhoneType.Other`) - код страны, город
- **Мобильные Украины** - относятся к категории "Other"

## Конфигурация

```csharp
/// <summary>
/// Настройки KodySu клиента (наследует от HttpClientOptions)
/// </summary>
public class KodySuClientOptions : HttpClientOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public int CacheExpiryMinutes { get; set; } = 60; // Используется для справки

    // Унаследованные от HttpClientOptions:
    // public string BaseUrl { get; set; } = "https://www.kody.su";
    // public int TimeoutSeconds { get; set; } = 30;
    // public int MaxRetries { get; set; } = 3;
    // public int BaseDelayMs { get; set; } = 1000;
    // public string UserAgent { get; set; } = "Planfact-KodySu-Client/1.0";
}
```

### Через appsettings.json

```json
{
  "KodySuOptions": {
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 60,
    "MaxRetries": 5,
    "BaseDelayMs": 2000
  }
}
```

## Обработка ошибок

Клиент использует надежную обработку ошибок на основе `Reliable.HttpClient`:

### **Автоматические политики (Reliable.HttpClient):**

- **TooManyRequests (429)** - автоматический retry с экспоненциальной задержкой
- **Транзиентные ошибки** - сетевые сбои, таймауты, 5xx ошибки
- **Circuit Breaker** - защита от каскадных сбоев

### **Исключения клиента:**

- `KodySuConfigurationException` - ошибки конфигурации
- `KodySuAuthenticationException` - ошибки аутентификации (401, 403)
- `KodySuValidationException` - ошибки валидации от API (с кодом ошибки)
- `KodySuHttpException` - прочие HTTP ошибки

```csharp
try
{
    var result = await _kodySuClient.SearchPhoneAsync("+79001234567");
}
catch (KodySuAuthenticationException ex)
{
    // Проблемы с API ключом
}
catch (KodySuValidationException ex)
{
    // API вернул ошибку валидации
    Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
}
catch (KodySuHttpException ex)
{
    // HTTP ошибки
    Console.WriteLine($"HTTP {ex.StatusCode}");
}
```

## Расширения моделей

`KodySuResult` содержит удобные методы:

```csharp
var details = await _kodySuClient.SearchPhoneAsync("+79001234567");

// Типизированный тип номера
KodySuPhoneType type = details.GetNumberType();

// Полное название оператора (приоритет - OperatorFull)
string operatorName = details.GetDisplayOperator();

// Проверки типов
bool isMobile = details.IsRussianMobile;
bool isFixed = details.IsRussianFixed;
bool isPorted = details.IsPortedNumber;
```

## Производительность и архитектура

### **HTTP-уровень кэширования (Reliable.HttpClient.Caching)**

- **Прозрачное кэширование**: HTTP запросы кэшируются автоматически на уровне `CachedHttpClient`
- **Время жизни по умолчанию**: 1 час с возможностью настройки
- **Ограничение размера**: максимум 10,000 записей с автоматической очисткой
- **Кэширование HTTP ответов**: кэшируется `KodySuSearchResponse`, а не результаты

### **Архитектурные преимущества**

- **Базовый класс `KodySuClientBase`**: устранение дублирования кода между `KodySuClient` и `CachedKodySuClient`
- **Наследование функциональности**: общие методы (нормализация номеров, построение URI, логирование)
- **Reliable.HttpClient интеграция**: использование проверенных паттернов надежности
- **Eager validation**: правильная валидация аргументов в async методах (устранение EPC37)

### **Производительность**

- **Пулинг подключений**: используется стандартный HttpClient пул
- **Экспоненциальная задержка**: интеллигентная обработка rate limiting
- **Circuit breaker**: предотвращение каскадных сбоев
- **Индивидуальные запросы**: корректное использование KodySu API (без ложной пакетной обработки)

## Логирование

Клиент использует структурированное логирование:

```text
[Debug] Поиск информации о номере телефона: {PhoneNumber}
[Debug] Запрос успешен. Квота: {Quota}, найдено номеров: {Count}
[Warning] Ошибка при определении номера {PhoneNumber}: {ErrorCode} - {ErrorMessage}
[Error] HTTP ошибка при запросе к KodySu API: {ErrorMessage}
```

## Совместимость

- **.NET 6+**
- **KodySu API v2.1**
- Обратно несовместимо со старыми версиями клиента (новая структура моделей)

## Зависимости

- **Reliable.HttpClient** - основа для HTTP клиента с политиками надежности
- **Reliable.HttpClient.Caching** - HTTP-уровень кэширования
