using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    [Serializable]
    public class FileMessage : Message 
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool IsMine { get; set; }

        [JsonIgnore]
        public string LocalFilePath { get; set; }
        public string FormatSizeFile
        {
            get {
                if (FileSize == 0) return "0 bytes";

                const long KB = 1024;
                const long MB = KB * 1024;
                const long GB = MB * 1024;

                string originalBytes = $"{FileSize:N0} bytes";

                if (FileSize >= GB)
                {
                    double result = (double)FileSize / GB;
                    return $"{result:F2} GB";
                }
                else if (FileSize >= MB)
                {
                    double result = (double)FileSize / MB;
                    return $"{result:F2} MB";
                }
                else if(FileSize >= KB)
                {
                    double result = (double)FileSize / KB;
                    return $"{result:F2} KB";
                }
                else
                {
                    return originalBytes;
                }
            }
        }
        public FileMessage()
        {
            Type = MessageType.File;
        }
    }
}
