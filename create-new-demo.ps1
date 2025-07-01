# create-demo-fixed.ps1 - Opravená verzia pre vytvorenie novej Demo aplikácie
Write-Host "🆕 Vytváram novú Demo aplikáciu..." -ForegroundColor Cyan

try {
    # Kontrola lokácie
    if (!(Test-Path "RpaWinUiComponents.sln")) {
        Write-Host "❌ Nie ste v root adresári! Prejdite do adresára s .sln súborom" -ForegroundColor Red
        exit 1
    }

    # 1. Vymazanie starej Demo aplikácie
    Write-Host "🗑️ Vymazávam starú Demo aplikáciu..." -ForegroundColor Yellow
    if (Test-Path "RpaWinUiComponents.Demo") {
        Remove-Item -Recurse -Force "RpaWinUiComponents.Demo"
        Write-Host "  ✅ Stará Demo aplikácia vymazaná" -ForegroundColor Green
    }

    # Vyčistenie VS cache
    if (Test-Path ".vs") {
        Remove-Item -Recurse -Force ".vs" -ErrorAction SilentlyContinue
        Write-Host "  ✅ Visual Studio cache vymazaná" -ForegroundColor Green
    }

    # 2. Vytvorenie adresárov
    Write-Host "📁 Vytváram adresáre..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "RpaWinUiComponents.Demo" -Force | Out-Null
    New-Item -ItemType Directory -Path "RpaWinUiComponents.Demo\Properties" -Force | Out-Null

    # 3. Vytvorenie .csproj súboru
    Write-Host "📄 Vytváram projekt súbor..." -ForegroundColor Yellow
    $csprojContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <WindowsPackageType>None</WindowsPackageType>
    <EnableMsixTooling>false</EnableMsixTooling>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    <SelfContained>true</SelfContained>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\RpaWinUiComponents\RpaWinUiComponents.csproj" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.6" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.6" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.6" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.6" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.7.250606001" />
    <PackageReference Include="System.Text.Json" Version="9.0.6" />
  </ItemGroup>
</Project>
'@
    $csprojContent | Out-File -FilePath "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj" -Encoding UTF8
    Write-Host "  ✅ .csproj súbor vytvorený" -ForegroundColor Green

    # 4. Vytvorenie launchSettings.json
    Write-Host "📄 Vytváram launchSettings..." -ForegroundColor Yellow
    $launchSettings = @'
{
  "profiles": {
    "RpaWinUiComponents.Demo": {
      "commandName": "Project",
      "launchBrowser": false,
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    },
    "RpaWinUiComponents.Demo (Unpackaged)": {
      "commandName": "Project",
      "launchBrowser": false,
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
'@
    $launchSettings | Out-File -FilePath "RpaWinUiComponents.Demo\Properties\launchSettings.json" -Encoding UTF8
    Write-Host "  ✅ launchSettings.json vytvorený" -ForegroundColor Green

    # 5. Vytvorenie App.xaml
    Write-Host "📄 Vytváram App.xaml..." -ForegroundColor Yellow
    $appXamlContent = @'
<Application
    x:Class="RpaWinUiComponents.Demo.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
'@
    $appXamlContent | Out-File -FilePath "RpaWinUiComponents.Demo\App.xaml" -Encoding UTF8
    Write-Host "  ✅ App.xaml vytvorený" -ForegroundColor Green

    # 6. Vytvorenie App.xaml.cs
    Write-Host "📄 Vytváram App.xaml.cs..." -ForegroundColor Yellow
    $appXamlCsContent = @'
using Microsoft.UI.Xaml;

namespace RpaWinUiComponents.Demo
{
    public partial class App : Application
    {
        private Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
'@
    $appXamlCsContent | Out-File -FilePath "RpaWinUiComponents.Demo\App.xaml.cs" -Encoding UTF8
    Write-Host "  ✅ App.xaml.cs vytvorený" -ForegroundColor Green

    # 7. Vytvorenie MainWindow.xaml
    Write-Host "📄 Vytváram MainWindow.xaml..." -ForegroundColor Yellow
    $mainWindowXamlContent = @'
<Window x:Class="RpaWinUiComponents.Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="RpaWinUiComponents Demo">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Title Bar -->
        <Border Grid.Row="0" Background="#2C3E50" Padding="16,8">
            <TextBlock Text="RpaWinUiComponents Demo" 
                       FontSize="18" 
                       FontWeight="Bold" 
                       Foreground="White"/>
        </Border>

        <!-- Main Content -->
        <StackPanel Grid.Row="1" 
                    Orientation="Vertical" 
                    HorizontalAlignment="Center" 
                    VerticalAlignment="Center"
                    Spacing="20">
            
            <TextBlock x:Name="StatusText" 
                       Text="Demo aplikácia je pripravená!" 
                       FontSize="16"
                       HorizontalAlignment="Center"/>
            
            <Button x:Name="TestButton" 
                    Content="Test RpaWinUiComponents" 
                    Click="OnTestButtonClick"
                    Background="#3498DB" 
                    Foreground="White"
                    Padding="20,10"/>
            
            <TextBlock x:Name="ResultText" 
                       Text=""
                       FontSize="14"
                       Foreground="Green"
                       HorizontalAlignment="Center"/>
        </StackPanel>

        <!-- Status Bar -->
        <Border Grid.Row="2" Background="#34495E" Padding="16,8">
            <TextBlock Text="Pripravené na testovanie" 
                       Foreground="White"/>
        </Border>
    </Grid>
</Window>
'@
    $mainWindowXamlContent | Out-File -FilePath "RpaWinUiComponents.Demo\MainWindow.xaml" -Encoding UTF8
    Write-Host "  ✅ MainWindow.xaml vytvorený" -ForegroundColor Green

    # 8. Vytvorenie MainWindow.xaml.cs
    Write-Host "📄 Vytváram MainWindow.xaml.cs..." -ForegroundColor Yellow
    $mainWindowXamlCsContent = @'
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Test of RpaWinUiComponents integration
                StatusText.Text = "Testuje sa RpaWinUiComponents...";
                TestButton.IsEnabled = false;

                // Basic test
                var testComponent = new RpaWinUiComponents.AdvancedWinUiDataGrid.AdvancedWinUiDataGridControl();
                
                if (testComponent != null)
                {
                    ResultText.Text = "✅ RpaWinUiComponents úspešne načítané!";
                    StatusText.Text = "Test úspešný";
                }
                else
                {
                    ResultText.Text = "❌ Problém s načítaním RpaWinUiComponents";
                    StatusText.Text = "Test neúspešný";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"❌ Chyba: {ex.Message}";
                StatusText.Text = "Test zlyhal";
            }
            finally
            {
                TestButton.IsEnabled = true;
            }
        }
    }
}
'@
    $mainWindowXamlCsContent | Out-File -FilePath "RpaWinUiComponents.Demo\MainWindow.xaml.cs" -Encoding UTF8
    Write-Host "  ✅ MainWindow.xaml.cs vytvorený" -ForegroundColor Green

    # 9. Pridanie do solution
    Write-Host "📄 Pridávam do solution..." -ForegroundColor Yellow
    dotnet sln add "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj"
    Write-Host "  ✅ Projekt pridaný do solution" -ForegroundColor Green

    # 10. Restore a build
    Write-Host "🔨 Building new Demo project..." -ForegroundColor Yellow
    
    Write-Host "  📦 Restore..." -ForegroundColor White
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "  ❌ Restore failed" -ForegroundColor Red
        throw "Restore failed"
    }
    
    Write-Host "  🔨 Building library..." -ForegroundColor White
    dotnet build "RpaWinUiComponents\RpaWinUiComponents.csproj" --configuration Debug --verbosity minimal
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "  ❌ Library build failed" -ForegroundColor Red
        throw "Library build failed"
    }
    
    Write-Host "  🔨 Building demo..." -ForegroundColor White
    dotnet build "RpaWinUiComponents.Demo\RpaWinUiComponents.Demo.csproj" --configuration Debug --verbosity minimal
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "  ❌ Demo build failed" -ForegroundColor Red
        throw "Demo build failed"
    }

    # 11. Test
    Write-Host "🎯 Testujem novú aplikáciu..." -ForegroundColor Yellow
    $exePaths = @(
        "RpaWinUiComponents.Demo\bin\Debug\net8.0-windows10.0.19041.0\RpaWinUiComponents.Demo.exe",
        "RpaWinUiComponents.Demo\bin\Debug\net8.0-windows10.0.19041.0\win-x64\RpaWinUiComponents.Demo.exe"
    )
    
    $foundExe = $null
    foreach ($path in $exePaths) {
        if (Test-Path $path) {
            $foundExe = $path
            Write-Host "  ✅ EXE nájdené: $path" -ForegroundColor Green
            break
        }
    }

    if ($foundExe) {
        $testRun = Read-Host "Spustiť aplikáciu teraz? (y/n)"
        if ($testRun -eq 'y' -or $testRun -eq 'Y') {
            Start-Process $foundExe
            Write-Host "  🚀 Aplikácia spustená!" -ForegroundColor Green
        }
    } else {
        Write-Host "  ⚠️ EXE súbor nebol nájdený v očakávaných lokáciách" -ForegroundColor Yellow
        Write-Host "  📂 Hľadám všetky .exe súbory..." -ForegroundColor Yellow
        Get-ChildItem "RpaWinUiComponents.Demo\bin" -Recurse -Filter "*.exe" -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "    📄 $($_.FullName)" -ForegroundColor White
        }
    }

    Write-Host "`n🎉 Nová Demo aplikácia úspešne vytvorená!" -ForegroundColor Green
    Write-Host "`n📝 Teraz v Visual Studio:" -ForegroundColor Cyan
    Write-Host "1. Otvorte RpaWinUiComponents.sln" -ForegroundColor White
    Write-Host "2. Nastavte 'RpaWinUiComponents.Demo' ako StartUp projekt" -ForegroundColor White
    Write-Host "3. Vyberte profil 'RpaWinUiComponents.Demo (Unpackaged)'" -ForegroundColor White
    Write-Host "4. Stlačte F5" -ForegroundColor White

} catch {
    Write-Host "`n❌ Chyba pri vytváraní Demo aplikácie: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Yellow
    exit 1
}

# Voliteľne otvorte VS
$openVS = Read-Host "`nOtvorit Visual Studio? (y/n)"
if ($openVS -eq 'y' -or $openVS -eq 'Y') {
    if (Test-Path "RpaWinUiComponents.sln") {
        Start-Process "RpaWinUiComponents.sln"
    }
}