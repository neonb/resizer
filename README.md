# Image resizer

A wrapper around [NConvert](https://www.xnview.com/en/nconvert/) that you can use via drag and drop.
Downscales images to a size suitable for sending over the internet. Only works on Windows.

Works by embedding an NConvert executable at build time and extracting it at runtime.
No XnView property is included in this repository. You must put your own copy of the `nconvert.exe` file
in the `Resizer/Resources/` directory to successfully build this app.
**Make sure to follow XnView's licence terms when using or distributing their software.**

Tested to work with the v6.88 version of NConvert.

## Usage

- Drag and drop one or multiple images on the `resizer` executable to downscale them.
- Configure settings by running the app on command line.
- Run `resizer --help` to see available options.
