using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaynitePersistPlugin.Providers
{
    /// <summary>
    /// Cloud Provider Interface.
    /// </summary>
    public interface ICloudProvider
    {
        /// <summary>
        /// Connect to the cloud provider.
        /// </summary>
        void Connect();

        /// <summary>
        /// Sync games data to provider.
        /// </summary>
        /// <param name="archiveFile">The archive of games data.</param>
        void SyncTo(string path, string archiveFile);

        /// <summary>
        /// Get games data from provider.
        /// </summary>
        /// <param name="archiveFile">The archive of games data.</param>
        /// <param name="downloadPath">The location to download the games archive to.</param>
        void SyncFrom(string archiveFile, string downloadPath);
    }
}
