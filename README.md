# DemoWpfSideBySide
This demonstrates side by side loading of the same assembly that has been modified with ILRepack.
ILRepack 2.0.13 will modify BAML resources in a way that removes version numbers preventing mutliple version from being loaded.
See issue [#205](https://github.com/gluck/il-repack/issues/205) in the ILRepack project for discussion.

The project consists of a host application, a plugin interface assembly, and 2 builds of a plugin that differ by version number.
The plugins contain some XAML controls and simple C# classes.

# Build

Run .\build.ps1 

# Run

The output is 3 folders in bin: None, Current, Modified

 - "None" contains a version of the project that is not modified by ILRepack
 - "Current" uses the current version of ILRepack (2.0.13) to modify the plugin assemblies. It will produce an error for one of the plugins when running the host application.
 - "Modified" uses a tweaked version of ILRepack (based off of 2.0.13) that fixes the assembly version information.

Run the host application WpfSideBySide.exe and click "Load" to load the plugins.
