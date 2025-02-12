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
        private readonly ILogger<MarkdownDiagramGenerator> _classDiagramGeneratorLogger;
        private readonly MarkdownDiagramGenerator _classDiagramGenerator;
        private readonly ILogger<ImageDiagramGenerator> _imageDiagramGeneratorLogger;
        private readonly ImageDiagramGenerator _imageDiagramGenerator;
        private readonly GenerateCommands _generateCommands;

        public GenerateCommandsTestsTest()
        {
            _classAnalyzerLogger = Substitute.For<ILogger<ClassAnalyzer>>();
            _classAnalyzer = new ClassAnalyzer(_classAnalyzerLogger);
            _classDiagramGeneratorLogger = Substitute.For<ILogger<MarkdownDiagramGenerator>>();
            _classDiagramGenerator = new MarkdownDiagramGenerator(_classDiagramGeneratorLogger);
            _imageDiagramGeneratorLogger = Substitute.For<ILogger<ImageDiagramGenerator>>();
            _imageDiagramGenerator = new ImageDiagramGenerator(_imageDiagramGeneratorLogger);
            _generateCommands = new GenerateCommands(_classAnalyzer, [_classDiagramGenerator, _imageDiagramGenerator]);
        }

        [Fact]
        public async Task GenerateCommands_ShouldCreateMarkdownFile()
        {
            // Arrange
            var outputPath = Path.Combine(Path.GetTempPath(), "diagram.md");
            var baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"Base Directory: {baseDirectory}");

            var solution = Path.Combine(baseDirectory, @"..", "..", "..", "..", "..", "test-solutions", "SampleWeb.sln");
            solution = Path.GetFullPath(solution);
            Console.WriteLine($"Solution Path: {solution}");
            Console.WriteLine($"Base Directory: {baseDirectory}");
            // Act
            await _generateCommands.Generate(solution, outputPath, OutputType.Markdown);

            // Assert
            Assert.True(File.Exists(outputPath));
        }

        [Fact]
        public async Task GenerateCommands_ShouldCreatePngFile()
        {
            // Arrange
            var outputPath = Path.Combine(Path.GetTempPath(), "diagram.png");
            var baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"Base Directory: {baseDirectory}");

            var solution = Path.Combine(baseDirectory, @"..", "..", "..", "..", "..", "test-solutions", "SampleWeb.sln");
            solution = Path.GetFullPath(solution);
            Console.WriteLine($"Solution Path: {solution}");
            Console.WriteLine($"Base Directory: {baseDirectory}");
            // Act
            await _generateCommands.Generate(solution, outputPath, OutputType.Image);

            // Assert
            Assert.True(File.Exists(outputPath));
        }
    }
}