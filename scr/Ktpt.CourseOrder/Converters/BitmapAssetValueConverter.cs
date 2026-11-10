using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Ktpt.CourseOrder.Converters;

public class BitmapAssetValueConverter : IValueConverter
{
    public static BitmapAssetValueConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string rawPath = value as string ?? "no-photo.png";
        string cleanPath = rawPath.TrimStart('/', '\\');

        var uri = new Uri($"avares://Ktpt.CourseOrder/Assets/{cleanPath}");

        if (AssetLoader.Exists(uri))
        {
            return new Bitmap(AssetLoader.Open(uri));
        }

        var fallbackUri = new Uri("avares://Ktpt.CourseOrder/Assets/no-photo.png");
        return AssetLoader.Exists(fallbackUri) ? new Bitmap(AssetLoader.Open(fallbackUri)) : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}