namespace DevtiPosLite.UI.Helpers;

public static class ImageHelper
{
    private static string? _basePath;

    public static void Initialize(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "products"));
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "denominations"));
        Directory.CreateDirectory(Path.Combine(_basePath, "images", "logos"));
    }

    public static string SaveImage(string sourcePath, string subfolder)
    {
        if (_basePath == null || string.IsNullOrWhiteSpace(sourcePath))
            return string.Empty;

        var ext = Path.GetExtension(sourcePath);
        var destDir = Path.Combine(_basePath, "images", subfolder);
        Directory.CreateDirectory(destDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var destPath = Path.Combine(destDir, fileName);
        File.Copy(sourcePath, destPath, true);
        return Path.Combine("images", subfolder, fileName);
    }

    public static void DeleteImage(string? relativePath)
    {
        if (_basePath == null || string.IsNullOrWhiteSpace(relativePath))
            return;
        var fullPath = Path.Combine(_basePath, relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public static string? ResolvePath(string? relativePath)
    {
        if (_basePath == null || string.IsNullOrWhiteSpace(relativePath))
            return null;
        var fullPath = Path.Combine(_basePath, relativePath);
        return File.Exists(fullPath) ? fullPath : null;
    }

    public static Size FitSize(int width, int height, int maxWidth, int maxHeight)
    {
        if (width <= 0 || height <= 0) return new Size(maxWidth, maxHeight);
        var scale = Math.Min(1.0, Math.Min((double)maxWidth / width, (double)maxHeight / height));
        return new Size(Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
    }

    public static Image? LoadImage(string? relativePath, int maxWidth = 120, int maxHeight = 120)
    {
        var fullPath = ResolvePath(relativePath);
        if (fullPath == null) return null;
        try
        {
            var img = Image.FromFile(fullPath);
            var ratio = Math.Min((double)maxWidth / img.Width, (double)maxHeight / img.Height);
            if (ratio >= 1) return img;
            var w = (int)(img.Width * ratio);
            var h = (int)(img.Height * ratio);
            var thumb = new Bitmap(w, h);
            using var g = Graphics.FromImage(thumb);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, w, h);
            img.Dispose();
            return thumb;
        }
        catch { return null; }
    }
}
