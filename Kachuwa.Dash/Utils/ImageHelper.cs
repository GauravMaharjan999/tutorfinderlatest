//using System;
//using System.Drawing;

//namespace Kachuwa.Dash.Utils
//{
//    //how to call?
//    //Rectangle srcRect = new Rectangle(overlays[i].Offset.X, overlays[i].Offset.Y, overlays[i].Size.Width, overlays[i].Size.Height);
//    //Bitmap card = (Bitmap)bitmapFromImage.Clone(srcRect, bitmapFromImage.PixelFormat);
//    ////cutting rect from image and getting ready to save it as a ellipse or rect.
//    //var pixellated = ImageHelper.Pixelate(card, PixelateSize);
//    //pixellated.Save(overlays[i].Path, System.Drawing.Imaging.ImageFormat.Png);
//    public class ImageHelper
//    {
//        public static Bitmap Pixelate(Bitmap image, Rectangle rectangle, Int32 pixelateSize)
//        {
//            Bitmap pixelated = new System.Drawing.Bitmap(image.Width, image.Height);

//            // make an exact copy of the bitmap provided
//            using (Graphics graphics = System.Drawing.Graphics.FromImage(pixelated))
//                graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
//                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

//            // look at every pixel in the rectangle while making sure we're within the image bounds
//            for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx += pixelateSize)
//            {
//                for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy += pixelateSize)
//                {
//                    Int32 offsetX = pixelateSize / 2;
//                    Int32 offsetY = pixelateSize / 2;

//                    // make sure that the offset is within the boundry of the image
//                    while (xx + offsetX >= image.Width) offsetX--;
//                    while (yy + offsetY >= image.Height) offsetY--;

//                    // get the pixel color in the center of the soon to be pixelated area
//                    var pixel = pixelated.GetPixel(xx + offsetX, yy + offsetY);

//                    // for each pixel in the pixelate size, set it to the center color
//                    for (Int32 x = xx; x < xx + pixelateSize && x < image.Width; x++)
//                        for (Int32 y = yy; y < yy + pixelateSize && y < image.Height; y++)
//                            pixelated.SetPixel(x, y, pixel);
//                }
//            }

//            return pixelated;
//        }
//        public static Bitmap Pixelate(Bitmap image, Int32 blurSize)
//        {
//            return Pixelate(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
//        }
//    }
//}