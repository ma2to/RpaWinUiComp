# 🚀 RpaWinUiComponents - Advanced DataGrid

**Pokročilý WinUI 3 DataGrid komponent s real-time validáciou, copy/paste funkcionalitou a Clean Architecture.**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![WinUI 3](https://img.shields.io/badge/WinUI-3.0-green.svg)](https://docs.microsoft.com/en-us/windows/apps/winui/)
[![C#](https://img.shields.io/badge/C%23-11.0-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📋 Obsah

- [🌟 Kľúčové funkcie](#-kľúčové-funkcie)
- [🏗️ Architektúra](#️-architektúra)
- [📦 Inštalácia](#-inštalácia)
- [🚀 Rýchly štart](#-rýchly-štart)
- [📖 Pokročilé použitie](#-pokročilé-použitie)
- [🎮 Ovládanie](#-ovládanie)
- [🧪 Testovanie](#-testovanie)
- [🔧 Konfigurácia](#-konfigurácia)
- [📝 API dokumentácia](#-api-dokumentácia)
- [🤝 Prispievanie](#-prispievanie)

## 🌟 Kľúčové funkcie

### ✅ Implementované funkcionality

- **🏛️ Clean Architecture** s Dependency Injection a MVVM
- **⚡ Real-time validácie** s throttling podporou (100-500ms)
- **📋 Copy/Paste** funkcionalita kompatibilná s Excel formátom
- **🔤 Keyboard Navigation** (Tab, Enter, F2, ESC, Delete, Shift+Enter)
- **📊 Flexibilné stĺpce** s konfigurovateľnými typmi a validáciami
- **🎯 Špeciálne stĺpce** (DeleteAction, ValidAlerts) s automatickým umiestnením
- **📤 Export/Import** dát (DataTable, Dictionary, CSV)
- **🔍 Custom validačné pravidlá** s podmienečnou aplikáciou
- **🎨 Moderné WinUI 3 dizajn** s Visual States a animáciami
- **📱 Responsive design** s podporou rôznych veľkostí obrazovky
- **♿ Accessibility** podpora s správnym focus managementom
- **🚀 Vysoký výkon** vďaka ItemsRepeater a virtualizácii
- **🔧 Extensibility** cez interfaces a dependency injection

### 🎮 Ovládanie klávesnicou

| Klávesa | Funkcia |
|---------|---------|
| `Tab` / `Shift+Tab` | Navigácia medzi bunkami |
| `Enter` | Potvrdenie a prechod na ďalší riadok |
| `F2` | Začatie editácie bunky |
| `ESC` | Zrušenie zmien |
| `Delete` | Vymazanie obsahu bunky |
| `Shift+Enter` | Nový riadok v bunke |
| `Ctrl+C` | Kopírovanie |
| `Ctrl+V` | Vkladanie |

## 🏗️ Architektúra

```
RpaWinUiComponents/
├── AdvancedWinUiDataGrid/           # 🏠 Hlavný komponent
│   ├── Views/                       # 🖼️ UserControls a XAML
│   │   ├── AdvancedDataGridControl.xaml
│   │   └── AdvancedDataGridControl.xaml.cs
│   ├── ViewModels/                  # 🧠 Business Logic (MVVM)
│   │   └── AdvancedDataGridViewModel.cs
│   ├── Models/                      # 📊 Dátové modely
│   │   ├── ColumnDefinition.cs
│   │   ├── DataGridRow.cs
│   │   ├── DataGridCell.cs
│   │   ├── ValidationRule.cs
│   │   ├── ValidationResult.cs
│   │   └── ThrottlingConfig.cs
│   ├── Services/                    # ⚙️ Business Services
│   │   ├── Interfaces/
│   │   └── Implementation/
│   │       ├── DataService.cs       # 📊 Data management
│   │       ├── ValidationService.cs # ✅ Validation logic
│   │       ├── ClipboardService.cs  # 📋 Copy/Paste
│   │       ├── ColumnService.cs     # 📏 Column management
│   │       ├── ExportService.cs     # 📤 Export/Import
│   │       └── NavigationService.cs # 🧭 Keyboard navigation
│   ├── Commands/                    # 🎯 Command Pattern
│   │   ├── RelayCommand.cs
│   │   └── AsyncRelayCommand.cs
│   ├── Converters/                  # 🔄 Data Binding Converters
│   ├── Controls/                    # 🎛️ Custom Controls
│   │   └── EditableTextBlock.cs    # ✏️ In-place editing
│   ├── Events/                      # 📢 Event Args
│   ├── Helpers/                     # 🛠️ Utility classes
│   ├── Collections/                 # 📦 Specialized collections
│   │   └── ObservableRangeCollection.cs
│   └── Configuration/               # ⚙️ DI & Setup
│       ├── DependencyInjectionConfig.cs
│       └── ServiceCollectionExtensions.cs
├── Themes/                          # 🎨 XAML Styles & Resources
│   └── Generic.xaml
└── build/                           # 📦 NuGet packaging
    └── RpaWinUiComponents.targets
```

### 🧩 Clean Architecture Layers

1. **Presentation Layer** - Views, ViewModels, Converters
2. **Application Layer** - Services, Commands, Events  
3. **Domain Layer** - Models, Validation Rules, Business Logic
4. **Infrastructure Layer** - Configuration, DI, Helpers

## 📦 Inštalácia

### Cez NuGet Package Manager

```powershell
Install-Package RpaWinUiComponents
```

### Cez .NET CLI

```bash
dotnet add package RpaWinUiComponents
```

### Cez PackageReference

```xml
<PackageReference Include="RpaWinUiComponents" Version="1.0.0" />
```

## 🚀 Rýchly štart

### 1. Základná konfigurácia

```csharp
// V App.xaml.cs
public partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Konfigurácia dependency injection
        var services = new ServiceCollection();
        services.AddAdvancedWinUiDataGrid();
        var serviceProvider = services.BuildServiceProvider();
        
        // Konfigurácia komponentu
        AdvancedWinUiDataGridControl.Configuration.ConfigureServices(serviceProvider);
        
        // Zapnutie debug loggu (voliteľné)
        AdvancedWinUiDataGridControl.Configuration.SetDebugLogging(true);
    }
}
```

### 2. Pridanie do XAML

```xml
<Window xmlns:controls="using:RpaWinUiComponents.AdvancedWinUiDataGrid">
    <Grid>
        <controls:AdvancedWinUiDataGridControl