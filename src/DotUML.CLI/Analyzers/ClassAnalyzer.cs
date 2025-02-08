

using DotUML.CLI.Models;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace DotUML.CLI.Analyzers;

public class ClassAnalyzer
{
    private readonly ILogger<ClassAnalyzer> _logger;
    public ClassAnalyzer(ILogger<ClassAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<Namespaces> ExtractClassesFromSolutionAsync(string solutionFilePath)
    {
        // Locate and register the default instance of MSBuild installed on this machine.
        // https://github.com/dotnet/roslyn/issues/17974#issuecomment-624408861
        if (!MSBuildLocator.IsRegistered)
        {
            _logger.LogInformation("Registering MSBuild defaults...");
            var instance = MSBuildLocator.QueryVisualStudioInstances().First();
            MSBuildLocator.RegisterInstance(instance);
        }
        using (var workspace = MSBuildWorkspace.Create())
        {
            workspace.WorkspaceFailed += (o, e) =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    _logger.LogError($"Workspace error: {e.Diagnostic.Message}");
            };
            _logger.LogInformation($"Opening solution: {solutionFilePath}");
            var solution = await workspace.OpenSolutionAsync(solutionFilePath);
            Namespaces namespaces = null;
            foreach (var project in solution.Projects)
            {
                _logger.LogInformation($"Analyzing project: {project.Name}");
                await foreach (var ns in AnalyzeProject(project))
                {
                    if (namespaces is null)
                    {
                        namespaces = new Namespaces([ns]);
                    }
                    else
                    {
                        namespaces.Add(ns);
                    }
                }
            }

            _logger.LogInformation("Finished analyzing solution.");
            return namespaces!;
        }
    }

    private async IAsyncEnumerable<NamespaceInfo> AnalyzeProject(Project project)
    {
        foreach (var document in project.Documents)
        {
            _logger.LogInformation($"Analyzing document: {document.Name}");
            if (document.SourceCodeKind == SourceCodeKind.Regular)
            {
                _logger.LogInformation($"Processing document: {document.Name}");
                var ns = await AnalyzeDocument(document);
                yield return ns;
            }
        }
    }

    private async Task<NamespaceInfo> AnalyzeDocument(Document document)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync();
        var root = syntaxTree.GetRoot();
        var semanticModel = await document.GetSemanticModelAsync();
        NamespaceInfo namespaceInfo = null;
        var namespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDeclaration is not null)
            namespaceInfo = new NamespaceInfo(namespaceDeclaration?.Name.ToString() ?? string.Empty);
        var fileScopedNamespace = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
        if (fileScopedNamespace is not null)
            namespaceInfo = new NamespaceInfo(fileScopedNamespace.Name.ToString());
        namespaceInfo ??= new NamespaceInfo(string.Empty);
        var records = AnalyzeRecords(root, semanticModel);
        namespaceInfo.AddObjectInfo(records);
        var classes = AnalyzeClasses(root, semanticModel);
        namespaceInfo.AddObjectInfo(classes);
        var interfaces = AnalyzeInterfaces(root);
        namespaceInfo.AddObjectInfo(interfaces);
        return namespaceInfo;
    }

    private IEnumerable<ObjectInfo> AnalyzeRecords(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var recordNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(recordNode.Identifier.Text))
            {
                _logger.LogInformation("Skipping record without identifier");
                continue;
            }
            _logger.LogInformation($"Found record: {recordNode.Identifier.Text}");
            string className = recordNode.Identifier.Text;
            var baseRecord = recordNode.BaseList?.Types.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax>().FirstOrDefault();
            ClassInfo recordInfo = null;
            if (baseRecord is null)
            {
                recordInfo = new ClassInfo(className);
            }
            if (semanticModel is not null && baseRecord is not null)
            {
                var symbol = semanticModel.GetSymbolInfo(baseRecord.Type).Symbol;
                if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    recordInfo = new ClassInfo(className, namedTypeSymbol.Name);
                    yield return new InterfaceInfo(namedTypeSymbol.Name);
                }
                else
                {
                    recordInfo = new ClassInfo(className, baseRecord.Type.ToString());
                }
            }

            AnalyzePropertiesFromRecordConstructor(recordNode.ParameterList, recordInfo);
            AnalyzePropertiesForObjectInfo(recordNode.Members, recordInfo!);
            AnalyzeMethodsForObjectInfo(recordNode.Members, recordInfo!);
            if (recordInfo is not null)
                yield return recordInfo;
        }
    }

    private void AnalyzePropertiesFromRecordConstructor(ParameterListSyntax parameterList, ClassInfo? recordInfo)
    {
        foreach (var parameter in parameterList.Parameters)
        {
            var parameterName = parameter.Identifier.Text;
            var parameterType = parameter.Type.ToString();
            recordInfo?.AddProperty(new PropertyInfo(parameterName, "public", (Models.TypeInfo)parameterType));
        }

    }

    private IEnumerable<ObjectInfo> AnalyzeClasses(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var classNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(classNode.Identifier.Text))
            {
                _logger.LogInformation("Skipping class without identifier");
                continue;
            }
            _logger.LogInformation($"Found class: {classNode.Identifier.Text}");
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
                    yield return new InterfaceInfo(namedTypeSymbol.Name);
                }
                else
                {
                    classInfo = new ClassInfo(className, baseClass.Type.ToString());
                }
            }
            AnalyzeDependenciesOnConstructor(classNode, classInfo);
            AnalyzePropertiesForObjectInfo(classNode.Members, classInfo!);
            AnalyzeMethodsForObjectInfo(classNode.Members, classInfo!);
            if (classInfo is not null)
                yield return classInfo;

        }
    }

    private void AnalyzeDependenciesOnConstructor(ClassDeclarationSyntax classNode, ClassInfo? classInfo)
    {
        var constructors = classNode.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax>();
        foreach (var constructor in constructors)
        {
            foreach (var parameter in constructor.ParameterList.Parameters)
            {
                var parameterType = parameter.Type.ToString();
                classInfo?.AddDependency(new DependencyInfo((Models.TypeInfo)parameterType));
            }
        }
    }

    private void AnalyzePropertiesForObjectInfo(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, ObjectInfo objectInformation)
    {
        var properties = members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            _logger.LogInformation($"Found property: {property.Identifier.Text}");
            var propertyName = property.Identifier.Text;
            var propertyType = property.Type.ToString();
            var accessibility = property.Modifiers.ToString();
            _logger.LogInformation($"Property accessibility: {accessibility}");
            objectInformation.AddProperty(new PropertyInfo(propertyName, accessibility, (Models.TypeInfo)propertyType));
        }
    }

    private void AnalyzeMethodsForObjectInfo(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, ObjectInfo objectInformation)
    {
        var methods = members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            _logger.LogInformation($"Found property: {method.Identifier.Text}");
            var methodName = method.Identifier.Text;
            var returnType = method.ReturnType.ToString();
            var accessibility = method.Modifiers.ToString();
            _logger.LogInformation($"Property accessibility: {accessibility}");
            var methodInfo = new MethodInfo(methodName, accessibility, (Models.TypeInfo)returnType);
            foreach (var parameter in method.ParameterList.Parameters)
            {
                var parameterName = parameter.Identifier.Text;
                var parameterType = parameter.Type.ToString();
                methodInfo.AddArgument(new MethodArgumentInfo(parameterName, (Models.TypeInfo)parameterType));
            }
            objectInformation.AddMethod(methodInfo);
        }
    }

    private IEnumerable<InterfaceInfo> AnalyzeInterfaces(SyntaxNode root)
    {
        foreach (var interfaceNode in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(interfaceNode.Identifier.Text))
            {
                _logger.LogInformation("Skipping interface without identifier");
                continue;
            }
            _logger.LogInformation($"Found interface: {interfaceNode.Identifier.Text}");
            string interfaceName = interfaceNode.Identifier.Text;
            var interfaceInfo = new InterfaceInfo(interfaceName);
            AnalyzePropertiesForObjectInfo(interfaceNode.Members, interfaceInfo);
            AnalyzeMethodsForObjectInfo(interfaceNode.Members, interfaceInfo);
            yield return interfaceInfo;
        }
    }
}
