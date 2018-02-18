using System.Collections.Generic;


namespace TagBackup {


    /// <summary>
    /// Tags for a directory.
    /// </summary>
    sealed class TagDirectory {


        public List<TagFile> Files = new List<TagFile>();


        /// <summary>
        /// Add a file with specified tags.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="tags">Tags</param>
        public void AddFileTags(string filename, HashSet<string> tags) {
            Files.Add(new TagFile(filename, tags));
        }


    }


}
