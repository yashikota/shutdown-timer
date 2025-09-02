using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using shutdown_timer.Services;
using shutdown_timer.Models;
using System.Reflection;

namespace shutdown_timer.Dialogs
{
    public sealed partial class AboutDialog : ContentDialog
    {
        private readonly LocalizationService _localizationService;
        private readonly SettingsService _settingsService;

        public AboutDialog()
        {
            this.InitializeComponent();
            _localizationService = LocalizationService.Instance;
            _settingsService = SettingsService.Instance;

            ApplyTheme();
            InitializeUI();
        }

        private void ApplyTheme()
        {
            var settings = _settingsService.CurrentSettings;
            var elementTheme = settings.Theme switch
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
            // Get version from assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";

            // Localize and set content
            Title = _localizationService.GetString("About");
            CloseButtonText = _localizationService.GetString("Close");

            AppNameText.Text = _localizationService.GetString("AppTitle");
            VersionText.Text = _localizationService.GetString("Version", versionString);

            // Set localized content
            SystemRequirementsHeader.Text = _localizationService.GetString("SystemRequirements");
            Requirement1Text.Text = _localizationService.GetString("Requirement1");
            Requirement2Text.Text = _localizationService.GetString("Requirement2");
            Requirement3Text.Text = _localizationService.GetString("Requirement3");
            Requirement4Text.Text = _localizationService.GetString("Requirement4");
            LinksHeader.Text = _localizationService.GetString("Links");
            GitHubLink.Content = _localizationService.GetString("GitHubRepo");
            IssuesLink.Content = _localizationService.GetString("BugReports");
            CopyrightText.Text = _localizationService.GetString("Copyright");
        }
    }
}
