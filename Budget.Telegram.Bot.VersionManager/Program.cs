using System.Xml.Linq;

namespace Budget.Telegram.Bot.VersionManager;

class Program
{
    static void Main()
    {
        // Get the root directory of the solution
        string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.FullName;

        if (solutionDirectory == null)
        {
            Console.WriteLine("Solution directory not found.");
            return;
        }

        Console.WriteLine($"Solution Directory: {solutionDirectory}");

        // Find all .csproj files in the relevant projects
        var projects = Directory.GetFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories)
                                .Where(path => !path.Contains("LMS.VersionManager"))
                                .ToList();

        if (!projects.Any())
        {
            Console.WriteLine("No projects found.");
            return;
        }

        foreach (var project in projects)
        {
            Console.WriteLine($"Updating version for: {Path.GetFileName(project)}");
            UpdateVersion(project);
        }
    }

    static void UpdateVersion(string csprojPath)
    {
        try
        {
            var document = XDocument.Load(csprojPath);
            var versionElement = document.Descendants("Version").FirstOrDefault();

            if (versionElement != null)
            {
                var currentVersion = versionElement.Value;
                var versionParts = currentVersion.Split('.').Select(int.Parse).ToArray();

                // Increment the patch version
                versionParts[2]++;

                // Update the version in the XML
                versionElement.Value = $"{versionParts[0]}.{versionParts[1]}.{versionParts[2]}";

                // Save the updated .csproj file
                document.Save(csprojPath);

                Console.WriteLine($"Updated version to {versionElement.Value} for {Path.GetFileName(csprojPath)}");
            }
            else
            {
                Console.WriteLine($"No <Version> tag found in {csprojPath}. Adding a new one...");

                // Add a new <Version> tag if not present
                var propertyGroup = document.Descendants("PropertyGroup").FirstOrDefault();
                if (propertyGroup != null)
                {
                    propertyGroup.Add(new XElement("Version", "1.0.0"));
                    document.Save(csprojPath);
                    Console.WriteLine($"Added <Version> tag with value 1.0.0 to {Path.GetFileName(csprojPath)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update version for {csprojPath}: {ex.Message}");
        }
    }
}