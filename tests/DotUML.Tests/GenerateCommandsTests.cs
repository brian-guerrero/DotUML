using Xunit;
using DotUML.CLI;
using DotUML.CLI.Analyzers;
using DotUML.CLI.Mermaid;
using NSubstitute;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace DotUML.Tests.Tests
{
    public class GenerateCommandsTestsTest
    {
        private readonly ILogger<ClassAnalyzer> _classAnalyzerLogger;
        private readonly ClassAnalyzer _classAnalyzer;
        private readonly ILogger<ClassDiagramGenerator> _classDiagramGeneratorLogger;
        private readonly ClassDiagramGenerator _classDiagramGenerator;

        public GenerateCommandsTestsTest()
        {
            _classAnalyzerLogger = Substitute.For<ILogger<ClassAnalyzer>>();
            _classAnalyzer = new ClassAnalyzer(_classAnalyzerLogger);
            _classDiagramGeneratorLogger = Substitute.For<ILogger<ClassDiagramGenerator>>();
            _classDiagramGenerator = new ClassDiagramGenerator(_classDiagramGeneratorLogger);
        }

        [Fact]
        public async Task GenerateCommands_ShouldCreateFile()
        {
            // Arrange
            var generateCommands = new GenerateCommands(_classAnalyzer, _classDiagramGenerator);
            var outputPath = Path.Combine(Path.GetTempPath(), "diagram.md");
            var baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"Base Directory: {baseDirectory}");

            var solution = Path.Combine(baseDirectory, @"..", "..", "..", "..", "..", "test-solutions", "SampleWeb.sln");
            solution = Path.GetFullPath(solution);
            Console.WriteLine($"Solution Path: {solution}");
            Console.WriteLine($"Base Directory: {baseDirectory}");
            // Act
            await generateCommands.Generate(solution, outputPath);

            // Assert
            Assert.True(File.Exists(outputPath));
        }
    }
}