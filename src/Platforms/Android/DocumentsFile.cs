using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsFolder
{
    public partial class DocumentsFile
    {
        private readonly Android.Net.Uri contentUri;
        private readonly string? name;
        private readonly DateTime dateModified;
        private readonly int size;

        public DocumentsFile(Android.Net.Uri contentUri, string? name, DateTime dateModified, int size)
        {
            this.contentUri = contentUri;
            this.name = name;
            this.dateModified = dateModified;
            this.size = size;
        }
    }
}
