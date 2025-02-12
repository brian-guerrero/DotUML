# DotUML
## DotUML a .NET CLI tool for designing mermaid diagrams from your .NET solutions.

![CI/CD .NET Build and Test Pipeline](https://github.com/brian-guerrero/DotUML/actions/workflows/dotnet.yml/badge.svg)
![CI/CD .NET Pack and Publish Tool Pipeline](https://github.com/brian-guerrero/DotUML/actions/workflows/publish.yml/badge.svg)


### Features
- Generate Mermaid class diagrams from .NET solutions
- Easy to use CLI interface

### Installation
To install DotUML, run the following command:
```sh
dotnet tool install -g DotUML
```

### Usage

#### Help 
For help run:

```sh
dotuml -h
```

or 

```sh
dotuml --help
```

#### UML
To generate a UML diagram, pass a solution as a parameter:

```sh
dotuml generate -s C:\source\repos\Solution\Awesome.sln
```

or 

```sh
dotuml generate --solution C:\source\repos\Solution\Awesome.sln
```

The following options are available on the method.


| Option                  | Description                                                                                                                                               |
|-------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-s` `--solution`       | Solution to analyze and generate UML diagram for. (Required)                                                                                              |
| `-f` `--format`         | Output type for the diagram. Options: markdown, image (Default: Markdown)                                                                                  |
| `-o` `--output-file`    | Target location for UML file output. If a location is not provided, then a filename including a timestamp will be created in the current directory. (Default: `@"diagramyyyyMMddHHmmss.md"`) |

### Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

### Running Tests
To run the unit tests, navigate to the `tests` directory and use the following command:
```sh
dotnet test
```

### CI/CD Pipeline
This project uses GitHub Actions for continuous integration and continuous deployment (CI/CD). The pipeline is configured to build and test the solution on every push and pull request to the `main` branch.
