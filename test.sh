#!/bin/bash

# Script para executar testes unitários do projeto Wipeout C# Rewrite

set -e

echo "=================================="
echo "  Wipeout C# - Unit Tests Runner"
echo "=================================="
echo ""

# Build do projeto de testes
echo "Building test project..."
dotnet build wipeout_csharp.Tests/wipeout_csharp.Tests.csproj -v q

# Executar testes
echo ""
echo "Running tests..."
dotnet test --no-build --verbosity normal

echo ""
echo "Tests completed!"
