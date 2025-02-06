using DotUML.CLI.Models;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotUML.CLI.Analyzers;

public class ClassAnalyzer
{
    private readonly HashSet<ObjectInfo> objectInfos = new();
    public HashSet<ObjectInfo> ExtractClassesFromSolution(string solutionFilePath)
    {
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
                AnalyzeProject(project);
            }

            Console.WriteLine("Finished analyzing solution.");
            return objectInfos;
        }
    }

    private void AnalyzeProject(Project project)
    {
        foreach (var document in project.Documents)
        {
            Console.WriteLine($"Analyzing document: {document.Name}");
            if (document.SourceCodeKind == SourceCodeKind.Regular)
            {
                Console.WriteLine($"Processing document: {document.Name}");
                AnalyzeDocument(document);
            }
        }
    }

    private void AnalyzeDocument(Document document)
    {
        var syntaxTree = document.GetSyntaxTreeAsync().Result;
        var root = syntaxTree.GetRoot();
        var semanticModel = document.GetSemanticModelAsync().Result;

        AnalyzeClasses(root, semanticModel);
        AnalyzeInterfaces(root);
    }

    private void AnalyzeClasses(SyntaxNode root, SemanticModel semanticModel)
    {
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
            ClassInfo classInfo = null;
            if (baseClass is null)
            {
                classInfo = new ClassInfo(className);
            }
            if (semanticModel is not null && baseClass is not null)
            {
                var symbol = semanticModel.GetSymbolInfo(baseClass.Type).Symbol;
                if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    classInfo = new ClassInfo(className, namedTypeSymbol.Name);
                    objectInfos.Add(new InterfaceInfo(namedTypeSymbol.Name));
                }
                else
                {
                    classInfo = new ClassInfo(className, baseClass.Type.ToString());
                }
            }
            AnalyzePropertiesForObjectInfo(classNode.Members, classInfo!);
            objectInfos.Add(classInfo!);

        }
    }

    private static void AnalyzePropertiesForObjectInfo(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, ObjectInfo classInfo)
    {
        var properties = members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            Console.WriteLine($"Found property: {property.Identifier.Text}");
            var propertyName = property.Identifier.Text;
            var propertyType = property.Type.ToString();
            var accessibility = property.Modifiers.ToString();
            Console.WriteLine($"Property accessibility: {accessibility}");
            classInfo!.AddProperty(new PropertyInfo(propertyName, accessibility, propertyType));
        }
    }

    private void AnalyzeInterfaces(SyntaxNode root)
    {
        foreach (var interfaceNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(interfaceNode.Identifier.Text))
            {
                Console.WriteLine("Skipping interface without identifier");
                continue;
            }
            Console.WriteLine($"Found interface: {interfaceNode.Identifier.Text}");
            string interfaceName = interfaceNode.Identifier.Text;
            var interfaceInfo = new InterfaceInfo(interfaceName);
            AnalyzePropertiesForObjectInfo(interfaceNode.Members, interfaceInfo);
            objectInfos.Add(interfaceInfo);
        }
    }
}
