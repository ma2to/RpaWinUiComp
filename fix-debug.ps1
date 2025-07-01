# fix-debug.ps1 - Riešenie WinUI 3 debug problémov
param(
    [string]$Configuration = "Debug"
)

Write-Host "🔧 Riešim WinUI 3 debug problém..." -ForegroundColor Cyan

try {
    # 1. Zastaviť všetky VS procesy
    Write-Host "`n🛑 Zastavujem Visual Studio procesy..." -ForegroundColor Yellow
    Get-Process -Name "devenv" -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "MSBuild" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2

    # 2. Vyčistiť všetky build výstupy
    Write-Host "`n🧹 Čistím build artefakty..." -ForegroundColor Yellow
    $pathsToClean = @(
        "bin", "obj", 
        "RpaWinUiComponents\bin", "RpaWinUiComponents\obj",
        "RpaWinUiComponents.Demo\bin", "RpaWinUiComponents.Demo\obj"
    )
    
    foreach ($path in $pathsToClean) {
        if (Test-Path $path) {
            Remove-Item -Recurse -Force $path
            Write-Host "  ✅ Vymazané: $path" -ForegroundColor Green
        }
    }

    # 3. Vyčistiť NuGet cache
    Write-Host "`n📦 Čistím NuGet cache..." -ForegroundColor Yellow
    dotnet nuget locals all --clear

    # 4. Skontrolovať a opraviť projekt súbory
    Write-Host "`n🔍 Kontrolujem projekt súbory..." -ForegroundColor Yellow
    
    $demoCsproj = "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj"
    if (Test-Path $demoCsproj) {
        $content = Get-Content $demoCsproj -Raw
        
        # Ensure correct properties for unpackaged deployment
        $requiredProperties = @(
            '<EnableMsixTooling>false</EnableMsixTooling>',
            '<WindowsPackageType>None</WindowsPackageType>',
            '<SelfContained>true</SelfContained>',
            '<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>'
        )
        
        $modified = $false
        foreach ($prop in $requiredProperties) {
            if ($content -notmatch [regex]::Escape($prop)) {
                Write-Host "  ⚠️ Chýba: $prop" -ForegroundColor Yellow
                $modified = $true
            }
        }
        
        if ($modified) {
            Write-Host "  ℹ️ Projekt súbor potrebuje aktualizáciu" -ForegroundColor Blue
        } else {
            Write-Host "  ✅ Projekt súbor je v poriadku" -ForegroundColor Green
        }
    }

    # 5. Restore packages
    Write-Host "`n📦 Obnovujem NuGet balíčky..." -ForegroundColor Yellow
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Restore zlyhal"
    }

    # 6. Build library projekt
    Write-Host "`n🔨 Buildím library projekt..." -ForegroundColor Yellow
    dotnet build "RpaWinUiComponents\RpaWinUiComponents.csproj" --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Build library projektu zlyhal"
    }

    # 7. Build demo projekt
    Write-Host "`n🔨 Buildím demo projekt..." -ForegroundColor Yellow
    dotnet build "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj" --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Build demo projektu zlyhal"
    }

    # 8. Skontrolovať výstupné súbory
    Write-Host "`n🔍 Kontrolujem výstupné súbory..." -ForegroundColor Yellow
    $outputPath = "RpaWinUiComponents.Demo\bin\$Configuration\net8.0-windows10.0.19041.0"
    $requiredFiles = @(
        "RpaWinUiComponents.Demo.exe",
        "RpaWinUiComponents.dll",
        "RpaWinUiComponents.Demo.dll"
    )
    
    foreach ($file in $requiredFiles) {
        $fullPath = Join-Path $outputPath $file
        if (Test-Path $fullPath) {
            Write-Host "  ✅ $file" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $file CHÝBA" -ForegroundColor Red
        }
    }

    # 9. Pokúsiť sa spustiť aplikáciu priamo
    Write-Host "`n🚀 Pokúšam sa spustiť aplikáciu..." -ForegroundColor Yellow
    $exePath = Join-Path $outputPath "RpaWinUiComponents.Demo.exe"
    
    if (Test-Path $exePath) {
        Write-Host "  📁 Spúšťam: $exePath" -ForegroundColor White
        Start-Process $exePath -WorkingDirectory (Split-Path $exePath)
        Write-Host "  ✅ Aplikácia spustená!" -ForegroundColor Green
    } else {
        throw "EXE súbor nebol nájdený: $exePath"
    }

    Write-Host "`n🎉 Debug problém vyriešený!" -ForegroundColor Green
    Write-Host "`n📝 Ďalšie kroky:" -ForegroundColor Cyan
    Write-Host "1. Otvorte Visual Studio" -ForegroundColor White
    Write-Host "2. Nastavte 'RpaWinUiComponents.Demo' ako StartUp projekt" -ForegroundColor White
    Write-Host "3. Vyberte 'RpaWinUiComponents.Demo (Unpackaged)' profil" -ForegroundColor White
    Write-Host "4. Stlačte F5 pre debug" -ForegroundColor White

} catch {
    Write-Host "`n❌ Chyba: $($_.Exception.Message)" -ForegroundColor Red
    
    Write-Host "`n🔧 Manuálne riešenie:" -ForegroundColor Yellow
    Write-Host "1. Zatvorte Visual Studio" -ForegroundColor White
    Write-Host "2. Spustite: dotnet clean" -ForegroundColor White
    Write-Host "3. Spustite: dotnet restore" -ForegroundColor White
    Write-Host "4. Spustite: dotnet build" -ForegroundColor White
    Write-Host "5. Otvorte VS a vyberte 'Unpackaged' profil" -ForegroundColor White
    
    exit 1
}

# Optional: Otvoriť Visual Studio
$openVS = Read-Host "`nChcete otvorit Visual Studio? (y/n)"
if ($openVS -eq 'y' -or $openVS -eq 'Y') {
    if (Test-Path "RpaWinUiComponents.sln") {
        Start-Process "RpaWinUiComponents.sln"
    }
}