﻿using System;
using System.IO;
using PListNet;
using PListNet.Nodes;
using System.Collections.Generic;
using Newtonsoft.Json;
using Mono.Unix.Native;
using Plossum.CommandLine;


namespace TagBackup {


    class Program {


        const string TagName = "com.apple.metadata:_kMDItemUserTags";

        const int ErrParserHasErrors          = 64;
        const int ErrInvalidCommand           = 65;
        const int ErrPathDirectoryDoesntExist = 66;

        internal static readonly Options Opt = new Options();


        /// <summary>
        /// Main.
        /// </summary>
        /// <returns>Exit code</returns>
        public static int Main() {
            var parser = new CommandLineParser(Opt);

            parser.Parse();

            /*
            // Testing commandline switches
            Console.WriteLine(parser.UsageInfo.ToString(78, false));
            return 0;
            */

            if (Opt.Help) {
                Console.WriteLine(parser.UsageInfo.ToString(78, false));

                return 0;
            }

            if (parser.HasErrors) {
                Console.WriteLine(parser.UsageInfo.ToString(78, true));

                return ErrParserHasErrors;
            }

            if (Opt.Backup)
                return BackupDirectoryTags();

            return ErrInvalidCommand;
        }


        /// <summary>
        /// Get the file tags.
        /// </summary>
        /// <param name="filename">Filename to extract tags from</param>
        /// <returns>Tags</returns>
        static HashSet<string> GetFileTags(string filename) {
            var tags = new HashSet<string>();

            Syscall.getxattr(filename, TagName, out byte[] tagData);

            if (tagData == null) {
                // empty tags
                Console.WriteLine("Warning: \"{0}\" has no tags", filename);

                return tags;
            }

            using (var stream = new MemoryStream(tagData)) {
                if (!(PList.Load(stream) is ArrayNode root)) {
                    // invalid tags
                    Console.WriteLine("Warning: \"{0}\" has invalid tags", filename);

                    return tags;
                }

                // tags should be fine at this point
                foreach (PNode pNode in root) {
                    var t = (StringNode) pNode;
                    tags.Add(t.Value);
                }
            }

            return tags;
        }


        /// <summary>
        /// Backup tags of all files in the given directory.
        /// </summary>
        /// <returns>Exit code</returns>
        static int BackupDirectoryTags() {
            var    tagDir       = new TagDirectory();
            var    i            = 0;
            int    reqsExitCode = CheckRequirements(Opt.DirectoryPath);
            string jsonPath     = Opt.DirectoryPath + "/" + Opt.JsonFilename;

            if (reqsExitCode != 0)
                return reqsExitCode;

            // everything is fine
            Console.WriteLine("Backing up the directory '{0}' to JSON backup '{1}'", Opt.DirectoryPath, jsonPath);

            foreach (string filename in Directory.EnumerateFiles(Opt.DirectoryPath, "*.*", SearchOption.TopDirectoryOnly)) {
                // ignore the potentially existing json backup file
                if (filename == jsonPath)
                    continue;

                HashSet<string> tags = GetFileTags(filename);

                // ignore files without any tags
                // or files with broken tags
                if (tags.Count <= 0)
                    continue;

                // everything should be fine at this point
                tagDir.AddFileTags(filename, tags);
                i++;
            }

            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(tagDir, Opt.Uglify ? Formatting.None : Formatting.Indented));

            Console.WriteLine("Successfully backed up {0} {1} with tags", i, i > 1 ? "files" : "file");

            return 0;
        }


        /// <summary>
        /// Check all the requirements to process the directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>Error code or 0 if OK</returns>
        static int CheckRequirements(string path) {
            bool exists = Directory.Exists(path);

            if (!exists) {
                Console.WriteLine("Can't process the directory '{0}': it doesn't exist.", path);

                return ErrPathDirectoryDoesntExist;
            }

            return 0;
        }


    }


}
