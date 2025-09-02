using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using shutdown_timer.Models;

namespace shutdown_timer.Services
{
    public class ShutdownService
    {
        private const string SCHEDULE_FILE = "shutdown_schedule.json";

        public async Task<bool> ScheduleShutdownAsync(ShutdownConfig config)
        {
            try
            {
                var secondsUntilAction = (int)(config.TargetTime - DateTime.Now).TotalSeconds;

                if (secondsUntilAction <= 0)
                {
                    throw new InvalidOperationException("The specified time has already passed");
                }

                // Sleep requires special handling
                if (config.ActionType == ActionType.Sleep)
                {
                    // For sleep, we schedule it using Task.Delay and then execute
                    await Task.Delay(TimeSpan.FromSeconds(secondsUntilAction));
                    return await ExecuteSleepAsync();
                }

                var arguments = GetShutdownArguments(config.ActionType, secondsUntilAction, config.ForceAction);

                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                await process!.WaitForExitAsync();

                if (process!.ExitCode == 0)
                {
                    await SaveScheduleAsync(config);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Shutdown command failed (Exit Code: {process!.ExitCode})");
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CancelShutdownAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/a",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                await process!.WaitForExitAsync();

                // Remove schedule file
                if (File.Exists(SCHEDULE_FILE))
                {
                    File.Delete(SCHEDULE_FILE);
                }

                return process!.ExitCode == 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ShutdownConfig?> LoadScheduleAsync()
        {
            try
            {
                if (!File.Exists(SCHEDULE_FILE))
                    return null;

                var jsonString = await File.ReadAllTextAsync(SCHEDULE_FILE);
                var config = JsonSerializer.Deserialize<ShutdownConfig>(jsonString);

                // Check if the schedule is still valid
                if (config?.TargetTime <= DateTime.Now)
                {
                    File.Delete(SCHEDULE_FILE);
                    return null;
                }

                return config;
            }
            catch
            {
                // If there's any error loading the schedule, delete the file and return null
                if (File.Exists(SCHEDULE_FILE))
                {
                    File.Delete(SCHEDULE_FILE);
                }
                return null;
            }
        }

        private async Task SaveScheduleAsync(ShutdownConfig config)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(SCHEDULE_FILE, jsonString);
            }
            catch
            {
                // Ignore save errors - not critical
            }
        }

        private string GetShutdownArguments(ActionType actionType, int seconds, bool force)
        {
            var forceFlag = force ? "/f " : "";

            return actionType switch
            {
                ActionType.Shutdown => $"/s {forceFlag}/t {seconds}",
                ActionType.Restart => $"/r {forceFlag}/t {seconds}",
                ActionType.Logoff => $"/l {forceFlag}/t {seconds}",
                ActionType.Sleep => "", // Sleep is handled separately
                _ => throw new ArgumentException("Invalid action type")
            };
        }

        private async Task<bool> ExecuteSleepAsync()
        {
            try
            {
                // For sleep, we use a different approach
                var psi = new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = "powrprof.dll,SetSuspendState 0,1,0",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                await process!.WaitForExitAsync();
                return process!.ExitCode == 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
