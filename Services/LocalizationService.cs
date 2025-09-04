using System.Collections.Generic;
using System.Globalization;

namespace shutdown_timer.Services
{
    public class LocalizationService
    {
        private static LocalizationService? _instance;
        private static readonly object _lock = new object();

        public static LocalizationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LocalizationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, Dictionary<string, string>> _resources;

        private LocalizationService()
        {
            _resources = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new()
                {
                    ["AppTitle"] = "Shutdown Timer",
                    ["Duration"] = "Duration",
                    ["SpecificTime"] = "Specific Time",
                    ["Hours"] = "Hours",
                    ["Minutes"] = "Minutes",
                    ["Seconds"] = "Seconds",
                    ["ShutdownAt"] = "Shutdown at",
                    ["Start"] = "Start",
                    ["Cancel"] = "Cancel",
                    ["Ready"] = "Ready",
                    ["QuickPresets"] = "Quick Presets",
                    ["ShutdownOptions"] = "Shutdown Options",
                    ["Shutdown"] = "Shutdown",
                    ["Restart"] = "Restart",
                    ["Sleep"] = "Sleep",
                    ["Logoff"] = "Logoff",
                    ["ForceShutdown"] = "Force shutdown (unsaved data will be lost)",
                    ["Settings"] = "Settings",
                    ["About"] = "About",
                    ["15Minutes"] = "15 min",
                    ["30Minutes"] = "30 min",
                    ["1Hour"] = "1 hour",
                    ["2Hours"] = "2 hours",
                    ["Midnight"] = "Midnight",
                    ["Morning6AM"] = "6 AM",
                    ["WillShutdownIn"] = "Will shutdown in {0}",
                    ["WillShutdownAt"] = "Will shutdown at {0}",
                    ["Today"] = "Today",
                    ["Tomorrow"] = "Tomorrow",
                    ["HourUnit"] = "hour",
                    ["HoursUnit"] = "hours",
                    ["MinuteUnit"] = "minute",
                    ["MinutesUnit"] = "minutes",
                    ["SecondUnit"] = "second",
                    ["SecondsUnit"] = "seconds",
                    ["ExecutingShutdown"] = "Executing shutdown...",
                    ["ExecutingRestart"] = "Executing restart...",
                    ["ExecutingSleep"] = "Executing sleep...",
                    ["ExecutingLogoff"] = "Executing logoff...",
                    ["Cancelled"] = "Cancelled",
                    ["Error"] = "Error: {0}",
                    ["Save"] = "Save",
                    ["Close"] = "Close",
                    ["Version"] = "Version {0}",
                    ["LanguageSettings"] = "Language Settings",
                    ["SelectLanguage"] = "Select the display language for the application",
                    ["ThemeSettings"] = "Theme Settings",
                    ["SelectTheme"] = "Select the appearance theme for the application",
                    ["SystemDefault"] = "Follow system settings",
                    ["LightTheme"] = "Light theme",
                    ["DarkTheme"] = "Dark theme",
                    ["StartupSettings"] = "Startup Settings",
                    ["StartMinimized"] = "Start minimized",
                    ["RememberLastSettings"] = "Remember last settings",
                    ["NotificationSettings"] = "Notification Settings",
                    ["ShowNotifications"] = "Show notifications when timer starts",
                    ["ShowWarningBeforeShutdown"] = "Show warning before shutdown",
                    ["WarningTiming"] = "Warning timing:",
                    ["SecondsBeforeShutdown"] = "seconds before",
                    ["AdvancedSettings"] = "Advanced Settings",
                    ["ConfirmOnExit"] = "Confirm when exiting app",
                    ["AutoSaveSchedule"] = "Auto-save schedule",
                    ["ShowCountdownInTitle"] = "Show countdown in title bar",

                    ["AppDescription"] = "Shutdown Timer is an application that allows you to automatically shutdown, restart, sleep, or log off your computer at a specified time or duration.\n\nIt helps automate daily tasks with simple operations, contributing to energy savings and improved work efficiency.",
                    ["MainFeatures"] = "Main Features",
                    ["Feature1"] = "• Timer setting with duration (hours, minutes, seconds)",
                    ["Feature2"] = "• Schedule setting with specific time",
                    ["Feature3"] = "• Quick presets (15 min, 30 min, 1 hour, etc.)",
                    ["Feature4"] = "• Multiple actions (shutdown, restart, sleep, logoff)",
                    ["Feature5"] = "• Force execution option",
                    ["Feature6"] = "• Multi-language support (Japanese, English)",
                    ["Feature7"] = "• Dark/Light theme support",
                    ["Feature8"] = "• Automatic schedule save/restore",
                    ["SystemRequirements"] = "System Requirements",
                    ["Requirement1"] = "• Windows 10 version 1809 or later",
                    ["Requirement2"] = "• Windows 11 (recommended)",
                    ["Requirement3"] = "• .NET 8.0 Runtime",
                    ["Requirement4"] = "• Administrator privileges (for shutdown execution)",
                    ["UsageTips"] = "Usage Tips",
                    ["Tip1"] = "• Use preset buttons to quickly set common durations",
                    ["Tip2"] = "• Force shutdown may cause loss of unsaved data",
                    ["Tip3"] = "• Schedules are automatically saved and restored on app restart",
                    ["Tip4"] = "• Customize language and theme in the settings screen",
                    ["DeveloperInfo"] = "Developer Information",
                    ["DevelopedBy"] = "Developed by: ",
                    ["DeveloperName"] = "yashikota",
                    ["Framework"] = "Framework: WinUI 3 (.NET 8)",
                    ["License"] = "License: MIT License",
                    ["Links"] = "Links",
                    ["GitHubRepo"] = "GitHub Repository",
                    ["BugReports"] = "Bug Reports & Feature Requests",
                    ["Documentation"] = "Online Documentation",
                    ["Copyright"] = "© 2025 yashikota. All rights reserved.",
                    ["Overview"] = "Overview",
                    ["SetTime"] = "Set time",
                    ["WillExecuteIn"] = "Will execute in {0}",
                    ["WillExecuteAt"] = "Will execute at {0}"
                },
                ["ja"] = new()
                {
                    ["AppTitle"] = "シャットダウンタイマー",
                    ["Duration"] = "時間指定",
                    ["SpecificTime"] = "時刻指定",
                    ["Hours"] = "時間",
                    ["Minutes"] = "分",
                    ["Seconds"] = "秒",
                    ["ShutdownAt"] = "シャットダウン時刻",
                    ["Start"] = "開始",
                    ["Cancel"] = "キャンセル",
                    ["Ready"] = "準備完了",
                    ["QuickPresets"] = "クイックプリセット",
                    ["ShutdownOptions"] = "シャットダウンオプション",
                    ["Shutdown"] = "シャットダウン",
                    ["Restart"] = "再起動",
                    ["Sleep"] = "スリープ",
                    ["Logoff"] = "ログオフ",
                    ["ForceShutdown"] = "強制シャットダウン（保存されていないデータは失われます）",
                    ["Settings"] = "設定",
                    ["About"] = "情報",
                    ["15Minutes"] = "15分",
                    ["30Minutes"] = "30分",
                    ["1Hour"] = "1時間",
                    ["2Hours"] = "2時間",
                    ["Midnight"] = "深夜0時",
                    ["Morning6AM"] = "朝6時",
                    ["WillShutdownIn"] = "{0}後にシャットダウンします",
                    ["WillShutdownAt"] = "{0} にシャットダウンします",
                    ["ExecutingShutdown"] = "シャットダウンを実行します",
                    ["ExecutingRestart"] = "再起動を実行します",
                    ["ExecutingSleep"] = "スリープを実行します",
                    ["ExecutingLogoff"] = "ログオフを実行します",
                    ["Cancelled"] = "キャンセルしました",
                    ["Error"] = "エラー: {0}",
                    ["Save"] = "保存",
                    ["Close"] = "閉じる",
                    ["Version"] = "バージョン {0}",
                    ["LanguageSettings"] = "言語設定",
                    ["SelectLanguage"] = "アプリケーションの表示言語を選択してください",
                    ["ThemeSettings"] = "テーマ設定",
                    ["SelectTheme"] = "アプリケーションの外観テーマを選択してください",
                    ["SystemDefault"] = "システム設定に従う",
                    ["LightTheme"] = "ライトテーマ",
                    ["DarkTheme"] = "ダークテーマ",
                    ["StartupSettings"] = "起動設定",
                    ["StartMinimized"] = "最小化状態で起動",
                    ["RememberLastSettings"] = "前回の設定を記憶する",
                    ["NotificationSettings"] = "通知設定",
                    ["ShowNotifications"] = "タイマー開始時に通知を表示",
                    ["ShowWarningBeforeShutdown"] = "シャットダウン前に警告を表示",
                    ["WarningTiming"] = "警告表示タイミング:",
                    ["SecondsBeforeShutdown"] = "秒前",
                    ["AdvancedSettings"] = "詳細設定",
                    ["ConfirmOnExit"] = "アプリ終了時に確認する",
                    ["AutoSaveSchedule"] = "スケジュールを自動保存",
                    ["ShowCountdownInTitle"] = "タイトルバーにカウントダウンを表示",

                    ["AppDescription"] = "シャットダウンタイマーは、指定した時間または時刻にコンピューターを自動的にシャットダウン、再起動、スリープ、またはログオフできるアプリケーションです。\n\n簡単な操作で日常的なタスクを自動化し、エネルギーの節約や作業効率の向上に役立ちます。",
                    ["MainFeatures"] = "主な機能",
                    ["Feature1"] = "• 時間指定（時間・分・秒）でのタイマー設定",
                    ["Feature2"] = "• 特定時刻でのスケジュール設定",
                    ["Feature3"] = "• クイックプリセット（15分、30分、1時間など）",
                    ["Feature4"] = "• 複数のアクション（シャットダウン、再起動、スリープ、ログオフ）",
                    ["Feature5"] = "• 強制実行オプション",
                    ["Feature6"] = "• 多言語対応（日本語・英語）",
                    ["Feature7"] = "• ダーク・ライトテーマ対応",
                    ["Feature8"] = "• スケジュール自動保存・復元",
                    ["SystemRequirements"] = "システム要件",
                    ["Requirement1"] = "• Windows 10 バージョン 1809 以降",
                    ["Requirement2"] = "• Windows 11（推奨）",
                    ["Requirement3"] = "• .NET 8.0 ランタイム",
                    ["Requirement4"] = "• 管理者権限（シャットダウン実行時）",
                    ["UsageTips"] = "使い方のヒント",
                    ["Tip1"] = "• プリセットボタンで素早く一般的な時間を設定",
                    ["Tip2"] = "• 強制シャットダウンは保存されていないデータが失われる可能性があります",
                    ["Tip3"] = "• スケジュールは自動的に保存され、アプリ再起動時に復元されます",
                    ["Tip4"] = "• 設定画面で言語やテーマをカスタマイズできます",
                    ["DeveloperInfo"] = "開発者情報",
                    ["DevelopedBy"] = "開発: ",
                    ["DeveloperName"] = "yashikota",
                    ["Framework"] = "フレームワーク: WinUI 3 (.NET 8)",
                    ["License"] = "ライセンス: MIT License",
                    ["Links"] = "リンク",
                    ["GitHubRepo"] = "GitHub リポジトリ",
                    ["BugReports"] = "バグ報告・機能要望",
                    ["Documentation"] = "オンラインドキュメント",
                    ["Copyright"] = "© 2025 yashikota. All rights reserved.",
                    ["Overview"] = "概要",
                    ["SetTime"] = "時間を設定してください",
                    ["WillExecuteAt"] = "{0} にシャットダウンします",
                    ["Today"] = "今日",
                    ["Tomorrow"] = "明日",
                    ["HourUnit"] = "時間",
                    ["HoursUnit"] = "時間",
                    ["MinuteUnit"] = "分",
                    ["MinutesUnit"] = "分",
                    ["SecondUnit"] = "秒",
                    ["SecondsUnit"] = "秒"
                }
            };

            CurrentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (!_resources.ContainsKey(CurrentLanguage))
            {
                CurrentLanguage = "en";
            }
        }

        public string GetString(string key)
        {
            if (_resources.TryGetValue(CurrentLanguage, out var value) && value.TryGetValue(key, out var getString))
            {
                return getString;
            }

            // Fallback to English
            return _resources["en"].GetValueOrDefault(key, key);
        }

        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            return string.Format(format, args);
        }

        public void SetLanguage(string language)
        {
            if (_resources.ContainsKey(language))
            {
                CurrentLanguage = language;
            }
        }

        public string CurrentLanguage { get; private set; }

        public IEnumerable<string> AvailableLanguages => _resources.Keys;
    }
}
