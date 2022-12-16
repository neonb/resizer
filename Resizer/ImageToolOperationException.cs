using System;

namespace Neonb.Resizer;

public class ImageToolOperationException : Exception
{
	public ImageToolOperationException(string message) : base(message)
	{
	}

	public ImageToolOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
