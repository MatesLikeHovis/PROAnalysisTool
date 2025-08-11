using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFBMI.Pages
{
    /// <summary>
    /// Interaction logic for DefaultPage.xaml
    /// </summary>
    public partial class DefaultPage : Page
    {
        int genreCount;
        int subgenreCount;
        int libraryCount;
        int statementCount;
        int usageCount;
        int trackCount;
        int rootTrackCount;
        int albumCount;
        int countryCount;

        public DefaultPage()
        {
            InitializeComponent();
            UpdatePage();
        }

        public async void CountItems()
        {
            this.genreCount = await EFData.CountGenres();
            this.subgenreCount = await EFData.CountSubgenres();
            this.libraryCount = await EFData.CountLibraries();
            this.albumCount = await EFData.CountAlbums();
            this.statementCount = await EFData.CountStatements();
            this.usageCount = await EFData.CountUsages();
            this.trackCount = await EFData.CountTracks();
            this.rootTrackCount = await EFData.CountRootTracks();
            this.countryCount = await EFData.CountCountries();
        }

        public async void UpdatePage()
        {
            CountItems();
            StatementNumLabel.Content = $"Number of Statements: {this.statementCount}";
            GenreNumLabel.Content = $"Number of Genres: {this.genreCount}";
            SubgenreNumLabel.Content = $"Number of Subgenres: {this.subgenreCount}";
            LibraryNumLabel.Content = $"Number of Libraries: {this.libraryCount}";
            AlbumNumLabel.Content = $"Number of Albums: {this.albumCount}";
            UsageNumLabel.Content = $"Number of Usages: {this.usageCount}";
            TrackNumLabel.Content = $"Number of Tracks: {this.trackCount}";
            RootTrackNumLabel.Content = $"Number of RootTracks: {this.rootTrackCount}";
            CountryNumLabel.Content = $"Number of Countries: {this.countryCount}";
        }

        public async void UpdatePageClick(object sender, EventArgs e)
        {
            UpdatePage();
        }
    }
}
