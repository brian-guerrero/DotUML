using DotUML.CLI.Analyzers;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotUML.CLI.Mermaid;


var app = ConsoleApp.Create();
app
    .ConfigureLogging(x =>
        {
            x.SetMinimumLevel(LogLevel.Information);
        })
    .ConfigureServices((services) =>
    {
        services.AddTransient<ClassAnalyzer>();
        services.AddTransient<ClassDiagramGenerator>();
    });


app.Add("generate", async ([FromServices] ClassAnalyzer classAnalyzer,
    [FromServices] ClassDiagramGenerator diagramGenerator,
    string solutionPath,
    string outputPath = "mermaid.md") =>
{
    if (string.IsNullOrEmpty(solutionPath))
    {
        Console.WriteLine("Usage: generate <solution-path> <output-path>");
        return;
    }


    try
    {
        Console.WriteLine("Extracting classes...");
        var classInfos = await classAnalyzer.ExtractClassesFromSolutionAsync(solutionPath);

        Console.WriteLine("Generating UML...");
        string mermaidDiagram = diagramGenerator.GenerateDiagram(classInfos);

        Console.WriteLine("Writing to output file...");
        diagramGenerator.WriteToReadme(outputPath, mermaidDiagram);

        Console.WriteLine("Done!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
    }
});

app.Run(args);