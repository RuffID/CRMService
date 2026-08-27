using System.Xml.Linq;
using Xunit;

namespace CRMService.Architecture.Tests;

public class ProjectReferenceArchitectureTests
{
    private static readonly IReadOnlyDictionary<string, string> PROJECT_PATHS =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CRMService.Domain"] = Path.Combine("CRMService.Domain", "CRMService.Domain.csproj"),
            ["CRMService.Contracts"] = Path.Combine("CRMService.Contracts", "CRMService.Contracts.csproj"),
            ["CRMService.Application"] = Path.Combine("CRMService.Application", "CRMService.Application.csproj"),
            ["CRMService.Infrastructure"] = Path.Combine("CRMService.Infrastructure", "CRMService.Infrastructure.csproj"),
            ["CRMService.Web"] = Path.Combine("CRMService.Web", "CRMService.csproj")
        };

    [Fact]
    public void ProductionProjects_ProjectReferences_RespectLayerDirection()
    {
        IReadOnlyDictionary<string, HashSet<string>> references = ReadProductionProjectReferences();

        Assert.Empty(references["CRMService.Domain"]);
        Assert.DoesNotContain("CRMService.Application", references["CRMService.Contracts"]);
        Assert.DoesNotContain("CRMService.Infrastructure", references["CRMService.Contracts"]);
        Assert.DoesNotContain("CRMService.Web", references["CRMService.Contracts"]);
        Assert.DoesNotContain("CRMService.Infrastructure", references["CRMService.Application"]);
        Assert.DoesNotContain("CRMService.Web", references["CRMService.Application"]);
        Assert.DoesNotContain("CRMService.Web", references["CRMService.Infrastructure"]);
    }

    [Fact]
    public void Web_ProjectReferences_AllProductionLayers_AsCompositionRoot()
    {
        IReadOnlyDictionary<string, HashSet<string>> references = ReadProductionProjectReferences();

        Assert.Equal(
            new HashSet<string>(
                [
                    "CRMService.Domain",
                    "CRMService.Contracts",
                    "CRMService.Application",
                    "CRMService.Infrastructure"
                ],
                StringComparer.OrdinalIgnoreCase),
            references["CRMService.Web"]);

        Assert.DoesNotContain(
            references.Where(pair => !string.Equals(pair.Key, "CRMService.Web", StringComparison.OrdinalIgnoreCase)),
            pair => pair.Value.Contains("CRMService.Web"));
    }

    private static IReadOnlyDictionary<string, HashSet<string>> ReadProductionProjectReferences()
    {
        string repositoryRoot = RepositoryRoot.Find();
        Dictionary<string, string> projectNamesByPath = PROJECT_PATHS.ToDictionary(
            pair => ResolveProjectPath(repositoryRoot, pair.Value),
            pair => pair.Key,
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, HashSet<string>> result = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string projectName, string relativePath) in PROJECT_PATHS)
        {
            string projectPath = ResolveProjectPath(repositoryRoot, relativePath);
            string projectDirectory = Path.GetDirectoryName(projectPath)
                ?? throw new InvalidOperationException($"Project directory was not found for {projectPath}.");
            XDocument project = XDocument.Load(projectPath);
            HashSet<string> references = project
                .Descendants("ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => ResolveProjectPath(projectDirectory, include!))
                .Where(projectNamesByPath.ContainsKey)
                .Select(path => projectNamesByPath[path])
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            result.Add(projectName, references);
        }

        return result;
    }

    private static string ResolveProjectPath(string basePath, string path)
    {
        string platformPath = path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(basePath, platformPath));
    }
}
