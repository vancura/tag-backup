using System;
using System.IO;
using PListNet;
using PListNet.Nodes;
using System.Collections.Generic;
using System.Linq;
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

        static readonly Regex FilterRegex = new Regex(@"(?m)\s*(?<=\n\d)(\n\d){1,}", RegexOptions.Compiled);


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

            if (opt.Backup)
                exitCode = BackupDirectoryTags(directoryPath: opt.DirectoryPath, jsonPath: opt.JsonPath, uglify: opt.Uglify, verbose: opt.Verbose);

            if (opt.Cleanup)
                exitCode = CleanupDirectoryTags(directoryPath: opt.DirectoryPath, verbose: opt.Verbose);

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


        static int CleanupDirectoryTags(string directoryPath, bool verbose) {
            var  exitCode     = 0;
            uint i            = 0;
            int  reqsExitCode = CheckRequirements(directoryPath);

            if (reqsExitCode != 0)
                return reqsExitCode;

            // everything is fine
            Console.WriteLine("Cleaning up the directory '{0}'", directoryPath);

            foreach (string filename in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)) {
                HashSet<string> tags     = GetFileTags(filename);
                var             rootNode = new ArrayNode();
                var             j        = 0;

                if (tags.Count <= 0)
                    continue;

                HashSet<string> sortedTags = CleanupTags(tags);

                string o = JsonConvert.SerializeObject(tags);
                string p = JsonConvert.SerializeObject(sortedTags);

                if (o == p)
                    continue;

                i++;

                foreach (string t in sortedTags) {
                    var node = new StringNode {
                        Value = t
                    };

                    rootNode.Insert(j++, node);
                }

                using (var stream = new MemoryStream()) {
                    PList.Save(rootNode, stream, PListFormat.Binary);
                    Syscall.setxattr(filename, TagName, stream.ToArray());
                }

                if (verbose)
                    Console.WriteLine("{0}: '{1}' - {2} → {3}", i, filename, o, p);
            }

            Console.WriteLine("Successfully cleaned up {0} {1} with tags", i, i > 1 ? "files" : "file");

            return exitCode;
        }


        /// <summary>
        /// Cleans the tags up.
        /// </summary>
        /// <returns>Old tags</returns>
        /// <param name="tags">New tags</param>
        static HashSet<string> CleanupTags(IEnumerable<string> tags) {
            var o = new HashSet<string>();

            foreach (string result in tags.Select(tag => FilterRegex.Replace(tag, ""))) {
                o.Add(result);
            }

            return o;
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
