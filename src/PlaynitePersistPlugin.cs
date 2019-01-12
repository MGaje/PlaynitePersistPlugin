using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlaynitePersistPlugin.Providers;
using PlaynitePersistPlugin.Archives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;

namespace PlaynitePersistPlugin
{
    public class PlaynitePersistPlugin : IGenericPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private ICloudProvider testProvider;
        private const string PersistPluginNotificationId = "persistSuccess";
        private const string PersistPluginErrorNotificationId = "persistError";
        private const string ArchivePath = "Archives";
        private const string ArchiveFile = "playnite-games-data.zip";

        public Guid Id { get; } = Guid.Parse("BE1C544D-8958-4448-B197-12F3393E0728");

        public PlaynitePersistPlugin(IPlayniteAPI api)
        {
            this.api = api;
        }

        public void Dispose()
        {
            // Empty.
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return null;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return null;
        }

        public IEnumerable<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                //new ExtensionFunction(
                //    "Execute function from PlaynitePersistPlugin",
                //    () => { }
                //)
            };
        }

        public void OnGameInstalled(Game game)
        {
            // Add code to be executed when game is finished installing.
        }

        public void OnGameStarted(Game game)
        {
            // Add code to be executed when game is started running.
        }

        public void OnGameStarting(Game game)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public void OnGameStopped(Game game, long elapsedSeconds)
        {
            try
            {
                string archiveFileAndPath = Path.Combine(ArchivePath, ArchiveFile);

                this.logger.Debug($"Creating games archive to persist...");
                Archive.CreateGamesArchive(ArchivePath, ArchiveFile);
                this.logger.Debug($"Done creating games archive!");

                this.logger.Debug($"Attempting to upload the games archive...");
                this.testProvider.SyncTo(ArchivePath, ArchiveFile);
                this.logger.Debug($"Finished uploading games archive");

                this.logger.Debug($"Attempting to delete games archive");
                Archive.DeleteGamesArchive(archiveFileAndPath);
                this.logger.Debug($"Games archive deleted");

                this.api.Notifications.Add(PersistPluginNotificationId, "Games data synced to Google Drive", NotificationType.Info);
            }
            catch (Exception e)
            {
                this.logger.Error($"An error occurred persisting data to Google Drive: {e.Message}");
                this.api.Notifications.Add(PersistPluginErrorNotificationId, "An error occurred persisting games data to Google Drive", NotificationType.Error);
            }
        }

        public void OnGameUninstalled(Game game)
        {
            // Add code to be executed when game is uninstalled.
        }

        public void OnApplicationStarted()
        {
            string archiveFileAndPath = Path.Combine(ArchivePath, ArchiveFile);

            this.logger.Debug("PlaynitePersistPlugin initialization!");
            this.testProvider = new GoogleDriveProvider(this.logger);
            this.testProvider.Connect();

            this.logger.Debug("Attempting to download archive.");
            this.testProvider.SyncFrom(ArchiveFile, archiveFileAndPath);
            this.logger.Debug("Games data archive downloaded from cloud provider.");

            this.logger.Debug("Attempting to extract games data from archive.");
            Archive.ExtractGamesArchive(archiveFileAndPath);
            Archive.DeleteGamesArchive(archiveFileAndPath);
            this.logger.Debug("Extracted games data from archive.");

            this.logger.Debug("Updating games from persist plugin...");
            this.updateGames();

            this.api.Notifications.Add(PersistPluginNotificationId, "Synced games data from Google Drive.", NotificationType.Info);
        }

        private void updateGames()
        {
            string gameLibraryPath = Path.Combine("library", "games");

            string[] filePaths = Directory.GetFiles(gameLibraryPath);
            foreach (var p in filePaths)
            {
                var g = JsonConvert.DeserializeObject<Game>(File.ReadAllText(p));
                this.api.Database.UpdateGame(g);
            }
        }
    }
}
