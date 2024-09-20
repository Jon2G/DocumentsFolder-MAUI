using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Icu.Util;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Systems;
using Android.Text;
using Android.Util;
using AndroidX.Annotations;
using AndroidX.Core.Content;
using DocumentsFolder;
using Java.IO;
using Java.Lang;
using Java.Net;
using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Android.Provider.MediaStore;
using static Android.Telephony.CarrierConfigManager;
using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;
using AndroidApp = Android.App;
using AndroidEnvironment = Android.OS.Environment;
using Debug = System.Diagnostics.Debug;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace DocumentsFolder
{
    public partial class DocumentsDirectoryManager : IDocumentsDirectoryManager
    {


        [RequiresApi(Api = (int)BuildVersionCodes.Q)]
        public DocumentsFile? WriteToFile(
          string fileName,
          string mimeType,
          string extension,
          byte[] data
      )
        {
            try
            {
                var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (context == null)
                {
                    return null;
                }


                Java.IO.InputStream inputStream = new ByteArrayInputStream(data);

                //Java.IO.InputStream inputStream = (
                //    (Android.Runtime.InputStreamInvoker)data
                //).BaseInputStream;
                Uri savedFileUri = WriteToFile(
                      context,
                      inputStream,
                      mimeType,
                      fileName,
                      extension,
                      this.FolderName
                  );
                if (savedFileUri == null)
                {
                    return null;
                }
                File? file = new File(savedFileUri.Path!); //create path from uri
                var fileInfo = new FileInfo(file.AbsolutePath);

                return new DocumentsFile(savedFileUri, fileName, DateTime.Now, data.Length);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        [RequiresApi(Api = (int)BuildVersionCodes.Q)]
        private Uri WriteToFile(
            Context context,
            InputStream inputStream,
            string mimeType,
            string displayName,
            string extension,
            string? subFolder
        )
        {
            string? relativeLocation = Environment.DirectoryDocuments;

            if (!TextUtils.IsEmpty(subFolder))
            {
                relativeLocation += File.Separator + subFolder;
            }

            ContentValues? contentValues = new ContentValues();
            contentValues.Put(MediaStore.IMediaColumns.DisplayName, displayName + "." + extension);
            contentValues.Put(MediaStore.IMediaColumns.MimeType, mimeType);
            contentValues.Put(MediaStore.IMediaColumns.RelativePath, relativeLocation);
            contentValues.Put(MediaStore.IMediaColumns.Title, displayName);
            contentValues.Put(
                MediaStore.IMediaColumns.DateAdded,
                Java.Lang.JavaSystem.CurrentTimeMillis() / 1000
            );
            contentValues.Put(
                MediaStore.IMediaColumns.DateTaken,
                Java.Lang.JavaSystem.CurrentTimeMillis()
            );
            ContentResolver? resolver = context.ContentResolver;
            Stream? stream = null;
            Uri? uri = null;

            try
            {
                Uri? contentUri = MediaStore.Files.GetContentUri(MediaStore.VolumeExternal);
                if (contentUri is null)
                {
                    throw new NullReferenceException();
                }
                uri = resolver?.Insert(contentUri, contentValues);
                if (uri is null)
                {
                    throw new NullReferenceException();
                }
                ParcelFileDescriptor? pfd = null;
                try
                {
                    pfd = context.ContentResolver!.OpenFileDescriptor(uri, "w");
                    if (pfd is null)
                    {
                        throw new NullReferenceException();
                    }
                    FileOutputStream? outputStream = new FileOutputStream(pfd.FileDescriptor);

                    byte[] buf = new byte[4 * 1024];
                    int len;
                    while ((len = inputStream.Read(buf)) > 0)
                    {
                        outputStream.Write(buf, 0, len);
                    }
                    outputStream.Close();
                    inputStream.Close();
                    pfd.Close();
                }
                catch (System.Exception ex)
                {
                    Log.Error("ERROR", ex.ToString());
                }

                contentValues.Clear();
                contentValues.Put(MediaStore.IMediaColumns.IsPending, 0);
                context.ContentResolver!.Update(uri, contentValues, null, null);
                stream = resolver!.OpenOutputStream(uri);
                if (stream == null)
                {
                    throw new System.IO.IOException("Failed to get output stream.");
                }
                return uri;
            }
            catch (System.IO.IOException)
            {
                // Don't leave an orphan entry in the MediaStore
                resolver?.Delete(uri!, null, null);
                throw;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public DocumentsFile[] GetDocumentsFiles()
        {

            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null)
            {
                return [];
            }
            var filesList = new List<DocumentsFile>();

            Uri? collection = MediaStore.Files.GetContentUri(MediaStore.VolumeExternal);

            string[] projection = new string[] {
    MediaStore.IMediaColumns.DocumentId,
    MediaStore.IMediaColumns.DisplayName,
    MediaStore.IMediaColumns.Size,
    MediaStore.IMediaColumns.DateModified
};
            string selection = " 1=1";
            //string selection = MediaStore.IMediaColumns.DisplayName +
            //        " >= ?";
            //        string[] selectionArgs = new string[] {
            //Java.Lang.String.ValueOf(Java.Util.Concurrent.TimeUnit.Milliseconds.Convert(5,Java.Util.Concurrent.TimeUnit.Minutes))
            //    };
            string sortOrder = MediaStore.IMediaColumns.DisplayName + " ASC";
            ContentResolver? resolver = context.ContentResolver;
            var cursor = resolver.Query(
                collection,
                projection,
                selection,
                null,
                sortOrder
            );
            int idColumn = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DocumentId);
            int displayNameColumn = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DisplayName);
            int sizeColumn = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.Size);
            int dateModifiedColumn = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DateModified);

            while (cursor.MoveToNext())
            {
                // Get values of columns for a given file.
                long id = cursor.GetLong(idColumn);
                string name = cursor.GetString(displayNameColumn);
                int size = cursor.GetInt(sizeColumn);
                int dateModified = cursor.GetInt(dateModifiedColumn);

                Uri? contentUri = MediaStore.Files.GetContentUri(MediaStore.VolumeExternal);

                contentUri = ContentUris.WithAppendedId(
                   contentUri, id);

                //// Stores column values and the contentUri in a local object
                //// that represents the media file.
                filesList.Add(new DocumentsFile(contentUri, name, DateTime.FromFileTimeUtc(dateModified), size));
            }




            return filesList.ToArray();
        }

    }
}
