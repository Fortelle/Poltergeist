# Poltergeist
[![Nuget](https://img.shields.io/nuget/v/Poltergeist?style=flat)](https://www.nuget.org/packages/Poltergeist)
[![LICENSE](https://img.shields.io/github/license/fortelle/poltergeist?style=flat)](https://github.com/fortelle/poltergeist/blob/master/LICENSE.txt)

A .NET framework for desktop automation.
![demo](https://github.com/Fortelle/Poltergeist/assets/38492315/8da6043a-1bdd-4052-8d76-a9dcdf50d424)

## Getting Started
### Step 1: Create a new project
Create a new WinUI 3 Application project in Visual Studio.
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
``` xaml
<Application
    x:Class="App1.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    >
</Application>
```
 to
``` xaml
<poltergeist:PoltergeistApplication
    x:Class="App1.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:poltergeist="using:Poltergeist"
    >
</poltergeist:PoltergeistApplication>
```

- Open the `app.xaml.cs` file, change
``` cs
public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private Window m_window;
}
```
 to
``` cs
public partial class App : PoltergeistApplication
{
    public App()
    {
        this.InitializeComponent();
    }
}
```
### Step 4: Done
You can build and run the app now.
