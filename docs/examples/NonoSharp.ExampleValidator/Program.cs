using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

var examplesDirectory = "docs/examples";
var examples = Directory.GetFiles(examplesDirectory, "*.cs");

const string libraryProject = "src/NonoSharp/NonoSharp.csproj";

Console.WriteLine($"Found {examples.Length} in {examplesDirectory}");


Directory.CreateDirectory("temp");

try
{

    Process[] processes = new Process[examples.Length];

    for (int i = 0; i < examples.Length; i++)
    {
        processes[i] = StartValidation(examples[i]);
    }

    if (!WaitForProcesses(processes))
    {
        Console.WriteLine("Failed to validate every example.");
        return 1;
    }

    Console.WriteLine($"All {examples.Length} examples successfully compile.");
    return 0;
} 
finally
{
    Directory.Delete("temp", true);
}

static Process StartValidation(string examplePath)
{
    string exampleFileName = Path.GetFileName(examplePath);
    string projectPath = CreateExampleProject(examplePath);

    Process process = new()
    {
        StartInfo = new() { FileName = "dotnet" }
    };

    process.StartInfo.ArgumentList.Add("build");
    process.StartInfo.ArgumentList.Add(projectPath);
    process.StartInfo.ArgumentList.Add("-v");
    process.StartInfo.ArgumentList.Add("quiet");
    process.StartInfo.ArgumentList.Add("-c");
    process.StartInfo.ArgumentList.Add("Release");

    Console.WriteLine($"Starting validation of {exampleFileName}...");
    process.Start();

    return process;
}

static string CreateExampleProject(string examplePath)
{
    // Create the project including the required csproj and .cs code example file
    string exampleName = Path.GetFileNameWithoutExtension(examplePath);
    string projectPath = Path.Combine("temp", exampleName);
    Directory.CreateDirectory(projectPath);
    CreateProjectFile(projectPath, examplePath);

    return projectPath;
}

/// <summary>
/// Sets up the required csproj file to build an example.
/// </summary>
/// <param name="projectPath">The path to create the project in</param>
/// <param name="examplePath">The path of the .cs file of the example from this repo's root</param>
static void CreateProjectFile(string projectPath, string examplePath)
{
    // Create the csproj file required to build the example

    var project = new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"));

    var propertyGroup = new XElement(
        "PropertyGroup",
            new XElement("OutputType", "Exe"),
            new XElement("ImplicitUsings", "enable"),
            new XElement("TargetFramework", "net10.0")
    );
    project.Add(propertyGroup);

    var itemGroup = new XElement(
        "ItemGroup",
            new XElement("ProjectReference",
                new XAttribute("Include", Path.Combine(Directory.GetCurrentDirectory(), libraryProject))
            ), // Add NonoSharp to dependency

            new XElement("Compile", 
                new XAttribute("Include", Path.Combine(Directory.GetCurrentDirectory(), examplePath))
            )
    );
    project.Add(itemGroup);

    string csproj = Path.Combine(projectPath, Path.GetFileName(projectPath) + ".csproj");
    
    var document = new XDocument(project);
    document.Save(csproj);
}

static bool WaitForProcesses(Process[] processes)
{
    bool success = true;

    var failureInfo = new List<string>();
    foreach (Process process in processes)
    {
        process.WaitForExit();

        string exampleName = Path.GetFileName(process.StartInfo.ArgumentList[1]);
        if (process.ExitCode != 0)
        {
            failureInfo.Add($"{exampleName} (exit code {process.ExitCode})");
            success = false;
            break;
        }
    }

    if (failureInfo.Count > 0)
    {
        StringBuilder sb = new();
        sb.AppendLine(new string('=', 10));
        sb.AppendLine("Failed to build the following examples:");
        foreach (var failure in failureInfo)
        {
            sb.AppendLine($"- {failure}");
        }
        Console.WriteLine(sb.ToString());
    }
    return success;
}
