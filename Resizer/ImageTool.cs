using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neonb.Resizer;

internal class ImageTool : IDisposable
{
	private readonly string toolPath;

	public ImageTool()
	{
		var toolStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Neonb.Resizer.Resources.nconvert.exe");
		if (toolStream is null)
		{
			throw new IOException("Couldn't access nconvert as a resource.");
		}
		var bytes = new byte[toolStream.Length];
		_ = toolStream.Read(bytes, 0, bytes.Length);
		this.toolPath = Path.GetTempFileName();
		File.WriteAllBytes(this.toolPath, bytes);
	}

	public void Dispose()
	{
		File.Delete(this.toolPath);
	}

	public IReadOnlyList<ImageInfo> GetInfo(IReadOnlyList<string> paths)
	{
		var pathsInQuotes = paths.Select(path => $"\"{path}\"");
		var pathsString = string.Join(" ", pathsInQuotes);
		var startInfo = new ProcessStartInfo
		{
			FileName = this.toolPath,
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			Arguments = $"-info {pathsString}"
		};

		var outputPerPath = paths.ToDictionary(path => path, _ => new List<string>());
		string? latestPath = null;
		void HandleOutput(object sender, DataReceivedEventArgs outputEvent)
		{
			if (outputEvent.Data is null)
			{
				return;
			}

			latestPath = paths.FirstOrDefault(path => outputEvent.Data.StartsWith(path, StringComparison.OrdinalIgnoreCase))
				?? latestPath;

			if (latestPath != null)
			{
				outputPerPath[latestPath].Add(outputEvent.Data);
			}
		}

		using (var toolProc = new Process())
		{
			toolProc.StartInfo = startInfo;
			toolProc.OutputDataReceived += HandleOutput;
			_ = toolProc.Start();
			toolProc.BeginOutputReadLine();
			toolProc.WaitForExit();
		}

		return paths
			.Select(path =>
			{
				var lines = outputPerPath[path];
				var widthLine = lines.FirstOrDefault(line => line.TrimStart().StartsWith("Width", StringComparison.Ordinal));
				if (widthLine == null)
				{
					throw new ImageFormatException(path);
				}

				var widthString = widthLine[(widthLine.LastIndexOf(':') + 2)..];
				var width = uint.Parse(widthString, CultureInfo.InvariantCulture);
				var heightLine = lines.First(line => line.TrimStart().StartsWith("Height", StringComparison.Ordinal));
				var heightString = heightLine[(heightLine.LastIndexOf(':') + 2)..];
				var height = uint.Parse(heightString, CultureInfo.InvariantCulture);
				var formatLine = lines.First(line => line.TrimStart().StartsWith("Name", StringComparison.Ordinal));
				var formatName = formatLine[(formatLine.LastIndexOf(':') + 2)..];
				var format = ImageFormatExtensions.ParseString(formatName);
				return new ImageInfo(path, format, new SizeU(width, height));
			})
			.ToArray();
	}

	public void Downsize(IEnumerable<(string path, ResizeInfo resize)> operations)
	{
		var groups = operations.GroupBy(operation => operation.resize, operation => operation.path);

		foreach (var operationGroup in groups)
		{
			var (resize, paths) = operationGroup;
			var pathsInQuotes = paths.Select(path => $"\"{path}\"");
			var pathsString = string.Join(" ", pathsInQuotes);

			var qualityArg = resize.Format == ImageFormat.Png ? "-clevel 9" : "-q 90";
			var argString = $"-out {resize.Format.GetExtension()} -ratio -rflag decr " +
				$"-resize {resize.Size.Width} {resize.Size.Height} " +
				$"{qualityArg} -o \"resized_%.{resize.Format.GetExtension()}\" {pathsString}";

			this.RunTool(argString);
		}
	}

	private void RunTool(string argString)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = this.toolPath,
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardError = true,
			Arguments = argString
		};

		using var toolProc = new Process();
		toolProc.StartInfo = startInfo;
		_ = toolProc.Start();
		var error = toolProc.StandardError.ReadToEnd();
		toolProc.WaitForExit();

		if (!string.IsNullOrWhiteSpace(error))
		{
			throw new ImageToolOperationException(error);
		}
	}
}
