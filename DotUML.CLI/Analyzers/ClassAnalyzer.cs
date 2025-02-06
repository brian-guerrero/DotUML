using System.Threading.Tasks;

using DotUML.CLI.Models;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotUML.CLI.Analyzers;

public static class ClassAnalyzer
{
    public static HashSet<ObjectInfo> ExtractClassesFromSolution(string solutionFilePath)
    {
        var objectInfos = new HashSet<ObjectInfo>();

        // Locate and register the default instance of MSBuild installed on this machine.
        // https://github.com/dotnet/roslyn/issues/17974#issuecomment-624408861
        if (!MSBuildLocator.IsRegistered)
        {
            Console.WriteLine("Registering MSBuild defaults...");
            MSBuildLocator.RegisterDefaults();
        }
        using (var workspace = MSBuildWorkspace.Create())
        {
            workspace.WorkspaceFailed += (o, e) =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    Console.WriteLine($"Workspace error: {e.Diagnostic.Message}");
            };
            Console.WriteLine($"Opening solution: {solutionFilePath}");
            var solution = workspace.OpenSolutionAsync(solutionFilePath).Result;
            foreach (var project in solution.Projects)
            {
                Console.WriteLine($"Analyzing project: {project.Name}");
                foreach (var document in project.Documents)
                {
                    Console.WriteLine($"Analyzing document: {document.Name}");
                    if (document.SourceCodeKind == SourceCodeKind.Regular)
                    {
                        Console.WriteLine($"Processing document: {document.Name}");
                        var syntaxTree = document.GetSyntaxTreeAsync().Result;
                        var root = syntaxTree.GetRoot();
                        var semanticModel = document.GetSemanticModelAsync().Result;

                        foreach (var classNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
                        {
                            if (string.IsNullOrEmpty(classNode.Identifier.Text))
                            {
                                Console.WriteLine("Skipping class without identifier");
                                continue;
                            }
                            Console.WriteLine($"Found class: {classNode.Identifier.Text}");
                            string className = classNode.Identifier.Text;
                            var baseClass = classNode.BaseList?.Types.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax>().FirstOrDefault();
                            if (baseClass is null)
                            {
                                objectInfos.Add(new ClassInfo(className));
                            }
                            if (semanticModel is not null && baseClass is not null)
                            {
                                var symbol = semanticModel.GetSymbolInfo(baseClass.Type).Symbol;
                                if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Interface)
                                {
                                    objectInfos.Add(new ClassInfo(className, namedTypeSymbol.Name));
                                    objectInfos.Add(new InterfaceInfo(namedTypeSymbol.Name));
                                }
                                else
                                {
                                    objectInfos.Add(new ClassInfo(className, baseClass.Type.ToString()));
                                }
                            }
                        }
                        foreach (var interfaceNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>())
                        {
                            if (string.IsNullOrEmpty(interfaceNode.Identifier.Text))
                            {
                                Console.WriteLine("Skipping interface without identifier");
                                continue;
                            }
                            Console.WriteLine($"Found interface: {interfaceNode.Identifier.Text}");
                            string interfaceName = interfaceNode.Identifier.Text;
                            objectInfos.Add(new InterfaceInfo(interfaceName));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipping document: {document.Name} (not regular source code)");
                    }
                }
            }

            Console.WriteLine("Finished analyzing solution.");
            return objectInfos;
        }
    }
}

