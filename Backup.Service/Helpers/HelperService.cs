using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Backup.Service.Helpers.Interfaces;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NLog;

namespace Backup.Service.Helpers
{
    public class HelperService : IHelperService
    {
        private static ILogger _logger;
        public HelperService()
        {
            SetUpNLog();
        }

        private string FolderToZipLocation
        {
            get
            {
                var folderToZip = Convert.ToString(ConfigurationManager.AppSettings["FolderToZipLocation"]);
                if (string.IsNullOrWhiteSpace(folderToZip))
                {
                    throw new CustomConfigurationException(CustomConstants.NoFolderToZipLocationSettings);
                }
                return folderToZip;
            }
        }

        private string FolderFromZipLocation
        {
            get
            {
                var folderToZip = Convert.ToString(ConfigurationManager.AppSettings["FolderFromZipLocation"]);
                if (string.IsNullOrWhiteSpace(folderToZip))
                {
                    throw new CustomConfigurationException(CustomConstants.NoFolderFromZipLocationSettings);
                }
                return folderToZip;
            }
        }

        private string StorageConnectionString
        {
            get
            {
                var folderToZip = Convert.ToString(ConfigurationManager.ConnectionStrings["StorageConnectionString"]);
                if (string.IsNullOrWhiteSpace(folderToZip))
                {
                    throw new CustomConfigurationException(CustomConstants.NoStorageConnectionStringSettings);
                }
                return folderToZip;
            }
        }

        public async Task PerformService(string schedule)
        {
            try
            {
                _logger.Info($"{DateTime.Now}: The PerformService() is called with {schedule} schedule");
                var fileName = $"{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}_{schedule}_backup.zip";
                var path = $"{FolderToZipLocation}\\{fileName}";
                if (!string.IsNullOrWhiteSpace(schedule))
                {
                    ZipTheFolder(path);
                    await UploadToAzureBlobStorage(schedule, path, fileName);
                    _logger.Info($"{DateTime.Now}: The PerformService() is finished with {schedule} schedule");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{DateTime.Now}: Exception is occured at PerformService(): {ex.Message}");
                throw new CustomConfigurationException(ex.Message);
            }
        }

        private async Task UploadToAzureBlobStorage(string schedule, string path, string fileName)
        {
            try
            {
                if (CloudStorageAccount.TryParse(StorageConnectionString, out CloudStorageAccount cloudStorageAccount))
                {
                    _logger.Info($"{DateTime.Now}: The UploadToAzureBlobStorage() is called with {schedule} schedule");
                    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(schedule);
                    var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                    await cloudBlockBlob.UploadFromFileAsync(path);
                    _logger.Info($"{DateTime.Now}: The file is been uplaoded to the blob with {schedule} schedule");
                }
                else
                {
                    _logger.Error($"{DateTime.Now}: {CustomConstants.NoStorageConnectionStringSettings}");
                    throw new CustomConfigurationException(CustomConstants.NoStorageConnectionStringSettings);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{DateTime.Now}: Exception is occured at UploadToAzureBlobStorage(): {ex.Message}");
                throw new CustomConfigurationException($"Error when uploading to blob: {ex.Message}");
            }
        }

        private void ZipTheFolder(string path)
        {
            try
            {
                _logger.Info($"{DateTime.Now}: The ZipTheFolder() is called ");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.Info($"{DateTime.Now}: The file with the same name exists, thus deleted the same");
                }
                ZipFile.CreateFromDirectory(FolderFromZipLocation, path);
                _logger.Info($"{DateTime.Now}: The Zip file is been created");
            }
            catch (Exception ex)
            {
                _logger.Error($"{DateTime.Now}: Exception is occured at ZipTheFolder(): {ex.Message}");
                throw new CustomConfigurationException($"Error when zip: {ex.Message}");
            }

        }

        private void SetUpNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "backupclientlogfile.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            _logger = LogManager.GetCurrentClassLogger();
        }
    }
}
