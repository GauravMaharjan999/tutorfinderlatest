using System;

namespace Kachuwa.Dash
{
    public class Class1
    {

        //var currentDir =
        //    new FileInfo(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
        //var appPath = currentDir.DirectoryName;

        //    if (string.IsNullOrWhiteSpace(appPath))
        //    throw new ApplicationException("app path not found.");

        //var inputPath = Path.Combine(appPath, "mero.mp4");
        //var outputPath = Path.Combine(appPath, Guid.NewGuid().ToString());
        //var image = Path.Combine(appPath, "logo.png");

        //Console.WriteLine("start...");

        //Encoder.Create()
        //.WidthInput(inputPath)
        //    .WithFilter(new X264Filter { Preset = X264Preset.Faster, ConstantQuantizer = 18 })
        //.WithFilter(new ImageWatermarkFilter(image, WatermarkPosition.TopRight))
        //    .WithFilter(new ResizeFilter(Resolution.X720P))
        //    .WithFilter(new SnapshotFilter(Path.Combine(appPath, "output", "output.png"), 320, 180, 10))//with snapshot
        //.To<Mp4>(outputPath)
        //.Execute();

        //Network.Create()
        //    .WithSource(inputPath)
        //    .WithDest("rtmp://192.168.10.12/live/stream")
        //    .WithFilter(new X264Filter { ConstantQuantizer = 20 })
        //    .WithFilter(new ResizeFilter(Resolution.X720P))
        //    .Push();

        //Console.WriteLine("done.\npress any key to exit.");

        //Console.ReadKey();
    }
}
