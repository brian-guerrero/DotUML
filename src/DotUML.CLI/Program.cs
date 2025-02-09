using DotUML.CLI.Analyzers;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotUML.CLI.Mermaid;
using DotUML.CLI;
using ZLogger;


var app = ConsoleApp.Create();
app
    .ConfigureLogging(x =>
        {
            x.ClearProviders();
            x.SetMinimumLevel(LogLevel.Warning);
            x.AddZLoggerConsole();
        })
    .ConfigureServices((services) =>
    {
        services.AddTransient<ClassAnalyzer>();
        services.AddTransient<ClassDiagramGenerator>();
    });


app.Add<GenerateCommands>();

app.Run(args);