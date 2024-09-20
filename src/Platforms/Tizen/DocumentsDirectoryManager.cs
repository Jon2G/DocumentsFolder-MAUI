using System;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace DocumentsFolder
{
    // All the code in this file is only included on Tizen.
    public partial class DocumentsDirectoryManager : IDocumentsDirectoryManager
    {
        public FileInfo[] GetDocumentsFiles(string? subFolder = null)
        {
            throw new NotImplementedException();
        }
        public FileInfo? WriteToFile(string fileName, string mimeType, string extension, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}