
var modeArg = Argument("mode", "All");

string[] allBuildModes = new[] { "None", "Current", "Modified" };
string[] modes;
if (string.Equals(modeArg, "All", StringComparison.OrdinalIgnoreCase))
{
    modes = allBuildModes;
}
else
{
    modes = new[] { modeArg };
}

Information("Modes:" + string.Join(",", modes));

Task("Build")
    .Does(() => {

    CreateDirectory("bin");
    CleanDirectory("bin");
        
    foreach (var mode in modes) 
    {
        string modeSubDir = "bin/" + mode;
        CreateDirectory(modeSubDir + "/v1");
        CreateDirectory(modeSubDir + "/v2");
        Information("Building mode: " + mode);
        
        var settings = new MSBuildSettings();
        settings.WithTarget("Rebuild");
        if (mode != "None") 
        {
            string toolPath = GetFiles($"./ilrepack/**/{mode}/ilrepack.exe").First().FullPath;
			Information("ILRepackToolPath:" + toolPath);
            settings.WithProperty("ILRepackToolPath", toolPath);
        }

        MSBuild("WpfSideBySide.sln", settings);
        
        string glob;
        if (mode != "None") 
        {
            glob = "**/merged";
        }
        else
        {
		    glob = "**/bin/Debug";
        }
        
        var files = GetFiles($"./**/v1/{glob}/*");
        CopyFiles(files, modeSubDir + "/v1");
        files = GetFiles($"./**/v2/{glob}/*");
        CopyFiles(files, modeSubDir + "/v2");
        files = GetFiles("./WpfSideBySide/bin/Debug/*");
        CopyFiles(files, modeSubDir);
    }
});

RunTarget("Build");