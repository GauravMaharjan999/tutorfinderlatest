using System.Collections.Generic;

namespace Kachuwa.Dash.Model
{
    public class VideoFileInfo
    {
        public List<Stream> streams { get; set; }
        public Format format { get; set; }
    }
}