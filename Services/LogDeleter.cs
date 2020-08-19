using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IISLogDeleter.Models;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IISLogDeleter.Services
{
    public class LogDeleter : BackgroundService
    {
        readonly ILogger<LogDeleter> _logger;
        AppSettings _appSettings;

        public LogDeleter(ILogger<LogDeleter> logger,
                          IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            appSettings.OnChange(OnAppSettingsUpdated);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ExecuteIteration(stoppingToken);
                await Task.Delay(_appSettings.DeletionInterval, stoppingToken);
            }
            stoppingToken.ThrowIfCancellationRequested();
        }

        void ExecuteIteration(CancellationToken ct)
        {
            string folder = _appSettings.LogsFolder;
            var maxLastWriteDate = DateTimeOffset.UtcNow - _appSettings.DeleteFilesOlderThan;
            _logger.LogInformation("Starting files deletion in folder [{folder}] older than {maxDate}.", folder, maxLastWriteDate.ToLocalTime());
            int filesDeleted = DeleteFilesRecursive(folder, ref maxLastWriteDate, ct);
            _logger.LogInformation("{count} files were deleted.", filesDeleted);
        }

        int DeleteFilesRecursive(string folderPath, ref DateTimeOffset maxLastWriteDate, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return 0;

            if (!Directory.Exists(folderPath))
            {
                _logger.LogError("Directory does not exist: [{dir}].", folderPath);
                return 0;
            }

            var deletedFiles = 0;
            var innerDirs = Directory.GetDirectories(folderPath);
            foreach (string innerDir in innerDirs)
                deletedFiles += DeleteFilesRecursive(innerDir, ref maxLastWriteDate, ct);

            var files = Directory.EnumerateFiles(folderPath);
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < maxLastWriteDate)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogTrace("Deleted file [{file}].", file);
                        deletedFiles++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Exception when trying to delete a file [{path}].", file);
                    }
                }
            }
            return deletedFiles;
        }

        void OnAppSettingsUpdated(AppSettings updatedAppSettings)
        {
            _appSettings = updatedAppSettings;
            _logger.LogInformation("App settings updated.");
        }
    }
}