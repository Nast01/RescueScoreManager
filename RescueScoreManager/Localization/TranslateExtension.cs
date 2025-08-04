using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using RescueScoreManager.Helpers;

namespace RescueScoreManager.Localization;

/// <summary>
/// Markup extension for XAML translation bindings
/// </summary>
public class TranslateExtension : Binding
{
    public TranslateExtension(string key) : base($"[{key}]")
    {
        Source = new TranslationProxy();
        Mode = BindingMode.OneWay;
    }
}
