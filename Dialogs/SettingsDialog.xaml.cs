using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using shutdown_timer.Services;
using shutdown_timer.Models;
using System;

namespace shutdown_timer.Dialogs
{
    public sealed partial class SettingsDialog
    {
        private readonly SettingsService _settingsService;
        private readonly LocalizationService _localizationService;
        private AppSettings _currentSettings = null!;
        private readonly Action<AppSettings>? _onSettingsChanged;
        private readonly bool _isInitializing;

        private AppSettings Settings { get; set; } = null!;

        public SettingsDialog(Action<AppSettings>? onSettingsChanged = null)
        {
            this.InitializeComponent();
            _settingsService = SettingsService.Instance;
            _localizationService = LocalizationService.Instance;
            _onSettingsChanged = onSettingsChanged;

            LoadCurrentSettings();
            ApplyTheme();
            InitializeUi();

            // Enable event handling after initialization
            _isInitializing = false;

            // Settings are auto-saved, no need for primary button
        }

        private void LoadCurrentSettings()
        {
            _currentSettings = _settingsService.CurrentSettings;
            Settings = _currentSettings.Clone();
        }

        private void ApplyTheme()
        {
            var themeToApply = Settings.Theme;
            var elementTheme = themeToApply switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            this.RequestedTheme = elementTheme;
        }

        private void InitializeUi()
        {
            // Set language
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag.ToString() != _currentSettings.Language) continue;
                LanguageComboBox.SelectedItem = item;
                break;
            }

            // Set theme
            foreach (RadioButton radio in ThemeRadioButtons.Items)
            {
                if (radio.Tag.ToString() != _currentSettings.Theme.ToString()) continue;
                radio.IsChecked = true;
                break;
            }



            // Localize UI
            LocalizeUi();
        }

        private void LocalizeUi()
        {
            Title = _localizationService.GetString("Settings");
            CloseButtonText = _localizationService.GetString("Close");

            // Localize all UI elements
            LanguageSettingsHeader.Text = _localizationService.GetString("LanguageSettings");
            ThemeSettingsHeader.Text = _localizationService.GetString("ThemeSettings");
            SystemDefaultRadio.Content = _localizationService.GetString("SystemDefault");
            LightThemeRadio.Content = _localizationService.GetString("LightTheme");
            DarkThemeRadio.Content = _localizationService.GetString("DarkTheme");
        }

                private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (LanguageComboBox.SelectedItem is not ComboBoxItem item) return;
            Settings.Language = item.Tag?.ToString() ?? "ja";
            // Auto-save settings
            Settings.AutoSaveSchedule = true;
            await _settingsService.SaveSettingsAsync(Settings);
            _currentSettings = Settings.Clone();
                
            // Apply language change to this dialog immediately
            _localizationService.SetLanguage(Settings.Language);
            LocalizeUi();
                
            // Notify parent window of changes
            _onSettingsChanged?.Invoke(Settings);
        }

                private async void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (ThemeRadioButtons.SelectedItem is not RadioButton radio) return;
            if (!Enum.TryParse<AppTheme>(radio.Tag.ToString(), out var theme)) return;
            Settings.Theme = theme;
            // Auto-save settings
            Settings.AutoSaveSchedule = true;
            await _settingsService.SaveSettingsAsync(Settings);
            _currentSettings = Settings.Clone();
                    
            // Apply theme change to this dialog immediately
            ApplyTheme();
                    
            // Notify parent window of changes
            _onSettingsChanged?.Invoke(Settings);
        }
    }
}
