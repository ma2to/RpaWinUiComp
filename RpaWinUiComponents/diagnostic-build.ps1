# Diagnostický build script pre WinUI 3 + .NET 8
# Spustite z root adresára solution

Write-Host "🔍 Diagnostika WinUI 3 + .NET 8 projektu..." -ForegroundColor Cyan

# Kontrola .NET verzie
Write-Host "`n📋 Kontrola .NET SDK..." -ForegroundColor Yellow
dotnet --version
dotnet --list-sdks

# Kontrola Windows App SDK
Write-Host "`n📋 Kontrola Windows App SDK..." -ForegroundColor Yellow
Get-AppxPackage | Where-Object {$_.Name -like "*WindowsAppRuntime*"} | Select-Object Name, Version

# Vyčistenie projektu
Write-Host "`n🧹 Vyčisťovanie build artefaktov..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "packages") { Remove-Item -Recurse -Force "packages" }
if (Test-Path "RpaWinUiComponents\bin") { Remove-Item -Recurse -Force "RpaWinUiComponents\bin" }
if (Test-Path "RpaWinUiComponents\obj") { Remove-Item -Recurse -Force "RpaWinUiComponents\obj" }
if (Test-Path "RpaWinUiComponents.Demo\bin") { Remove-Item -Recurse -Force "RpaWinUiComponents.Demo\bin" }
if (Test-Path "RpaWinUiComponents.Demo\obj") { Remove-Item -Recurse -Force "RpaWinUiComponents.Demo\obj" }

Write-Host "✅ Build artefakty vyčistené" -ForegroundColor Green

# Restore packages
Write-Host "`n📦 Obnovenie NuGet balíčkov..." -ForegroundColor Yellow
dotnet restore --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Chyba pri restore" -ForegroundColor Red
    exit 1
}
Write-Host "✅ NuGet packages obnovené" -ForegroundColor Green

# Clean solution
Write-Host "`n🧽 Čistenie solution..." -ForegroundColor Yellow
dotnet clean --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Chyba pri clean" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Solution vyčistená" -ForegroundColor Green

# Build Library first
Write-Host "`n🔨 Building Library projekt..." -ForegroundColor Yellow
dotnet build "RpaWinUiComponents\RpaWinUiComponents.csproj" --configuration Debug --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Chyba pri build Library projektu" -ForegroundColor Red
    Write-Host "🔍 Skontrolujte XAML súbory a namespaces v Library projekte" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Library projekt úspešne zostavený" -ForegroundColor Green

# Build Demo project
Write-Host "`n🔨 Building Demo projekt..." -ForegroundColor Yellow
dotnet build "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj" --configuration Debug --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Chyba pri build Demo projektu" -ForegroundColor Red
    Write-Host "🔍 Skontrolujte namespace imports v MainWindow.xaml" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Demo projekt úspešne zostavený" -ForegroundColor Green

# Check generated files
Write-Host "`n🔍 Kontrola vygenerovaných súborov..." -ForegroundColor Yellow
$demoObjPath = "RpaWinUiComponents.Demo\obj\Debug\net8.0-windows10.0.26100.0"
$libraryObjPath = "RpaWinUiComponents\obj\Debug\net8.0-windows10.0.26100.0"

if (Test-Path "$demoObjPath\App.g.i.cs") {
    Write-Host "✅ App.g.i.cs vygenerovaný" -ForegroundColor Green
} else {
    Write-Host "❌ App.g.i.cs NEVYGENEROVANÝ" -ForegroundColor Red
}

if (Test-Path "$demoObjPath\MainWindow.g.i.cs") {
    Write-Host "✅ MainWindow.g.i.cs vygenerovaný" -ForegroundColor Green
} else {
    Write-Host "❌ MainWindow.g.i.cs NEVYGENEROVANÝ" -ForegroundColor Red
}

if (Test-Path "$libraryObjPath\AdvancedWinUiDataGrid\Views\AdvancedDataGridControl.g.cs") {
    Write-Host "✅ AdvancedDataGridControl.g.cs vygenerovaný" -ForegroundColor Green
} else {
    Write-Host "❌ AdvancedDataGridControl.g.cs NEVYGENEROVANÝ" -ForegroundColor Red
}

Write-Host "`n🎉 Diagnostika dokončená!" -ForegroundColor Cyan
Write-Host "Ak sa všetky súbory úspešne zostavili, môžete otvoriť Visual Studio a spustiť projekt." -ForegroundColor White

# Optional: Start Visual Studio
$choice = Read-Host "`nChcete otvoriť Visual Studio? (y/n)"
if ($choice -eq 'y' -or $choice -eq 'Y') {
    if (Test-Path "RpaWinUiComponents.sln") {
        Start-Process "RpaWinUiComponents.sln"
    }
}