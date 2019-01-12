using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace PlaynitePersistPlugin.Providers
{
    /// <summary>
    /// Cloud provider for Google Drive.
    /// </summary>
    public class GoogleDriveProvider : ICloudProvider
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "PlaynitePersistPlugin";

        private readonly ILogger logger;
        private DriveService driveService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        public GoogleDriveProvider(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Connect to Google Drive.
        /// </summary>
        public void Connect()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                ).Result;
                this.logger.Debug($"Credential file saved to: {credPath}");
            }

            this.driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        /// <summary>
        /// Sync games data to Google Drive.
        /// </summary>
        /// <param name="archiveFile">The archive of games data.</param>
        public void SyncTo(string path, string archiveFile)
        {
            string archiveFileAndPath = Path.Combine(path, archiveFile);

            this.logger.Debug($"Making Google Drive File List Request...");
            List<GoogleFile> files = this.FilesRequest();
            this.logger.Debug($"Made request. Searching files for Playnite Games Archive...");
            if (files != null && files.Count > 0)
            {
                var playniteGamesArchive = files.FirstOrDefault(x => String.Equals(x.Name, archiveFile, StringComparison.OrdinalIgnoreCase));
                if (playniteGamesArchive != null)
                {
                    this.logger.Debug($"Playnite Games Archive found. Deleting it so we can replace it. Id = {playniteGamesArchive.Id}");
                    this.driveService.Files.Delete(playniteGamesArchive.Id).Execute();
                }
                else
                {
                    this.logger.Debug("No Playnite Games Archive found on Google Drive.");
                }
            }

            var fileMetadata = new GoogleFile()
            {
                Name = archiveFile
            };

            this.logger.Debug($"Making upload request...");
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(archiveFileAndPath, System.IO.FileMode.Open))
            {
                request = this.driveService.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";
                request.Upload();
            }

            var file = request.ResponseBody;
            this.logger.Debug($"Playnite Games Archive uploaded. The file id is {file.Id}");
        }

        /// <summary>
        /// Get games data from Google Drive.
        /// </summary>
        /// <param name="archiveFile">The archive of games data.</param>
        /// <param name="downloadPath">The location to download the games archive to.</param>
        public void SyncFrom(string archiveFile, string downloadPath)
        {
            List<GoogleFile> files = this.FilesRequest();
            if (files != null && files.Count > 0)
            {
                var playniteGamesArchive = files.FirstOrDefault(x => String.Equals(x.Name, archiveFile, StringComparison.OrdinalIgnoreCase));
                if (playniteGamesArchive == null)
                {
                    this.logger.Debug("No Playnite Games Archive found on Google Drive.");
                    return;
                }

                this.logger.Debug($"Playnite Games Archive found. Downloading to sync.");
                this.DownloadFile(this.driveService.Files.Get(playniteGamesArchive.Id), downloadPath);
            }
        }

        // --
        // Utility Methods.
        // --

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<GoogleFile> FilesRequest()
        {
            FilesResource.ListRequest listRequest = this.driveService.Files.List();
            listRequest.Fields = "files(id, name)";
            List<GoogleFile> files = listRequest.Execute().Files.ToList();

            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private void DownloadFile(FilesResource.GetRequest request, string path)
        {
            MemoryStream downloadStream = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                    {
                        this.logger.Debug($"Downloading games data archive (Downloaded {progress.BytesDownloaded} bytes.");
                        break;
                    }

                    case DownloadStatus.Completed:
                    {
                        this.logger.Debug($"Finished downloading games data archive.");

                        System.IO.File.WriteAllBytes(path, downloadStream.ToArray());

                        // TODO: Add notification.
                        break;
                    }

                    case DownloadStatus.Failed:
                    {
                        this.logger.Error($"Could not download games data archive from Google Drive. Exception: {progress.Exception}");
                        // TODO: Add error notification.
                        break;
                    }
                }
            };

            request.Download(downloadStream);
        }
    }
}
