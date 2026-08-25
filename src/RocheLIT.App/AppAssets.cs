namespace RocheLIT;

internal static class AppAssets
{
    public static Image? LoadImage(string fileName)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path))
            {
                return null;
            }

            // Copy into memory so the file isn't locked for the app's lifetime.
            using var stream = File.OpenRead(path);
            return Image.FromStream(stream);
        }
        catch
        {
            return null;
        }
    }
}
