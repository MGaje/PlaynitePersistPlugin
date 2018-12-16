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
            string zipFilename = Path.Combine("Archives", "games.zip");
            ZipFile.CreateFromDirectory(Path.Combine("library", "games"), zipFilename);

            return zipFilename;
        }
    }
}
