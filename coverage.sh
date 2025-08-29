#!/bin/bash

# Скрипт для локальной генерации отчета о покрытии кода

echo "🧪 Запуск тестов с генерацией покрытия..."
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults

echo "📊 Генерация HTML отчета..."
dotnet tool restore > /dev/null 2>&1
dotnet tool install --global dotnet-reportgenerator-globaltool > /dev/null 2>&1

# Ищем файл coverage.cobertura.xml
COVERAGE_FILE=$(find ./TestResults -name "coverage.cobertura.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
    echo "❌ Файл покрытия не найден!"
    exit 1
fi

echo "📁 Найден файл покрытия: $COVERAGE_FILE"

# Генерируем HTML отчет
reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"./CoverageReport" -reporttypes:Html

echo "✅ HTML отчет создан в ./CoverageReport/index.html"
echo "🌐 Откройте файл в браузере для просмотра:"
echo "   file://$(pwd)/CoverageReport/index.html"

# Показываем краткую статистику
if command -v reportgenerator &> /dev/null; then
    echo ""
    echo "📈 Краткая статистика покрытия:"
    reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"./temp" -reporttypes:TextSummary > /dev/null 2>&1
    cat ./temp/Summary.txt 2>/dev/null || echo "Не удалось получить краткую статистику"
    rm -rf ./temp
fi
