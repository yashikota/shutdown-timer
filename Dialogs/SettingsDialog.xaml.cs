using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using shutdown_timer.Services;
using shutdown_timer.Models;
using System;

namespace shutdown_timer.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsService _settingsService;
        private readonly LocalizationService _localizationService;
        private AppSettings _currentSettings;

        public AppSettings Settings { get; private set; }

                public SettingsDialog()
        {
            this.InitializeComponent();
            _settingsService = SettingsService.Instance;
            _localizationService = LocalizationService.Instance;

            LoadCurrentSettings();
            ApplyTheme();
            InitializeUI();

            // Set up event handlers
            this.PrimaryButtonClick += SettingsDialog_PrimaryButtonClick;
        }

        private void LoadCurrentSettings()
        {
            _currentSettings = _settingsService.LoadSettings();
            Settings = _currentSettings.Clone();
        }

        private void ApplyTheme()
        {
            var elementTheme = _currentSettings.Theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                AppTheme.Default => ElementTheme.Default,
                _ => ElementTheme.Default
            };

            this.RequestedTheme = elementTheme;
        }

        private void InitializeUI()
        {
            // Set language
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag.ToString() == _currentSettings.Language)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }

            // Set theme
            foreach (RadioButton radio in ThemeRadioButtons.Items)
            {
                if (radio.Tag.ToString() == _currentSettings.Theme.ToString())
                {
                    radio.IsChecked = true;
                    break;
                }
            }



            // Localize UI
            LocalizeUI();
        }

        private void LocalizeUI()
        {
            Title = _localizationService.GetString("Settings");
            PrimaryButtonText = _localizationService.GetString("Save");
            CloseButtonText = _localizationService.GetString("Cancel");

            // Localize all UI elements
            LanguageSettingsHeader.Text = _localizationService.GetString("LanguageSettings");
            ThemeSettingsHeader.Text = _localizationService.GetString("ThemeSettings");
            SystemDefaultRadio.Content = _localizationService.GetString("SystemDefault");
            LightThemeRadio.Content = _localizationService.GetString("LightTheme");
            DarkThemeRadio.Content = _localizationService.GetString("DarkTheme");
            ResetSettingsHeader.Text = _localizationService.GetString("ResetSettings");
            ResetSettingsButton.Content = _localizationService.GetString("ResetAllSettings");
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item)
            {
                Settings.Language = item.Tag.ToString();
            }
        }

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeRadioButtons.SelectedItem is RadioButton radio)
            {
                if (Enum.TryParse<AppTheme>(radio.Tag.ToString(), out var theme))
                {
                    Settings.Theme = theme;
                }
            }
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings = new AppSettings();
            InitializeUI();
        }

        private void SettingsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
                        // Auto-save is always enabled
            Settings.AutoSaveSchedule = true;

            // Save settings
            _settingsService.SaveSettings(Settings);
        }
    }
}
