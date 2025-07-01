// GlobalUsings.cs - Pridajte do oboch projektov (RpaWinUiComponents a RpaWinUiComponents.Demo)
// Tento súbor vyrieši množstvo namespace problémov

// GlobalUsings.cs - Pridajte do Demo projektu
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.ComponentModel;
global using System.Runtime.CompilerServices;

// WinUI 3 basic namespaces
global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;
global using Microsoft.UI.Xaml.Data;
global using Microsoft.UI.Xaml.Media;
global using Microsoft.UI.Xaml.Input;

// .NET Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// OPRAVA: Aliasy pre riešenie konfliktov
global using WinUIGrid = Microsoft.UI.Xaml.Controls.Grid;
global using WinUIRowDefinition = Microsoft.UI.Xaml.Controls.RowDefinition;
global using WinUIColumnDefinition = Microsoft.UI.Xaml.Controls.ColumnDefinition;
global using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;