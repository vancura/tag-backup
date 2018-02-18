using System.Collections.Generic;


namespace TagBackup {


    /// <summary>
    /// Tags for a file.
    /// </summary>
    sealed class TagFile {


        /// <summary>
        /// Filename.
        /// </summary>
        public string Filename { get; }


        /// <summary>
        /// Tags.
        /// </summary>
        public HashSet<string> Tags { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="tags">Tags</param>
        public TagFile(string filename, HashSet<string> tags) {
            Filename = filename;
            Tags     = tags;
        }


    }


}
