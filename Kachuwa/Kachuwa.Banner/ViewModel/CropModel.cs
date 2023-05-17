namespace Kachuwa.Banner
{
    public class CropModel
    {
        public string imagePath { get; set; }
        public int? cropPointX { get; set; }
        public int? cropPointY { get; set; }
        public int? imageCropWidth { get; set; }
        public int? imageCropHeight { get; set; }

    }
}