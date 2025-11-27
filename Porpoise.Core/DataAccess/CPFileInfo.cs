using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Porpoise.Core.DataAccess
{
    public class CPFileInfo
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

