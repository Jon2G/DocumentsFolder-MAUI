using System.Text;

namespace DocumentsFolder
{
    public interface IDocumentsDirectoryManager
    {
        DocumentsFile? WriteToFile(
            string fileName,
            string mimeType,
            string extension,
            byte[] data
        );
        DocumentsFile? WriteToFile(
            string fileName,
            string mimeType,
            string extension,
            string data,
            Encoding? encoding = null
        );
        DocumentsFile? WriteToFile(
            string fileName,
            string mimeType,
            string extension,
            Stream data
        );
        DocumentsFile[] GetDocumentsFiles();
    }
    // All the code in this file is included in all platforms.
    public partial class DocumentsDirectoryManager
    {
        internal readonly string? FolderName;

        public DocumentsDirectoryManager(string? folderName = null)
        {
            this.FolderName = folderName?? AppInfo.Current.Name;
        }

        public DocumentsFile? WriteToFile(
            string fileName,
            string mimeType,
            string extension,
            Stream data
        )
        {
            return WriteToFile(fileName, mimeType, extension, data: StreamToByte(data));
        }

        private static byte[] StreamToByte(Stream data)
        {
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;
            while ((read = data.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        public DocumentsFile? WriteToFile(
            string fileName,
            string mimeType,
            string extension,
            string data,
           Encoding? encoding = null
        )
        {
            DocumentsFile? result;
            byte[]? bytes = (encoding ?? Encoding.ASCII).GetBytes(data);
            result = this.WriteToFile(fileName, mimeType, extension, data: bytes);
            return result;
        }
    }
}
