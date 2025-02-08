using System.Collections.Generic;
using DotUML.CLI.Mermaid;
using DotUML.CLI.Diagram;
using Xunit;

namespace DotUML.Tests;

public class MermaidClassDiagramGeneratorTests
{
    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForSingleClass()
    {
        // Arrange
        var generator = new ClassDiagramGenerator();
        var classInfo = new ClassInfo("TestClass");
        classInfo.AddProperty(new PropertyInfo("TestProperty", "public", (TypeInfo)"string"));
        classInfo.AddMethod(new MethodInfo("TestMethod", "public", (TypeInfo)"void"));
        var namespaceInfo = new NamespaceInfo("TestNamespace");
        namespaceInfo.AddObjectInfo(new List<ObjectInfo> { classInfo });
        var namespaces = new Namespaces(new List<NamespaceInfo> { namespaceInfo });

        // Act
        var result = generator.GenerateDiagram(namespaces);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    namespace TestNamespace {\n        class TestClass {\n            +TestProperty : string\n            +TestMethod()\n        }\n    }\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForClassWithBaseClass()
    {
        // Arrange
        var generator = new ClassDiagramGenerator();
        var classInfo = new ClassInfo("DerivedClass");
        classInfo.Inherits("BaseClass");
        var namespaceInfo = new NamespaceInfo("TestNamespace");
        namespaceInfo.AddObjectInfo(new List<ObjectInfo> { classInfo });
        var namespaces = new Namespaces(new List<NamespaceInfo> { namespaceInfo });

        // Act
        var result = generator.GenerateDiagram(namespaces);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    namespace TestNamespace {\n        class DerivedClass {\n        }\n    }\n    BaseClass <|-- DerivedClass\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForInterface()
    {
        // Arrange
        var generator = new ClassDiagramGenerator();
        var interfaceInfo = new InterfaceInfo("ITestInterface");
        var namespaceInfo = new NamespaceInfo("TestNamespace");
        namespaceInfo.AddObjectInfo(new List<ObjectInfo> { interfaceInfo });
        var namespaces = new Namespaces(new List<NamespaceInfo> { namespaceInfo });

        // Act
        var result = generator.GenerateDiagram(namespaces);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    namespace TestNamespace {\n        class ITestInterface {\n            <<interface>>\n        }\n    }\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForEnum()
    {
        // Arrange
        var generator = new ClassDiagramGenerator();
        var enumInfo = new EnumInfo("TestEnum");
        enumInfo.AddValue("Value1");
        enumInfo.AddValue("Value2");
        var namespaceInfo = new NamespaceInfo("TestNamespace");
        namespaceInfo.AddObjectInfo(new List<ObjectInfo> { enumInfo });
        var namespaces = new Namespaces(new List<NamespaceInfo> { namespaceInfo });

        // Act
        var result = generator.GenerateDiagram(namespaces);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    namespace TestNamespace {\n        class TestEnum {\n            <<enumeration>>\n            Value1\n            Value2\n        }\n    }\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForClassWithDependencies()
    {
        // Arrange
        var generator = new ClassDiagramGenerator();
        var classInfo = new ClassInfo("TestClass");
        classInfo.AddDependency(new DependencyInfo((TypeInfo)"DependencyClass"));
        var namespaceInfo = new NamespaceInfo("TestNamespace");
        namespaceInfo.AddObjectInfo(new List<ObjectInfo> { classInfo });
        var namespaces = new Namespaces(new List<NamespaceInfo> { namespaceInfo });

        // Act
        var result = generator.GenerateDiagram(namespaces);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    namespace TestNamespace {\n        class TestClass {\n        }\n    }\n    TestClass ..> DependencyClass\n```";
        Assert.Equal(expected, result);
    }
}
