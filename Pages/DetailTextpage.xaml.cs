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
using WPFBMI.Models;

namespace WPFBMI.Pages
{
    /// <summary>
    /// Interaction logic for DetailTextpage.xaml
    /// </summary>
    public partial class DetailTextpage : Page
    {
        public string type { get; set; }
        public string name {  get; set; }
        public DetailTextpage(string Type, string Name)
        {
            this.type = Type;
            this.name = Name;
            this.DataContext = this;
            InitializeComponent();
            if (this.type == "RT") { GetTrackInformation(); }
            else
            {
                GetCategoryInformation();
            }
        }

        public async void GetTrackInformation()
        {
            RootTrack rootTrack = await EFData.GetRootTrackFromName(name);
            List<EFUsage> usages = await EFData.GetUsageQuery(new List<string> { "RT" }, new List<string> { name });
            Label1.Content = new TextBlock
            {
                Inlines = {
                    new Run("Track: "),
                    new Run(rootTrack.trackName) { FontWeight = FontWeights.Bold }
                }
            };
            Label2.Content = new TextBlock
            {
                Inlines = {
                    new Run("Album: "),
                    new Run(rootTrack.album.name) { FontWeight = FontWeights.Bold }
                }
            };
            Label3.Content = new TextBlock
            {
                Inlines = {
                    new Run("Genre: "),
                    new Run(rootTrack.genre.name) { FontWeight = FontWeights.Bold }
                }
            };
            Label4.Content = new TextBlock
            {
                Inlines = {
                    new Run("Subgenre: "),
                    new Run(rootTrack.subgenre.name) { FontWeight = FontWeights.Bold }
                }
            };
            Label5.Content = new TextBlock
            {
                Inlines = {
                    new Run("Library: "),
                    new Run(rootTrack.library.name) { FontWeight = FontWeights.Bold }
                }
            };
            Label6.Content = new TextBlock
            {
                Inlines = {
                    new Run("Usage Count: "),
                    new Run(usages.Count.ToString()) { FontWeight = FontWeights.Bold }
                }
            };
        }

        public async void GetCategoryInformation()
        {

        }
    }
}
