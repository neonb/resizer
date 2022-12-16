using System;

namespace Neonb.Resizer;

internal static class ImageFormatExtensions
{
	public static string GetExtension(this ImageFormat imageFormat) => imageFormat switch
	{
		ImageFormat.Jpeg => "jpg",
		ImageFormat.Png => "png",
		ImageFormat.Gif => "gif",
		ImageFormat.Other => throw new InvalidOperationException("Can't get extension for Other format."),
		_ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, "Unexpected value."),
	};

	public static ImageFormat ParseString(string formatName)
	{
		if (formatName.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
			formatName.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return ImageFormat.Jpeg;
		}
		else if (formatName.Equals("png", StringComparison.OrdinalIgnoreCase))
		{
			return ImageFormat.Png;
		}
		else if (formatName.Equals("gif", StringComparison.OrdinalIgnoreCase))
		{
			return ImageFormat.Gif;
		}
		else
		{
			return ImageFormat.Other;
		}
	}
}
