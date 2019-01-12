using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace PlaynitePersistPlugin.Archives
{
    public static class Archive
    {
        /// <summary>
        /// Create games data archive.
        /// </summary>
        /// <param name="path">The parent path.</param>
        /// <param name="archiveFile">The games data archive.</param>
        /// <returns></returns>
        public static string CreateGamesArchive(string path, string archiveFile)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string gamesArchive = Path.Combine(path, archiveFile);
            ZipFile.CreateFromDirectory(Path.Combine("library", "games"), gamesArchive);

            return gamesArchive;
        }

        /// <summary>
        /// Delete the games data archive.
        /// </summary>
        /// <param name="archiveFile">The games data archive.</param>
        public static void DeleteGamesArchive(string archiveFile)
        {
            if (File.Exists(archiveFile))
            {
                File.Delete(archiveFile);
            }
        }

        /// <summary>
        /// Extract games data from archive.
        /// </summary>
        /// <param name="archiveFile">The games data archive.</param>
        /// <param name="extractPath">The path to extract the archive to.</param>
        public static void ExtractGamesArchive(string archiveFile)
        {
            if (!File.Exists(archiveFile))
            {
                throw new FileNotFoundException($"{archiveFile} not found!");
            }

            string extractPath = Path.Combine("library", "games");

            using (var archive = ZipFile.OpenRead(archiveFile))
            {
                foreach (var entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(extractPath, entry.FullName), true);
                }
            }
        }
    }
}
