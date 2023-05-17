using System;

namespace Kachuwa.Dash.Model
{
    public class EncodingException : Exception
    {
        public int Code { get; }
        public EncodingException(int code, string msg) : base(msg)
        {
            this.Code = code;
        }
    }
}