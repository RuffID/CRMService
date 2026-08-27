namespace CRMService.Architecture.Tests;

internal static class RepositoryRoot
{
    internal static string Find()
    {
        string[] startingPaths =
        [
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        ];

        foreach (string startingPath in startingPaths)
        {
            DirectoryInfo? directory = new(startingPath);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "CRMService.Domain", "CRMService.Domain.csproj"))
                    && File.Exists(Path.Combine(
                        directory.FullName,
                        "tests",
                        "CRMService.Architecture.Tests",
                        "CRMService.Architecture.Tests.csproj")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("CRMService repository root was not found.");
    }
}
