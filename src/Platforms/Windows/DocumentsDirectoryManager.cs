
namespace DocumentsFolder
{
    // All the code in this file is only included on Windows.
    public partial class DocumentsDirectoryManager : IDocumentsDirectoryManager
    {
        public DocumentsFile[] GetDocumentsFiles()
        {
            throw new NotImplementedException();
        }
        public DocumentsFile? WriteToFile(string fileName, string mimeType, string extension, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
