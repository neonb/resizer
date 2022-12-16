using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neonb.Resizer;

internal static class ImageResizer
{
	public static void Resize(IReadOnlyList<string> imagePaths, uint jpegSize, uint pngSize)
	{
		foreach (var path in imagePaths)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(null, path);
			}
		}

		using var imageTool = new ImageTool();

		var images = imageTool.GetInfo(imagePaths);

		var operations = images.Select(image =>
		{
			var resize = image.Format is ImageFormat.Png or ImageFormat.Gif
				? new ResizeInfo(ImageFormat.Png, image.Size.ScaleToArea(pngSize))
				: new ResizeInfo(ImageFormat.Jpeg, image.Size.ScaleToArea(jpegSize));
			return (image.Path, resize);
		});

		imageTool.Downsize(operations);
	}
}
