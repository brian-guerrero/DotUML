using System.Collections.Generic;

using DotUML.CLI.Generators;
using DotUML.CLI.Models;

using Xunit;

namespace DotUML.Tests;

public class MermaidClassDiagramGeneratorTests
{
    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForSingleClass()
    {
        // Arrange
        var generator = new MermaidClassDiagramGenerator();
        var classInfo = new ClassInfo("TestClass");
        classInfo.AddProperty(new PropertyInfo("TestProperty", "public", (TypeInfo)"string"));
        classInfo.AddMethod(new MethodInfo("TestMethod", "public", (TypeInfo)"void"));
        var objectInfos = new List<ObjectInfo> { classInfo };

        // Act
        var result = generator.GenerateDiagram(objectInfos);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    class TestClass {\n        +TestProperty : string\n        +TestMethod()\n    }\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForClassWithBaseClass()
    {
        // Arrange
        var generator = new MermaidClassDiagramGenerator();
        var classInfo = new ClassInfo("DerivedClass", "BaseClass");
        var objectInfos = new List<ObjectInfo> { classInfo };

        // Act
        var result = generator.GenerateDiagram(objectInfos);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    class DerivedClass {\n    }\n    BaseClass <|-- DerivedClass\n```";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateDiagram_ShouldReturnCorrectDiagram_ForInterface()
    {
        // Arrange
        var generator = new MermaidClassDiagramGenerator();
        var interfaceInfo = new InterfaceInfo("ITestInterface");
        var objectInfos = new List<ObjectInfo> { interfaceInfo };

        // Act
        var result = generator.GenerateDiagram(objectInfos);

        // Assert
        var expected = "```mermaid\nclassDiagram\n    class ITestInterface {\n        <<interface>>\n    }\n```";
        Assert.Equal(expected, result);
    }
}
