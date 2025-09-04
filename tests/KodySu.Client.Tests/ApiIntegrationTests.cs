using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using FluentAssertions;

namespace KodySu.Client.Tests;

/// <summary>
/// Integration тесты для проверки реального API kody.su
/// Эти тесты делают настоящие HTTP запросы и проверяют актуальный контракт API
/// </summary>
[Collection("Integration")]
public class ApiIntegrationTests : IAsyncLifetime
{
    private readonly IKodySuClient _client;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _apiKey;

    // Кэшированные опции JSON сериализации для переиспользования
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Тестовые номера для проверки (не персональные данные)
    private const string TestMobileNumber = "79161234567";  // Тестовый мобильный
    private const string TestFixedNumber = "74951234567";   // Тестовый стационарный
    private const string InvalidNumber = "123";             // Заведомо некорректный

    public ApiIntegrationTests()
    {
        // Настройка реального клиента для integration тестов
        _apiKey = Environment.GetEnvironmentVariable("KodySu__ApiKey") ?? throw new InvalidOperationException(
            "Переменная окружения KodySu__ApiKey не установлена. Установите реальный API ключ для integration тестов.");

        var configDict = new Dictionary<string, string?>
        {
            ["KodySuOptions:ApiKey"] = _apiKey,
            ["KodySuOptions:BaseUrl"] = "https://www.kody.su/api/v2.1/",
            ["KodySuOptions:TimeoutSeconds"] = "30"
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Добавляем реальный клиент (без моков!)
        services.AddKodySuClient(configuration);

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<IKodySuClient>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RealAPI_SearchMobileNumber_ReturnsValidResponse()
    {
        // Act - делаем реальный запрос к API
        KodySuResult? result = await _client.SearchPhoneAsync(TestMobileNumber);

        // Assert - проверяем структуру реального ответа
        if (result != null)
        {
            // Проверяем обязательные поля
            result.PhoneNumber.Should().NotBeNullOrEmpty();
            result.Success.Should().BeTrue();

            // Если это мобильный номер, проверяем специфичные поля
            if (result.IsRussianMobile)
            {
                result.NumberType.Should().Be(1);
                result.NumberTypeString.Should().NotBeNullOrEmpty();
                result.DefCode.Should().NotBeNullOrEmpty();
                result.Operator.Should().NotBeNullOrEmpty();
            }
        }

        // Используем Verify для фиксации актуальной структуры ответа
        await Verify(result)
            .UseDirectory("Fixtures/Integration")
            .UseFileName("RealAPI_MobileNumber_Response");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RealAPI_SearchInvalidNumber_HandlesGracefully()
    {
        // Act & Assert - проверяем обработку некорректного номера
        Exception exception = await Record.ExceptionAsync(async () => await _client.SearchPhoneAsync(InvalidNumber).AsTask());

        // API может вернуть null, пустой результат или выбросить исключение
        // Главное - не должно быть необработанных исключений
        exception?.Should().BeAssignableTo<KodySuException>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RealAPI_ResponseStructure_MatchesExpectedContract()
    {
        // Этот тест проверяет что API возвращает JSON который наши модели могут десериализовать

        // Делаем запрос через HttpClient напрямую чтобы получить raw JSON
        using var httpClient = new HttpClient();

        // Используем правильный endpoint и параметры как в клиенте
        var url = $"https://www.kody.su/api/v2.1/search.json?q={Uri.EscapeDataString(TestMobileNumber)}&key={Uri.EscapeDataString(_apiKey)}";
        HttpResponseMessage response = await httpClient.GetAsync(url);

        response.Should().BeSuccessful("API должен отвечать успешно");

        var jsonContent = await response.Content.ReadAsStringAsync();
        jsonContent.Should().NotBeNullOrWhiteSpace();

        // Пытаемся десериализовать реальный JSON в наши модели
        KodySuSearchResponse? deserializedResponse = JsonSerializer.Deserialize<KodySuSearchResponse>(
            jsonContent, s_jsonOptions);

        deserializedResponse.Should().NotBeNull("JSON должен корректно десериализоваться");

        // Проверяем основную структуру ответа (без переменной квоты)
        deserializedResponse!.Numbers.Should().NotBeEmpty();
        deserializedResponse.Numbers[0].Success.Should().BeTrue();

        // Фиксируем структуру реального JSON ответа (убираем переменную квота)
        var jsonForSnapshot = jsonContent.Replace($"\"quota\": {deserializedResponse.Quota}", "\"quota\": \"[VARIABLE]\"");
        await Verify(jsonForSnapshot)
            .UseDirectory("Fixtures/Integration")
            .UseFileName("RealAPI_RawJSON_Response");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RealAPI_MultipleRequests_QuotaHandling()
    {
        // Проверяем как API обрабатывает квоты и множественные запросы
        var numbers = new[] { TestMobileNumber, TestFixedNumber };

        IReadOnlyList<KodySuResult> results = await _client.SearchPhonesAsync(numbers);

        // Проверяем что получили ответы
        results.Should().NotBeNull();

        // Фиксируем структуру множественного ответа
        await Verify(results)
            .UseDirectory("Fixtures/Integration")
            .UseFileName("RealAPI_MultipleNumbers_Response");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }
}

/// <summary>
/// Отдельная коллекция для integration тестов чтобы они не выполнялись параллельно
/// (чтобы не превысить лимиты API)
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationTestCollection
{
}
