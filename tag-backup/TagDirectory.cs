using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace TagBackup {


    /// <summary>
    ///     Tags for a directory.
    /// </summary>
    sealed class TagDirectory {


        static readonly Regex FilterRegex = new Regex(@"(?m)\s*\n\d", RegexOptions.Compiled);

        public List<TagFile> Files = new List<TagFile>();


        /// <summary>
        ///     Add a file with specified tags.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="tags">Tags</param>
        /// <returns>Tag structure</returns>
        public HashSet<string> AddFileTags(string filename, HashSet<string> tags) {
            var o = new HashSet<string>();

            if (Program.Opt.NoColor) {
                foreach (string tag in tags)
                    o.Add(FilterRegex.Replace(tag, ""));
            }
            else
                o = tags;

            Files.Add(new TagFile(filename, o));

            return o;
        }


    }


}
