using Microsoft.Win32;
using Mono.Options;
using Neonb.Resizer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Neonb.Resizer;

[SupportedOSPlatform("windows")]
public static class ResizerApp
{
	private const string JpegSizeRegistryName = "jpegSize";
	private const string PngSizeRegistryName = "pngSize";
	private const uint JpegSizeDefault = 1_000_000U;
	private const uint PngSizeDefault = 200_000U;

	[STAThread]
	private static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			ShowMessage(Resources.NoArgumentsHelpMessage, Resources.HelpCaption, MessageBoxIcon.Information);
		}
		else
		{
			var jpegSizeOption = (uint?)null;
			var pngSizeOption = (uint?)null;
			var doSave = false;
			var doReset = false;
			var doHelp = false;
			var options = new OptionSet
			{
				{ "jpeg-size=", "The target size for JPEG images, in pixels.", (uint p) => jpegSizeOption = p },
				{ "png-size=", "The target size for PNG images, in pixels.", (uint p) => pngSizeOption = p },
				{ "save-options", "Save given options as defaults for later runs.", p => doSave = p is not null },
				{ "reset-options", "Reset any saved options.", p => doReset = p is not null },
				{ "h|help", "Show this help.", p => doHelp = p is not null }
			};

			List<string> extraArgs;
			try
			{
				extraArgs = options.Parse(args);
			}
			catch (OptionException oex)
			{
				ShowOptionsError(oex);
				return;
			}

			if (doHelp)
			{
				ShowOptionsHelp(options);
				return;
			}

			var userData = Application.UserAppDataRegistry;
			if (doReset)
			{
				userData.DeleteValue(JpegSizeRegistryName, throwOnMissingValue: false);
				userData.DeleteValue(PngSizeRegistryName, throwOnMissingValue: false);
			}

			if (doSave)
			{
				if (jpegSizeOption is uint newJpegSize)
				{
					userData.SetValue(JpegSizeRegistryName, newJpegSize, RegistryValueKind.QWord);
				}

				if (pngSizeOption is uint newPngSize)
				{
					userData.SetValue(PngSizeRegistryName, newPngSize, RegistryValueKind.QWord);
				}
			}

			var jpegSize = jpegSizeOption
				?? (userData.GetValue(JpegSizeRegistryName) is uint savedJpegSize ? savedJpegSize : JpegSizeDefault);

			var pngSize = pngSizeOption
				?? (userData.GetValue(PngSizeRegistryName) is uint savedPngSize ? savedPngSize : PngSizeDefault);

			if (extraArgs.Count > 0)
			{
				try
				{
					ImageResizer.Resize(extraArgs, jpegSize, pngSize);
				}
				catch (FileNotFoundException fnfex)
				{
					var message = Resources.FileNotFoundErrorMessage + fnfex.FileName;
					ShowMessage(message, Resources.ErrorCaption, MessageBoxIcon.Error);
				}
				catch (ImageFormatException ifex)
				{
					var message = Resources.FormatErrorMessage + ifex.Path;
					ShowMessage(message, Resources.ErrorCaption, MessageBoxIcon.Error);
				}
				catch (Exception ex)
				{
					var message = $"{Resources.UnknownErrorMessage}\n{ex.Message}";
					ShowMessage(message, Resources.ErrorCaption, MessageBoxIcon.Error);
				}
			}
		}
	}

	private static void ShowOptionsError(OptionException oex)
	{
		Console.WriteLine($"resizer: {oex.Message}");
		Console.WriteLine("Try `resizer --help` for more information.");
	}

	private static void ShowOptionsHelp(OptionSet options)
	{
		Console.WriteLine("Usage: resizer [OPTIONS]... [FILE]...");
		Console.WriteLine();
		options.WriteOptionDescriptions(Console.Out);
	}

	private static void ShowMessage(string message, string caption, MessageBoxIcon icon) =>
		_ = MessageBox.Show(message, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, 0);
}
