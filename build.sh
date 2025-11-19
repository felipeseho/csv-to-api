#!/bin/bash

# Script de build para CsvToApi

echo "🔨 Building CsvToApi..."

cd "$(dirname "$0")/CsvToApi"

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build release
echo "🏗️  Building release..."
dotnet build -c Release

# Publish self-contained executable
echo "📤 Publishing self-contained executable..."
dotnet publish -c Release -r osx-arm64 --self-contained -o ../publish/osx-arm64
dotnet publish -c Release -r osx-x64 --self-contained -o ../publish/osx-x64
dotnet publish -c Release -r linux-x64 --self-contained -o ../publish/linux-x64
dotnet publish -c Release -r win-x64 --self-contained -o ../publish/win-x64

echo "✅ Build completed!"
echo ""
echo "Executables location:"
echo "  - macOS ARM64: ./publish/osx-arm64/CsvToApi"
echo "  - macOS x64:   ./publish/osx-x64/CsvToApi"
echo "  - Linux x64:   ./publish/linux-x64/CsvToApi"
echo "  - Windows x64: ./publish/win-x64/CsvToApi.exe"

