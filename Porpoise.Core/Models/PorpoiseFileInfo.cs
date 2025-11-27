using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Porpoise.Core.Models
{
    public class PorpoiseFileInfo
    {
        public PFileType FileType { get; set; }
        public bool Exported { get; set; }
        public required string Content { get; set; }
    }

    public enum PFileType
    {
        Binary,
        Text
    }
}
