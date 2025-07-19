
using System;
using System.Linq;
using System.Windows;
using iNKORE.UI.WPF.Modern; // 导入 iNKORE UI 库，用于现代WPF控件和主题
using Microsoft.Win32; // 导入用于访问Windows注册表的类

namespace DMS.WPF.Helper;

/// <summary>
/// 主题管理帮助类，用于应用和初始化应用程序的主题。
/// </summary>
public static class ThemeHelper
{
    /// <summary>
    /// 应用指定的主题。
    /// </summary>
    /// <param name="themeName">要应用的主题名称（"跟随系统", "浅色", "深色"）。</param>
    public static void ApplyTheme(string themeName)
    {
        ApplicationTheme theme; // 定义一个变量来存储最终的主题

        // 判断是否设置为"跟随系统"
        if (themeName == "跟随系统")
        {
            // 如果是，则根据当前系统主题设置应用主题
            theme = IsSystemInDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }
        else
        {
            // 否则，根据传入的主题名称进行匹配
            theme = themeName switch
            {
                "浅色" => ApplicationTheme.Light, // 设置为浅色主题
                "深色" => ApplicationTheme.Dark,   // 设置为深色主题
                _ => IsSystemInDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light // 默认情况，跟随系统
            };
        }

        // 为 iNKORE 控件应用主题
        ThemeManager.Current.ApplicationTheme = theme;
        
        // 为 HandyControl 控件应用主题
        UpdateHandyControlTheme(theme);
    }

    /// <summary>
    /// 初始化主题，在应用启动时调用。
    /// </summary>
    public static void InitializeTheme()
    {
        // 从应用设置中读取保存的主题并应用
        ApplyTheme(App.Current.Settings.Theme);

        // 监听系统主题变化事件
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            // 当用户偏好设置中的"常规"类别发生变化，并且应用主题设置为"跟随系统"时
            if (e.Category == UserPreferenceCategory.General && App.Current.Settings.Theme == "跟随系统")
            {
                // 在UI线程上调用ApplyTheme来更新主题，以响应系统主题的变化
                App.Current.Dispatcher.Invoke(() => { ApplyTheme("跟随系统"); });
            }
        };
    }

    /// <summary>
    /// 更新 HandyControl 库的主题。
    /// </summary>
    /// <param name="theme">要应用的主题 (浅色或深色)。</param>
    private static void UpdateHandyControlTheme(ApplicationTheme theme)
    {
        // 获取当前应用的资源字典集合
        var dictionaries = App.Current.Resources.MergedDictionaries;

        // 查找并移除现有的 HandyControl 皮肤资源字典
        var existingSkin = dictionaries.FirstOrDefault(d =>
            d.Source != null && d.Source.OriginalString.Contains("HandyControl;component/Themes/Skin"));
        if (existingSkin != null)
        {
            dictionaries.Remove(existingSkin);
        }

        // 根据主题确定新的皮肤资源URI
        string skinUri = theme == ApplicationTheme.Dark
            ? "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml" // 深色主题皮肤
            : "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml"; // 浅色主题皮肤

        // 添加新的皮肤资源字典
        dictionaries.Add(new ResourceDictionary { Source = new Uri(skinUri, UriKind.Absolute) });
        
        // 为了强制刷新动态资源，先移除再重新添加主主题字典
        var existingTheme = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("HandyControl;component/Themes/Theme.xaml"));
        if (existingTheme != null)
        { 
            dictionaries.Remove(existingTheme);
            dictionaries.Add(existingTheme);
        }
    }

    /// <summary>
    /// 检查当前Windows系统是否处于深色模式。
    /// </summary>
    /// <returns>如果系统是深色模式，则返回 true；否则返回 false。</returns>
    private static bool IsSystemInDarkTheme()
    {
        try
        {
            // 定义注册表项路径和值名称
            const string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";
            // 读取注册表值，1表示浅色模式，0表示深色模式
            var value = Registry.GetValue(keyName, valueName, 1);
            // 如果值为0，则系统为深色模式
            return value is 0;
        }
        catch
        {
            // 如果访问注册表失败，则默认返回浅色主题
            return false;
        }
    }
}
