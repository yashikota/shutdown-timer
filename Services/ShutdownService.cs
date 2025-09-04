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
            var secondsUntilAction = (int)(config.TargetTime - DateTime.Now).TotalSeconds;

            // If time has already passed (negative), throw error
            if (secondsUntilAction < 0)
            {
                throw new InvalidOperationException("The specified time has already passed");
            }

            // If 0 seconds, execute immediately (set to 1 second minimum for shutdown command)
            if (secondsUntilAction == 0)
            {
                secondsUntilAction = 1;
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

            switch (process.ExitCode)
            {
                case 0:
                    await SaveScheduleAsync(config);
                    return true;
                case 1190:
                {
                    // Shutdown already scheduled - try to cancel first, then reschedule
                    await CancelShutdownAsync();

                    // Retry scheduling after cancellation
                    var retryProcess = Process.Start(psi);
                    await retryProcess!.WaitForExitAsync();

                    if (retryProcess.ExitCode == 0)
                    {
                        await SaveScheduleAsync(config);
                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Shutdown command failed after retry (Exit Code: {retryProcess.ExitCode})");
                    }
                }
                default:
                    throw new InvalidOperationException($"Shutdown command failed (Exit Code: {process.ExitCode})");
            }
        }

        public static async Task<bool> CancelShutdownAsync()
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

                // Always remove schedule file regardless of shutdown command result
                // This ensures cleanup even if no shutdown was scheduled
                if (File.Exists(SCHEDULE_FILE))
                {
                    File.Delete(SCHEDULE_FILE);
                }

                // Return true if shutdown was cancelled successfully OR if no shutdown was scheduled (exit code 1116)
                return process.ExitCode is 0 or 1116;
            }
            catch
            {
                // If cancel fails, still try to clean up the schedule file
                try
                {
                    if (File.Exists(SCHEDULE_FILE))
                    {
                        File.Delete(SCHEDULE_FILE);
                    }
                }
                catch
                {
                    // Ignore file deletion errors
                }
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
                _ => throw new ArgumentException("Invalid action type")
            };
        }


    }
}
