# Poltergeist
[![Nuget](https://img.shields.io/nuget/v/Poltergeist?style=flat)](https://www.nuget.org/packages/Poltergeist)
[![LICENSE](https://img.shields.io/github/license/fortelle/poltergeist?style=flat)](https://github.com/fortelle/poltergeist/blob/master/LICENSE.txt)

A .NET framework for desktop automation.
![demo](https://user-images.githubusercontent.com/38492315/190919754-9fd76ff2-7929-44cb-9763-ae78640f8101.gif)

## Getting Started
### Step 1: Create a new project
Create a new WPF Application project in Visual Studio.
### Step 2: Install Poltergeist
- Install via [NuGet](https://www.nuget.org/packages/Poltergeist).
```
Install-Package Poltergeist
```
- Or clone the repository to local.
```
git clone https://github.com/Fortelle/Poltergeist.git
```
### Step 3: Override application
- Open the `App.xaml` file, change
```
<Application
    x:Class="WpfApp1.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:WpfApp1"
    StartupUri="MainWindow.xaml">
</Application>
```
 to
```
<poltergeist:PoltergeistApplication
    x:Class="WpfApp1.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:WpfApp1"
    xmlns:poltergeist="clr-namespace:Poltergeist;assembly=Poltergeist"
    >
</poltergeist:PoltergeistApplication>
```

- Open the `app.xaml.cs` file, change
```
public partial class App : Application
```
 to
```
public partial class App : Poltergeist.PoltergeistApplication
```
### Step 4: Done
You can build and run the app now.
