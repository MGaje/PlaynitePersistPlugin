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

        public static void DeleteGamesArchive(string archiveFile)
        {
            if (File.Exists(archiveFile))
            {
                File.Delete(archiveFile);
            }
        }

        public static void ExtractGamesArchive(string archiveFile)
        {

        }
    }
}
