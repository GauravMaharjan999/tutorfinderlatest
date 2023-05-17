using System;
using System.ComponentModel;

namespace Kachuwa.Dash.Filters
{
    public static class Extentions
    {
        public static string GetDescription(this X264Preset value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name == null) return null;

            var field = type.GetField(name);

            if (field == null) return null;

            var attr =
                Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attr != null ? attr.Description : null;
        }
        public static string GetDescription(this VP9Quality value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name == null) return null;

            var field = type.GetField(name);

            if (field == null) return null;

            var attr =
                Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attr != null ? attr.Description : null;
        }
        public static string GetDescription(this AudioCodec value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name == null) return null;

            var field = type.GetField(name);

            if (field == null) return null;

            var attr =
                Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attr != null ? attr.Description : null;
        }
        
    }
}