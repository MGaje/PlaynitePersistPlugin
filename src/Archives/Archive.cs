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
        public static string CreateGamesArchive()
        {
            if (!Directory.Exists("Archives"))
            {
                Directory.CreateDirectory("Archives");
            }

            string gamesArchive = Path.Combine("Archives", "playnite-games-data.zip");
            ZipFile.CreateFromDirectory(Path.Combine("library", "games"), gamesArchive);

            return gamesArchive;
        }

        public static void DeleteGamesArchive()
        {
            string gamesArchive = Path.Combine("Archives", "playnite-games-data.zip");
            if (File.Exists(gamesArchive))
            {
                File.Delete(gamesArchive);
            }
        }
    }
}
