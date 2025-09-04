# Тестирование KodySu.Client

## Стратегия тестирования

Пакет использует комбинированный подход к тестированию API контракта:

### 1. Unit-тесты (с моками)

- **Где**: Все файлы кроме `ApiIntegrationTests.cs`
- **Цель**: Быстрая проверка логики клиента
- **Особенности**: Используют моки, выполняются быстро
- **Запуск**: `dotnet test --filter "Category!=Integration"`

### 2. Verify Snapshot тесты

- **Где**: `ApiContractVerifyTests.cs` и `ApiContractRegressionTests.cs`
- **Цель**: Автоматическое отслеживание изменений в ответах API
- **Особенности**: Создают `.verified.txt` файлы со снепшотами
- **Запуск**: Включены в обычные unit-тесты
- **Классы**:
  - `ApiContractVerifyTests` - основные сценарии API (12 тестов)
  - `ApiContractRegressionTests` - расширенные поля и метаданные (1 тест)

### 3. Integration тесты (реальный API)

- **Где**: `ApiIntegrationTests.cs`
- **Цель**: Проверка реального контракта API kody.su
- **Особенности**: Реальные HTTP запросы, требуют API ключ
- **Запуск**: `dotnet test --filter "Category=Integration"`

## Зачем нужны integration тесты?

Замоканные тесты **не могут** поймать изменения в реальном API:

- Если kody.su изменит структуру JSON ответа
- Если изменятся коды ошибок
- Если появятся новые поля или исчезнут старые

Integration тесты решают эту проблему, делая реальные запросы.

## Настройка API ключа

Для запуска integration тестов нужен API ключ kody.su:

**Для тестирования** можно использовать ключ `test` (с ограниченным лимитом):

```bash
export KodySu__ApiKey="test"
dotnet test --filter "Category=Integration"
```

**Для продакшена** нужно получить реальный ключ, написав на <admin@kody.su>:

```bash
export KodySu__ApiKey="your-real-api-key"
dotnet test --filter "Category=Integration"
```

Или через `appsettings.json`:

```json
{
  "KodySuOptions": {
    "ApiKey": "test"
  }
}
```

## Файловая структура

```bash
tests/
├── README.md                            # Эта документация
├── ApiContractVerifyTests.cs            # Основные snapshot тесты контракта API
├── ApiContractRegressionTests.cs        # Расширенные поля и метаданные API
├── ApiIntegrationTests.cs               # Integration тесты с реальным API
├── Fixtures/
│   ├── Snapshots/                       # Verify снепшоты (.verified.txt)
│   └── Integration/                     # Снепшоты integration тестов
├── CachedKodySuClientTests.cs           # Unit тесты кэширующего клиента
├── CapturedDependencyTests.cs           # Unit тесты DI регистрации
├── KodySuClientTests.cs                 # Unit тесты основного клиента
├── KodySuHttpResponseHandlerTests.cs    # Unit тесты обработки ответов
├── KodySuPhoneTypeTests.cs              # Unit тесты типов телефонов
├── KodySuResultTests.cs                 # Unit тесты результатов
└── ServiceCollectionExtensionsTests.cs # Unit тесты DI расширений
```

## Git и Verify файлы

- `.verified.txt` - **коммитятся** в репозиторий (эталонные снепшоты)
- `.received.txt` - **игнорируются** git (временные файлы)
- При изменении API Verify покажет diff между `.verified.txt` и `.received.txt`

## Команды для разработчиков

```bash
# Все тесты кроме integration
dotnet test --filter "Category!=Integration"

# Только integration тесты
dotnet test --filter "Category=Integration"

# Все тесты (нужен API ключ)
dotnet test

# Обновить снепшоты после изменения API
# (запустить тесты, проверить diff, заапрувить изменения)
```

## Принцип работы

1. **Unit тесты** - проверяют что код работает с ожидаемыми данными
2. **Verify тесты** - автоматически отслеживают изменения в структуре данных
3. **Integration тесты** - подтверждают что реальный API соответствует ожиданиям

Это дает полное покрытие: от внутренней логики до реального API контракта.

## Статистика тестов

- **Всего тестов**: 70
- **Unit тесты**: 66 (включая Verify snapshot тесты)
- **Integration тесты**: 4 (с реальными HTTP вызовами)
- **Snapshot файлов**: 11 (в `Fixtures/Snapshots/` и `Fixtures/Integration/`)
