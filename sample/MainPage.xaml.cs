using DocumentsFolder;
using System.ComponentModel;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SaveCommand { get; set; }
        public string Text { get; set; } = "Hello world";

        public MainPageViewModel()
        {
            SaveCommand = new Command(Save);
        }

        public void Save()
        {
            string appName = "MySampleApp"; //will be used as folder name or null to use default app name
            if (!string.IsNullOrEmpty(Text))
            {
                DocumentsDirectoryManager mediaDirectory = new (folderName: appName);
                var files = mediaDirectory.GetDocumentsFiles();
                DocumentsFile? file = mediaDirectory.WriteToFile(fileName: "myFileName", MediaTypeNames.Text.Plain, "txt", Text);
                if(file is not null)
                {
                }
            }
        }
    }
}