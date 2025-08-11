using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using WPFBMI.Models;
using WPFBMI.Pages;
using static SQLite.SQLite3;

namespace WPFBMI
{
   
    public partial class MainWindow : Window
    {
        
        public string defaultFilePath;
        public MainWindow()
        {
            defaultFilePath = AppDomain.CurrentDomain.BaseDirectory;
            InitializeComponent();
            EFData.ApplyMigrations();
            DefaultPage page = new DefaultPage();
            MainFrame.Navigate(page);
        }

        public async void UpdateSubPages()
        {
            if (MainFrame.Content is DefaultPage defaultPage)
            {
                defaultPage.UpdatePage();
            }
            if (MainFrame.Content is AssignRootTracksPage tracksPage)
            {
                tracksPage.RefreshContent();
            }
        }

        public async void LoadCSVHandler(object sender, RoutedEventArgs e)
        {
            string fileLocation = await FileSelectionWindow();
            if (fileLocation != null && fileLocation != "")
            {
                string answer = await CSVParsing.ProcessStatementCsv(fileLocation);
                var result = MessageBox.Show(answer, "CSV Response");
                if (result == MessageBoxResult.OK || result == MessageBoxResult.None) { UpdateSubPages(); }
            }
        }

        public async void LoadCSVFolder(object sender, EventArgs e)
        {
            string folderPath = await FolderSelectionWindow();
            if (folderPath != null && folderPath != "")
            {
                string answer = await CSVParsing.ParseFolderCSV(folderPath);
                var result = MessageBox.Show(answer, "CSV Response");
                if (result == MessageBoxResult.OK || result == MessageBoxResult.None) { UpdateSubPages(); }
            }
        }

        public async void LoadMetaHandler(object sender, EventArgs e)
        {
            string fileLocation = await FileSelectionWindow();
            if (fileLocation != null && fileLocation != "")
            {
                string answer = await CSVParsing.ParseMetadataCSV(fileLocation);
                var result = MessageBox.Show(answer, "Metadata Response");
                if (result == MessageBoxResult.OK || result == MessageBoxResult.None) { UpdateSubPages(); }
            }
        }

        public async Task<string> FileSelectionWindow()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "Csv files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                return filePath;
            } else
            {
                return null;
            }
        }

        public async Task<string> FolderSelectionWindow()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                return folderPath;
            } else
            {
                return null;
            }
        }

        public async void CheckPairings(object sender, EventArgs e)
        {
            var result = MessageBox.Show(await EFData.CheckLibrary());
            if (result == MessageBoxResult.OK || result == MessageBoxResult.None) { UpdateSubPages(); }
        }

        public async void RemoveRootTrackPairings(object sender, EventArgs e)
        {
            await EFData.RemoveAllRootAssignments();
            var result = MessageBox.Show("Completed: All Root Assignments Cleared", "Task Completed");
            if (result == MessageBoxResult.OK || result == MessageBoxResult.None) { UpdateSubPages(); }
        }

        public async void EditMetadataHandler(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("./Pages/EditMetadataPage.xaml", UriKind.Relative));
        }

        public async void AssignTracksHandler(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("./Pages/AssignRootTracksPage.xaml", UriKind.Relative));
        }

        public async void TrackReportDisplay(object sender, RoutedEventArgs e)
        {
            TrackReportDialog page = new TrackReportDialog();
            MainFrame.Navigate(page); 
        }

        public async void ViewStatementsDisplay(object sender, RoutedEventArgs e)
        {
            ViewStatementData page = new ViewStatementData();
            MainFrame.Navigate(page);
        }

        public async void DeleteDatabase(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This Will Delete All Data, Are you Sure?", "Delete ALL???", MessageBoxButton.OKCancel);
            if ( result == MessageBoxResult.OK)
            {
                EFData.DeleteDatabase();
            }
        }
        public void EditGenreList(object sender, EventArgs e)
        {
            
        }

        public void EditSubgenreList(object sender, EventArgs e)
        {

        }

        public void EditLibraryList(object sender, EventArgs e)
        {

        }
        public void DisplayStatementData()
        {
            
        }

        public void LibraryReportsDisplay(object sender, EventArgs e)
        {
            CategoryReportPage page = new CategoryReportPage("library");
            MainFrame.Navigate(page);
        }

        public void GenreReportsDisplay(object sender, EventArgs e)
        {
            CategoryReportPage page = new CategoryReportPage("genre");
            MainFrame.Navigate(page);
        }

        public void SubgenreReportsDisplay(object sender, EventArgs e)
        {
            CategoryReportPage page = new CategoryReportPage("subgenre");
            MainFrame.Navigate(page);
        }

        public void AlbumReportsDisplay(object sender, EventArgs e)
        {
            CategoryReportPage page = new CategoryReportPage("album");
            MainFrame.Navigate(page);
        }

        public void CountryReportsDisplay(object sender, EventArgs e)
        {
            CategoryReportPage page = new CategoryReportPage("country");
            MainFrame.Navigate(page);
        }

        public void MakeChart(object sender, EventArgs e)
        {
            DetailDisplayPage page = new DetailDisplayPage("RT", "Vintage Champion");
            MainFrame.Navigate(page);
        }
    }
}