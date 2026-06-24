namespace RocheSimuLink.Core.Tests.Him;

/// <summary>
/// Locates the Host Interface Manual PDF in the repository for HIM tests.
/// Walks up from the test output directory to the repo root.
/// </summary>
internal static class HimTestData
{
    public const string PdfFileName = "HIMv2_1.pdf";

    public static string PdfPath
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, PdfFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }

            throw new FileNotFoundException(
                $"Could not locate {PdfFileName} walking up from {AppContext.BaseDirectory}.");
        }
    }

    public static bool PdfExists
    {
        get
        {
            try
            {
                _ = PdfPath;
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }
    }
}
