


using DotUML.CLI.Diagram;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            try
            {
                _logger.LogInformation("Registering MSBuild defaults...");
                var instance = MSBuildLocator.QueryVisualStudioInstances().First();
                MSBuildLocator.RegisterInstance(instance);
            }
            catch (InvalidOperationException ie) when (ie.Message.Contains("MSBuild assemblies were already loaded."))
            {
                _logger.LogWarning("MSBuild is already registered.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error registering MSBuild: {e.Message}");
            }
        }
        using (var workspace = MSBuildWorkspace.Create())
        {
            workspace.WorkspaceFailed += (o, e) =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    _logger.LogError($"Workspace error: {e.Diagnostic.Message}");
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Warning)
                    _logger.LogWarning($"Workspace warning: {e.Diagnostic.Message}");
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
        var enums = AnalyzeEnums(root, semanticModel);
        namespaceInfo.AddObjectInfo(enums);
        return namespaceInfo;
    }

    private IEnumerable<EnumInfo> AnalyzeEnums(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var e in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(e.Identifier.Text))
            {
                _logger.LogInformation("Skipping enum without identifier");
                yield break;
            }
            _logger.LogInformation($"Found enum: {e.Identifier.Text}");
            var enumName = e.Identifier.Text;
            var enumInfo = new EnumInfo(enumName);
            foreach (var member in e.Members)
            {
                var memberName = member.Identifier.Text;
                enumInfo.AddValue(memberName);
            }
            yield return enumInfo;
        }
        ;
    }

    private IEnumerable<ObjectInfo> AnalyzeRecords(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var recordNode in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(recordNode.Identifier.Text))
            {
                _logger.LogInformation("Skipping record without identifier");
                continue;
            }
            _logger.LogInformation($"Found record: {recordNode.Identifier.Text}");
            string className = recordNode.Identifier.Text;
            var baseRecords = recordNode.BaseList?.Types.OfType<BaseTypeSyntax>();
            var recordInfo = new ClassInfo(className);
            foreach (var baseObject in ExtractBaseObjects(semanticModel, baseRecords, recordInfo))
            {
                yield return baseObject;
            }
            AnalyzePropertiesFromRecordConstructor(recordNode.ParameterList, recordInfo);
            AnalyzePropertiesForObjectInfo(recordNode.Members, recordInfo!);
            AnalyzeFieldForObjectInfo(recordNode.Members, recordInfo);
            AnalyzeMethodsForObjectInfo(recordNode.Members, recordInfo!);
            if (recordInfo is not null)
                yield return recordInfo;
        }
    }

    private static IEnumerable<ObjectInfo> ExtractBaseObjects(SemanticModel semanticModel, IEnumerable<BaseTypeSyntax>? baseRecords, ClassInfo objectInfo)
    {
        if (baseRecords is not null && baseRecords.Any())
        {
            foreach (var baseRecord in baseRecords)
            {
                var symbol = semanticModel.GetSymbolInfo(baseRecord.Type).Symbol;
                if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    objectInfo.Implements(namedTypeSymbol.Name);
                    yield return new InterfaceInfo(namedTypeSymbol.Name);
                }
                else
                {
                    objectInfo.Inherits(baseRecord.Type.ToString());
                    yield return new ClassInfo(baseRecord.Type.ToString());
                }
            }
        }
    }

    private void AnalyzePropertiesFromRecordConstructor(ParameterListSyntax parameterList, ClassInfo? recordInfo)
    {
        foreach (var parameter in parameterList.Parameters)
        {

            var parameterName = parameter.Identifier.Text;
            var parameterType = parameter.Type;

            recordInfo?.AddProperty(new PropertyInfo(parameterName, "public", TypeSyntaxAnalyzer.GetTypeInfo(parameterType)));
        }

    }

    private IEnumerable<ObjectInfo> AnalyzeClasses(SyntaxNode root, SemanticModel semanticModel)
    {
        foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (string.IsNullOrEmpty(classNode.Identifier.Text))
            {
                _logger.LogInformation("Skipping class without identifier");
                continue;
            }
            _logger.LogInformation($"Found class: {classNode.Identifier.Text}");
            string className = classNode.Identifier.Text;
            var baseClasses = classNode.BaseList?.Types.OfType<BaseTypeSyntax>();
            var classInfo = new ClassInfo(className);
            foreach (var baseObject in ExtractBaseObjects(semanticModel, baseClasses, classInfo))
            {
                yield return baseObject;
            }
            AnalyzeDependenciesOnConstructor(classNode, classInfo);
            AnalyzePropertiesForObjectInfo(classNode.Members, classInfo!);
            AnalyzeFieldForObjectInfo(classNode.Members, classInfo);
            AnalyzeMethodsForObjectInfo(classNode.Members, classInfo!);
            if (classInfo is not null)
                yield return classInfo;

        }
    }

    private void AnalyzeFieldForObjectInfo(SyntaxList<MemberDeclarationSyntax> members, ClassInfo classInfo)
    {
        var fields = members.OfType<FieldDeclarationSyntax>();
        foreach (var field in fields)
        {
            var fieldName = field.Declaration.Variables.First().Identifier.Text;
            var fieldType = field.Declaration.Type;
            var accessibility = field.Modifiers.ToString();
            classInfo.AddProperty(new PropertyInfo(fieldName, accessibility, TypeSyntaxAnalyzer.GetTypeInfo(fieldType)));
        }
    }

    private void AnalyzeDependenciesOnConstructor(ClassDeclarationSyntax classNode, ClassInfo? classInfo)
    {
        var constructors = classNode.Members.OfType<ConstructorDeclarationSyntax>();
        foreach (var constructor in constructors)
        {
            foreach (var parameter in constructor.ParameterList.Parameters)
            {
                var parameterType = parameter.Type;
                classInfo?.AddDependency(new DependencyInfo(TypeSyntaxAnalyzer.GetTypeInfo(parameterType)));
            }
        }
    }

    private void AnalyzePropertiesForObjectInfo(SyntaxList<MemberDeclarationSyntax> members, ObjectInfo objectInformation)
    {
        var properties = members.OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            _logger.LogInformation($"Found property: {property.Identifier.Text}");
            var propertyName = property.Identifier.Text;
            var propertyType = property.Type;
            var accessibility = property.Modifiers.ToString();
            _logger.LogInformation($"Property accessibility: {accessibility}");
            objectInformation.AddProperty(new PropertyInfo(propertyName, accessibility, TypeSyntaxAnalyzer.GetTypeInfo(propertyType)));
        }
    }

    private void AnalyzeMethodsForObjectInfo(SyntaxList<MemberDeclarationSyntax> members, ObjectInfo objectInformation)
    {
        var methods = members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            _logger.LogInformation($"Found property: {method.Identifier.Text}");
            var methodName = method.Identifier.Text;
            var returnType = method.ReturnType;
            var accessibility = method.Modifiers.ToString();
            _logger.LogInformation($"Property accessibility: {accessibility}");
            var methodInfo = new MethodInfo(methodName, accessibility, TypeSyntaxAnalyzer.GetTypeInfo(returnType));
            foreach (var parameter in method.ParameterList.Parameters)
            {
                var parameterName = parameter.Identifier.Text;
                var parameterType = parameter.Type;
                methodInfo.AddArgument(new MethodArgumentInfo(parameterName, TypeSyntaxAnalyzer.GetTypeInfo(parameterType)));
            }
            objectInformation.AddMethod(methodInfo);
        }
    }

    private IEnumerable<InterfaceInfo> AnalyzeInterfaces(SyntaxNode root)
    {
        foreach (var interfaceNode in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
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
