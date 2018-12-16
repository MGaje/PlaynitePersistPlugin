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

        public GoogleDriveDriver(ILogger logger)
        {
            this.logger = logger;
        }

        public void Load()
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

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            this.logger.Debug($"Files: ");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    this.logger.Debug($"{file.Name} - {file.Id}");
                }
            }
            else
            {
                this.logger.Debug($"No files found!");
            }
        }
    }
}
