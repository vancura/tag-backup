using System;
using System.IO;
using PListNet;
using PListNet.Nodes;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Mono.Unix.Native;
using Plossum.CommandLine;


namespace TagBackup {


    class Program {


        const string TagName = "com.apple.metadata:_kMDItemUserTags";

        const int ErrParserHasErrors          = 64;
        const int ErrInvalidCommand           = 65;
        const int ErrPathDirectoryDoesntExist = 66;



        public static int Main() {
            int exitCode = ErrInvalidCommand;
            var opt      = new Options();
            var parser   = new CommandLineParser(opt);

            parser.Parse();

            if (opt.Help) {
                Console.WriteLine(parser.UsageInfo.ToString(78, false));

                return 0;
            }

            if (parser.HasErrors) {
                Console.WriteLine(parser.UsageInfo.ToString(78, true));

                return ErrParserHasErrors;
            }

            string jsonPath = opt.DirectoryPath + "/" + opt.JsonFilename;

            if (opt.Backup)
                exitCode = BackupDirectoryTags(directoryPath: opt.DirectoryPath, jsonPath: jsonPath, uglify: opt.Uglify, verbose: opt.Verbose);


            return exitCode;
        }


        static HashSet<string> GetFileTags(string filename) {
            var tags = new HashSet<string>();

            Syscall.getxattr(filename, TagName, out byte[] tagData);

            if (tagData != null) {
                using (var stream = new MemoryStream(tagData)) {
                    if (!(PList.Load(stream) is ArrayNode root))
                        return tags;

                    foreach (PNode pNode in root) {
                        var t = (StringNode) pNode;
                        tags.Add(t.Value);
                    }
                }
            }

            return tags;
        }


        static int BackupDirectoryTags(string directoryPath, string jsonPath, bool uglify, bool verbose) {
            var exitCode     = 0;
            var tagDir       = new TagDirectory();
            var i            = 0;
            int reqsExitCode = CheckRequirements(directoryPath);

            if (reqsExitCode != 0)
                return reqsExitCode;

            // everything is fine
            Console.WriteLine("Backing up the directory '{0}' to JSON backup '{1}'", directoryPath, jsonPath);

            foreach (string filename in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)) {
                HashSet<string> tags = GetFileTags(filename);

                if (tags.Count <= 0)
                    continue;

                tagDir.AddFileTags(filename, tags);

                i++;

                if (verbose)
                    Console.WriteLine("{0}: '{1}' - {2}", i, filename, JsonConvert.SerializeObject(tags));
            }

            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(tagDir, uglify ? Formatting.None : Formatting.Indented));

            Console.WriteLine("Successfully backed up {0} {1} with tags", i, i > 1 ? "files" : "file");

            return exitCode;
        }


        /// <summary>
        /// Check all the requirements to process the directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>Error code or 0 if OK</returns>
        static int CheckRequirements(string path) {
            bool exists   = Directory.Exists(path);
            var  exitCode = 0;

            if (!exists) {
                Console.WriteLine("Can't process the directory '{0}': it doesn't exist.", path);

                exitCode = ErrPathDirectoryDoesntExist;
            }

            return exitCode;
        }


    }


}
