using System.Collections.Generic;

namespace Kachuwa.FileUploader.Model
{
    public class FileUploadSetting
    {
        public string UniqueId { get; set; }
        public bool AllowMultiple { get; set; } = false;
        public string AcceptFile { get; set; } = "*";
        public string ExtraDataFieldName { get; set; } = "";
        public object ExtraDataFieldValue { get; set; } = null;
        public int MaxFile { get; set; } = 1;
        public string ClassName { get; set; } = "";
        public bool AutoUpload { get; set; } = true;
        public string[] IgnoreFiles { get; set; }=new string[]{};
        public string UploadDir { get; set; } = "FU";
        public List<string> ExistingFiles { get; set; }=new List<string>();


    }
}