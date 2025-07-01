# Build script pre RpaWinUiComponents NuGet balíček
param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release"
)

Write-Host "🏗️ Building RpaWinUiComponents NuGet package v$Version..." -ForegroundColor Cyan

try {
    # 1. Clean previous builds
    Write-Host "`n🧹 Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path "RpaWinUiComponents\bin") { Remove-Item -Recurse -Force "RpaWinUiComponents\bin" }
    if (Test-Path "RpaWinUiComponents\obj") { Remove-Item -Recurse -Force "RpaWinUiComponents\obj" }
    
    # Clear NuGet cache
    dotnet nuget locals all --clear
    Write-Host "✅ Cleaned successfully" -ForegroundColor Green

    # 2. Restore dependencies
    Write-Host "`n📦 Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore "RpaWinUiComponents\RpaWinUiComponents.csproj" --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed"
    }
    Write-Host "✅ Dependencies restored" -ForegroundColor Green

    # 3. Build Library project
    Write-Host "`n🔨 Building RpaWinUiComponents library..." -ForegroundColor Yellow
    dotnet build "RpaWinUiComponents\RpaWinUiComponents.csproj" --configuration $Configuration --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "✅ Library built successfully" -ForegroundColor Green

    # 4. Create NuGet package
    Write-Host "`n📦 Creating NuGet package..." -ForegroundColor Yellow
    dotnet pack "RpaWinUiComponents\RpaWinUiComponents.csproj" --configuration $Configuration --output "./nupkg" --verbosity normal -p:PackageVersion=$Version
    if ($LASTEXITCODE -ne 0) {
        throw "Pack failed"
    }

    # 5. Verify package contents
    Write-Host "`n🔍 Verifying package contents..." -ForegroundColor Yellow
    $packagePath = "./nupkg/RpaWinUiComponents.$Version.nupkg"
    
    if (Test-Path $packagePath) {
        Write-Host "✅ Package created: $packagePath" -ForegroundColor Green
        
        # Extract and show contents
        $extractPath = "./nupkg/extracted"
        if (Test-Path $extractPath) { Remove-Item -Recurse -Force $extractPath }
        
        try {
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            [System.IO.Compression.ZipFile]::ExtractToDirectory($packagePath, $extractPath)
            
            Write-Host "`n📋 Package contents:" -ForegroundColor Cyan
            Get-ChildItem $extractPath -Recurse | ForEach-Object {
                $relativePath = $_.FullName.Substring($extractPath.Length + 1)
                if ($_.PSIsContainer) {
                    Write-Host "  📁 $relativePath" -ForegroundColor Blue
                } else {
                    Write-Host "  📄 $relativePath" -ForegroundColor White
                }
            }
            
            # Check critical files
            $criticalFiles = @(
                "lib\net8.0-windows10.0.19041.0\RpaWinUiComponents.dll",
                "build\RpaWinUiComponents.targets",
                "buildTransitive\RpaWinUiComponents.targets"
            )
            
            Write-Host "`n✅ Critical files check:" -ForegroundColor Green
            foreach ($file in $criticalFiles) {
                $fullPath = Join-Path $extractPath $file
                if (Test-Path $fullPath) {
                    Write-Host "  ✅ $file" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ $file MISSING" -ForegroundColor Red
                }
            }
        }
        catch {
            Write-Host "⚠️ Could not extract package for verification: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        throw "Package not created"
    }

    # 6. Install to local NuGet cache for testing
    Write-Host "`n📥 Installing to local NuGet cache..." -ForegroundColor Yellow
    $localNuGetPath = "$env:USERPROFILE\.nuget\packages\rpawinuicomponents\$Version"
    if (Test-Path $localNuGetPath) {
        Remove-Item -Recurse -Force $localNuGetPath
    }
    
    try {
        # Copy package content to local cache
        $packageContent = "./nupkg/extracted"
        $targetPath = "$env:USERPROFILE\.nuget\packages\rpawinuicomponents\$Version"
        
        if (Test-Path $packageContent) {
            New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            Copy-Item -Recurse -Path "$packageContent\*" -Destination $targetPath
            Write-Host "✅ Package installed to local cache: $targetPath" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Package content not extracted, skipping local cache installation" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⚠️ Could not install to local cache: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    Write-Host "`n🎉 Build completed successfully!" -ForegroundColor Green
    Write-Host "📦 Package: $packagePath" -ForegroundColor White
    
    Write-Host "`n📝 Next steps:" -ForegroundColor Cyan
    Write-Host "1. Update Demo project to use version $Version" -ForegroundColor White
    Write-Host "2. Run: dotnet restore in Demo project" -ForegroundColor White
    Write-Host "3. Build and test Demo project" -ForegroundColor White

}
catch {
    Write-Host "`n❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}