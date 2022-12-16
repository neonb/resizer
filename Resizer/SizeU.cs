using System.Diagnostics.Contracts;
using System;

namespace Neonb.Resizer;

internal readonly struct SizeU
{
	public readonly uint Width;
	public readonly uint Height;

	public SizeU(uint width, uint height)
	{
		this.Width = width;
		this.Height = height;
	}

	[Pure]
	public SizeU ScaleToArea(uint newArea)
	{
		var scale = Math.Sqrt((double)newArea / (this.Width * this.Height));
		return new SizeU((uint)(scale * this.Width), (uint)(scale * this.Height));
	}
}
