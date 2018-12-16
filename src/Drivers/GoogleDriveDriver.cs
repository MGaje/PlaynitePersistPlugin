using Google.Apis.Auth.OAuth2;
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

namespace PlaynitePersistPlugin.Drivers
{ 
    public class GoogleDriveDriver
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "PlaynitePersistPlugin";
        
        private readonly ILogger logger;
        private DriveService driveService;

        public GoogleDriveDriver(ILogger logger)
        {
            this.logger = logger;
            this.Load();
        }

        public void UploadGamesArchive()
        {
            this.logger.Debug($"Making Google Drive File List Request...");
            FilesResource.ListRequest listRequest = this.driveService.Files.List();
            listRequest.Fields = "files(id, name)";
            List<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files.ToList();

            this.logger.Debug($"Made request. Searching files for Playnite Games Archive...");
            if (files != null && files.Count > 0)
            {
                var playniteGamesArchive = files.FirstOrDefault(x => String.Equals(x.Name, "playnite-games-data.zip", StringComparison.OrdinalIgnoreCase));
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

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "playnite-games-data.zip"
            };

            this.logger.Debug($"Making upload request...");
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(Path.Combine("Archives", "playnite-games-data.zip"), System.IO.FileMode.Open))
            {
                request = this.driveService.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";
                request.Upload();
            }

            var file = request.ResponseBody;
            this.logger.Debug($"Playnite Games Archive uploaded. The file id is {file.Id}");
        }

        private void Load()
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
    }
}
