using System;

using DotUML.CLI.Diagram;

using Xunit;

namespace DotUML.Tests.Models;

public class ClassInfoTests
{
    [Fact]
    public void ClassInfo_ShouldSanitizeGenericsInObjectRepresentation()
    {
        // Arrange
        var classInfo = new ClassInfo("Class<Generic>");

        // Act
        var result = classInfo.GetObjectRepresentation();

        // Assert
        Assert.True(result.StartsWith("class Class~Generic~"));
    }
}
