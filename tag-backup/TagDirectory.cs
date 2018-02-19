using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace TagBackup {


    /// <summary>
    /// Tags for a directory.
    /// </summary>
    sealed class TagDirectory {


        static readonly Regex FilterRegex = new Regex(@"(?m)\s*\n\d", RegexOptions.Compiled);

        public List<TagFile> Files = new List<TagFile>();


        /// <summary>
        /// Add a file with specified tags.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="tags">Tags</param>
        public void AddFileTags(string filename, HashSet<string> tags) {
            var o = new HashSet<string>();

            if (Program.Opt.NoColor) {
                foreach (string tag in tags)
                    o.Add(FilterRegex.Replace(tag, ""));
            }
            else
                o = tags;

            Files.Add(new TagFile(filename, o));

            if (Program.Opt.Verbose)
                Console.WriteLine("\"{0}\" - {1}", filename, JsonConvert.SerializeObject(o));
        }


    }


}
