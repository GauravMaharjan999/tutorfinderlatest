namespace Kachuwa.Dash.Model
{
    public class Stream
    {
        public int index { get; set; }
        public string codec_name { get; set; }
        public string codec_long_name { get; set; }
        public string profile { get; set; }
        public string codec_type { get; set; }
        public string codec_time_base { get; set; }
        public string codec_tag_string { get; set; }
        public string codec_tag { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int coded_width { get; set; }
        public int coded_height { get; set; }
        public int has_b_frames { get; set; }
        public string pix_fmt { get; set; }
        public int level { get; set; }
        public string color_range { get; set; }
        public string color_space { get; set; }
        public string color_transfer { get; set; }
        public string color_primaries { get; set; }
        public string chroma_location { get; set; }
        public int refs { get; set; }
        public string is_avc { get; set; }
        public string nal_length_size { get; set; }
        public string r_frame_rate { get; set; }
        public string avg_frame_rate { get; set; }
        public string time_base { get; set; }
        public int start_pts { get; set; }
        public string start_time { get; set; }
        public int duration_ts { get; set; }
        public string duration { get; set; }
        public string bit_rate { get; set; }
        public string bits_per_raw_sample { get; set; }
        public string nb_frames { get; set; }
        public Disposition disposition { get; set; }
        public Tags tags { get; set; }
        public string sample_fmt { get; set; }
        public string sample_rate { get; set; }
        public int? channels { get; set; }
        public string channel_layout { get; set; }
        public int? bits_per_sample { get; set; }
        public string max_bit_rate { get; set; }
    }
}