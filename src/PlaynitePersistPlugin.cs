using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlaynitePersistPlugin.Drivers;
using PlaynitePersistPlugin.Archives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PlaynitePersistPlugin
{
    public class PlaynitePersistPlugin : IGenericPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private GoogleDriveDriver testDriver;
        private const string persistPluginNotificationId = "persistSuccess";
        private const string persistPluginErrorNotificationId = "persistError";

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
                this.logger.Debug($"Creating games archive to persist...");
                Archive.CreateGamesArchive();
                this.logger.Debug($"Done creating games archive!");

                this.logger.Debug($"Attempting to upload the games archive...");
                this.testDriver.UploadGamesArchive();
                this.logger.Debug($"Finished uploading games archive");

                this.logger.Debug($"Attempting to delete games archive");
                Archive.DeleteGamesArchive();
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
            this.testDriver = new GoogleDriveDriver(this.logger);
        }
    }
}
