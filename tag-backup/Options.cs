using System.Diagnostics.CodeAnalysis;
using Plossum.CommandLine;


namespace TagBackup {


    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [CommandLineManager(ApplicationName = "tag-backup", Copyright = "Copyright (c) Václav Vančura",
        EnabledOptionStyles             = OptionStyles.LongUnix | OptionStyles.Group)]
    [CommandLineOptionGroup("commands", Name = "Commands", Require = OptionGroupRequirement.ExactlyOne)]
    [CommandLineOptionGroup("options", Name  = "Options")]
    sealed class Options {


        [CommandLineOption(Name = "h", Aliases = "help", Description = "Shows this help text", GroupId = "commands")]
        public bool Help { get; set; }


        [CommandLineOption(Name = "b", Aliases = "backup", Description = "Backup tags to a JSON file", GroupId = "commands")]
        public bool Backup { get; set; }



        // ===========
        // = OPTIONS =
        // ===========


        string _directoryPath;
        string _jsonFilename;


        [CommandLineOption(Name = "p", Aliases = "path", Description = "Specify the path to the directory to backup (. if not specified)", GroupId = "options")]
        public string DirectoryPath {
            get => _directoryPath ?? ".";
            set => _directoryPath = value;
        }


        [CommandLineOption(Name = "j", Aliases = "json", Description = "Specify the filename for the JSON backup (_tags.json if not specified)", GroupId = "options")]
        public string JsonFilename {
            get => _jsonFilename ?? "_tags.json";
            set => _jsonFilename = value;
        }


        [CommandLineOption(Name = "uglify", Description = "Uglify the backup JSON", GroupId = "options")]
        public bool Uglify { get; set; }


        [CommandLineOption(Name = "v", Aliases = "verbose", Description = "Produce verbose output", GroupId = "options")]
        public bool Verbose { get; set; }


    }


}
