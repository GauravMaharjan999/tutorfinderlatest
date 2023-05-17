using System.ComponentModel;

namespace Kachuwa.Dash.Filters
{
    public enum VP9Quality
    {
        // is the default and recommended for most applications. 
        [Description("good")]
        Good,
        //is recommended if you have lots of time and want the best compression efficiency. 
        [Description("best")]
        Best,
        // is recommended for live / fast encoding. 
        [Description("realtime ")]
        RealTime
    }
}