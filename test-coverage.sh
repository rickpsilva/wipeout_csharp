#!/bin/bash

# Script para executar testes com cobertura de código (coverage)

set -e

echo "========================================="
echo "  Wipeout C# - Coverage Report Generator"
echo "========================================="
echo ""

# Build do projeto de testes
echo "Building test project..."
dotnet build wipeout_csharp.Tests/wipeout_csharp.Tests.csproj -v q

# Limpar resultados anteriores
echo "Cleaning previous coverage results..."
rm -rf TestResults/

# Executar testes com cobertura
echo ""
echo "Running tests with coverage collection..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Verificar se reportgenerator está instalado
if ! command -v reportgenerator &> /dev/null
then
    echo ""
    echo "Installing reportgenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Gerar relatório HTML
echo ""
echo "Generating HTML coverage report..."
reportgenerator \
    -reports:"TestResults/**/coverage.cobertura.xml" \
    -targetdir:"TestResults/CoverageReport" \
    -reporttypes:"Html;TextSummary"

# Exibir sumário
echo ""
echo "========================================="
echo "Coverage Summary:"
echo "========================================="
cat TestResults/CoverageReport/Summary.txt

echo ""
echo "========================================="
echo "Full HTML report available at:"
echo "  TestResults/CoverageReport/index.html"
echo "========================================="
echo ""
echo "To view the report, run:"
echo "  xdg-open TestResults/CoverageReport/index.html"
echo ""
