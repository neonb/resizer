using System;

namespace Neonb.Resizer;

public class ImageFormatException : Exception
{
	public string Path { get; }

	public ImageFormatException(string path)
		: base("Unsupported image format.")
	{
		this.Path = path;
	}

	public ImageFormatException(string path, Exception innerException)
		: base("Unsupported image format.", innerException)
	{
		this.Path = path;
	}
}
