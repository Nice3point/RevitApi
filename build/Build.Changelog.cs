using System.Text;

sealed partial class Build
{
    string CreateNugetChangelog()
    {
        Assert.True(File.Exists(ChangeLogPath), $"Unable to locate the changelog file: {ChangeLogPath}");
        Log.Information("Changelog: {Path}", ChangeLogPath);

        var changelog = BuildChangelog();
        Assert.True(changelog.Length > 0, $"No version entry exists in the changelog: {ReleaseVersion}");

        return EscapeMsBuild(changelog.ToString());
    }

    string CreateGithubChangelog()
    {
        Assert.True(File.Exists(ChangeLogPath), $"Unable to locate the changelog file: {ChangeLogPath}");
        Log.Information("Changelog: {Path}", ChangeLogPath);

        var changelog = BuildChangelog();
        Assert.True(changelog.Length > 0, $"No version entry exists in the changelog: {ReleaseVersion}");

        return changelog.ToString();
    }

    StringBuilder BuildChangelog()
    {
        const string separator = "# ";

        var hasEntry = false;
        var changelog = new StringBuilder();
        foreach (var line in File.ReadLines(ChangeLogPath))
        {
            if (hasEntry)
            {
                if (line.StartsWith(separator)) break;

                changelog.AppendLine(line);
                continue;
            }

            if (line.StartsWith(separator) && line.Contains(ReleaseVersion))
            {
                hasEntry = true;
            }
        }

        TrimEmptyLines(changelog);
        return changelog;
    }

    static void TrimEmptyLines(StringBuilder builder)
    {
        if (builder.Length == 0) return;

        while (builder[^1] == '\r' || builder[^1] == '\n')
        {
            builder.Remove(builder.Length - 1, 1);
        }

        while (builder[0] == '\r' || builder[0] == '\n')
        {
            builder.Remove(0, 1);
        }
    }

    static string EscapeMsBuild(string value)
    {
        return value
            .Replace(";", "%3B")
            .Replace(",", "%2C");
    }
}