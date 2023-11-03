#r "System.IO"
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*
Usage example:
  husky run -n update-version -a build
  husky run -n update-version -a minor
  husky run -n update-version -a major
  husky run -n update-version -a MinorPreview
  husky run -n update-version -a 3.5.1
*/

var paths = new[] {
    @"root.props",
    @"docs\.vuepress\configs\version.ts"
};

private string customVersion = null;
private IncrementMode incrementMode = IncrementMode.Build;

var arg = Args[0];

if (Enum.TryParse<IncrementMode>(arg, true, out var mode))
   incrementMode = mode;
else if (IsValidVersion(arg))
   customVersion = arg;
else
{
   Console.ForegroundColor = ConsoleColor.Red;
   Console.WriteLine("Invalid argument: " + arg);
   Console.WriteLine("Valid arguments: Major, Minor, Build, MajorPreview, MinorPreview, BuildPreview or a Custom version");
   Console.ResetColor();
   return 1;
}

foreach(var path in paths)
{
    var fullPath = Path.GetFullPath(path);

    var content = await File.ReadAllTextAsync(fullPath);
    const string pattern =  @"<Version>(\d+(\.\d+)*(-\w+)?)<\/Version>";
    var regex = new Regex(pattern, RegexOptions.Compiled);
    Match match = regex.Match(content);

    if (!match.Success)
    {
        // Checking TypeScript files in the docs
        regex = new Regex(@"export\s+const\s+version:\s+string\s*=\s*'([^']+)'");
        match = regex.Match(content);

        if (!match.Success)
            throw new Exception("Version not found!");
    }

    var currentVersion = match.Groups[1].Value;
    var newVersion = !string.IsNullOrEmpty(customVersion) ? customVersion : IncrementVersion(currentVersion, incrementMode);

    var newContent = ReplaceVersion(regex, content, newVersion);
    await File.WriteAllTextAsync(fullPath, newContent);

    Console.WriteLine($"{path} updated to version {newVersion}");
}

// ------------------ Methods ---------------------------

string ReplaceVersion(Regex regex, string content, string newVersion)
{
    Match match = regex.Match(content);

    if (match.Success)
    {
        string originalValue = match.Value;
        string newValue = match.Value.Replace(match.Groups[1].Value, newVersion);

        return content.Substring(0, match.Index) + newValue + content.Substring(match.Index + match.Length);
    }

    return content;
}

string IncrementVersion(string currentVersion, IncrementMode incrementMode)
{
    const string previewPattern = @"-preview(\d+)$";
    Match previewMatch = Regex.Match(currentVersion, previewPattern);
    var previewNumber = 0;
    if (previewMatch.Success)
    {
        previewNumber = int.Parse(previewMatch.Groups[1].Value);
        currentVersion = Regex.Replace(currentVersion, previewPattern, "");
    }
    var version = new Version(currentVersion);
    return incrementMode switch
    {
        IncrementMode.Major => new Version(version.Major + 1, 0, 0).ToString(),
        IncrementMode.Minor => new Version(version.Major, version.Minor + 1, 0).ToString(),
        IncrementMode.Build => new Version(version.Major, version.Minor, version.Build + 1).ToString(),
        IncrementMode.MajorPreview when !previewMatch.Success => new Version(version.Major + 1, 0, 0).ToString() + "-preview" + ++previewNumber,
        IncrementMode.MinorPreview when !previewMatch.Success => version.Major.ToString() + "." + (version.Minor + 1).ToString() + ".0-preview" + ++previewNumber,
        IncrementMode.BuildPreview when !previewMatch.Success => version.Major.ToString() + "." + version.Minor.ToString() + "." + (version.Build + 1).ToString() + "-preview" + ++previewNumber,
        IncrementMode.MajorPreview or IncrementMode.MinorPreview or IncrementMode.BuildPreview when previewMatch.Success
            =>  currentVersion + "-preview" + ++previewNumber,
        _ => throw new ArgumentOutOfRangeException(nameof(incrementMode), incrementMode, null)
    };
}

bool IsValidVersion(string customVersion)
{
    string versionPattern = @"^\d+\.\d+\.\d+(-\w+)?$";
    return Regex.IsMatch(customVersion, versionPattern);
}


public enum IncrementMode
{
    Major,
    Minor,
    Build,
    MajorPreview,
    MinorPreview,
    BuildPreview
}
