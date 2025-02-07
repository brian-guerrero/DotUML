# DotUML
## DotUML a .NET CLI tool for designing mermaid diagrams from your .NET solutions.

### Features
- Generate Mermaid class diagrams from .NET solutions
- Easy to use CLI interface

### Installation
To install DotUML, run the following command:
```sh
dotnet tool install -g DotUML
```

### Usage
To generate a Mermaid diagram, navigate to your .NET solution directory and run:
```sh
dotuml generate
```

or pass an absolute directory as a parameter:

```sh
dotuml generate C:\\source\\repos\\Solution\\Awesome.sln
```

### Running Tests
To run the unit tests, navigate to the `tests` directory and use the following command:
```sh
dotnet test
```

### Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

### CI/CD Pipeline
This project uses GitHub Actions for continuous integration and continuous deployment (CI/CD). The pipeline is configured to build and test the solution on every push and pull request to the `main` branch.

#### Status Badge
![CI/CD Pipeline](https://github.com/brian-guerrero/DotUML/actions/workflows/dotnet.yml/badge.svg)
