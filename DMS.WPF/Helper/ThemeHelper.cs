
using System;
using System.Linq;
using System.Windows;
using DMS.Infrastructure.Configurations;
using iNKORE.UI.WPF.Modern;
using Microsoft.Win32;

namespace DMS.WPF.Helper;

public static class ThemeHelper
{
    public static void ApplyTheme(string themeName)
    {
        ApplicationTheme theme;

        if (themeName == "跟随系统")
        {
            theme = IsSystemInDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }
        else
        {
            theme = themeName switch
            {
                "浅色" => ApplicationTheme.Light,
                "深色" => ApplicationTheme.Dark,
                _ => IsSystemInDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light
            };
        }

        // Apply theme for iNKORE controls
        ThemeManager.Current.ApplicationTheme = theme;
        
        // Apply theme for HandyControl
        UpdateHandyControlTheme(theme);
    }

    public static void InitializeTheme()
    {
        var settings = ConnectionSettings.Load();
        ApplyTheme(settings.Theme);

        // Listen for system theme changes
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category == UserPreferenceCategory.General && ConnectionSettings.Load().Theme == "跟随系统")
            {
                Application.Current.Dispatcher.Invoke(() => { ApplyTheme("跟随系统"); });
            }
        };
    }

    private static void UpdateHandyControlTheme(ApplicationTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;

        // Find and remove the existing HandyControl skin dictionary
        var existingSkin = dictionaries.FirstOrDefault(d =>
            d.Source != null && d.Source.OriginalString.Contains("HandyControl;component/Themes/Skin"));
        if (existingSkin != null)
        {
            dictionaries.Remove(existingSkin);
        }

        // Determine the new skin URI
        string skinUri = theme == ApplicationTheme.Dark
            ? "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml"
            : "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml";

        // AddAsync the new skin dictionary
        dictionaries.Add(new ResourceDictionary { Source = new Uri(skinUri, UriKind.Absolute) });
        
        // To force refresh of dynamic resources, remove and re-add the main theme dictionary
        var existingTheme = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("HandyControl;component/Themes/Theme.xaml"));
        if (existingTheme != null)
        { 
            dictionaries.Remove(existingTheme);
            dictionaries.Add(existingTheme);
        }
    }

    private static bool IsSystemInDarkTheme()
    {
        try
        {
            const string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";
            var value = Registry.GetValue(keyName, valueName, 1);
            return value is 0;
        }
        catch
        {
            // Default to light theme if registry access fails
            return false;
        }
    }
}
