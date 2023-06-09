using System.Collections.Generic;
using System.Text;

namespace Kachuwa.Web
{
    public class TitleTag : IMetaTag
    {
        public Dictionary<string, string> MetaKeyValues { get; set; }

        public TitleTag(Dictionary<string, string> metaKeyValues)
        {
            MetaKeyValues = metaKeyValues;
        }

        public string Generate()
        {
            StringBuilder tagBuilder = new StringBuilder();

            foreach (var keyvalue in MetaKeyValues)
            {
                tagBuilder.AppendFormat("<title >{0}</title>", keyvalue.Value);

            }
            return tagBuilder.ToString();
        }

    }
}