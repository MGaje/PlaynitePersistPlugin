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

namespace PlaynitePersistPlugin
{
    public class PlaynitePersistPlugin : IGenericPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private ICloudProvider testProvider;
        private const string persistPluginNotificationId = "persistSuccess";
        private const string persistPluginErrorNotificationId = "persistError";
        private const string archiveFile = "playnite-games-data.zip";

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
                new ExtensionFunction(
                    "Execute function from PlaynitePersistPlugin",
                    () => { }
                )
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
                string path = "Archives";
                string archiveFileAndPath = Path.Combine(path, archiveFile);

                this.logger.Debug($"Creating games archive to persist...");
                Archive.CreateGamesArchive(path, archiveFile);
                this.logger.Debug($"Done creating games archive!");

                this.logger.Debug($"Attempting to upload the games archive...");
                this.testProvider.SyncTo(path, archiveFile);
                this.logger.Debug($"Finished uploading games archive");

                this.logger.Debug($"Attempting to delete games archive");
                Archive.DeleteGamesArchive(archiveFileAndPath);
                this.logger.Debug($"Games archive deleted");

                this.api.Notifications.Add(persistPluginNotificationId, "Games data synced to Google Drive", NotificationType.Info);
            }
            catch (Exception e)
            {
                this.logger.Error($"An error occurred persisting data to Google Drive: {e.Message}");
                this.api.Notifications.Add(persistPluginErrorNotificationId, "An error occurred persisting games data to Google Drive", NotificationType.Error);
            }
        }

        public void OnGameUninstalled(Game game)
        {
            // Add code to be executed when game is uninstalled.
        }

        public void OnApplicationStarted()
        {
            this.logger.Debug("PlaynitePersistPlugin initialization!");
            this.testProvider = new GoogleDriveDriver(this.logger);
            this.testProvider.Connect();

            //this.logger.Debug("Attempting to download archive.");
            //this.testProvider.SyncFrom(archiveFile, Path.Combine("Archives", archiveFile));
        }
    }
}
