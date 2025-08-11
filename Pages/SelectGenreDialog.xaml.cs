using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFBMI.Models;

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for SelectGenreDialog.xaml
    /// </summary>
    public partial class SelectGenreDialog : Window
    {
        public ObservableCollection<DisplayItem> Genres { get; set; }
        private RootTrack _selectedTrack;
        public RootTrack UpdatedTrack => _selectedTrack;
        public SelectGenreDialog(RootTrack selectedTrack)
        {
            InitializeComponent();
            _selectedTrack = selectedTrack;
            this.DataContext = this;
            PopulateGenreList();
        }
        public async void PopulateGenreList()
        {
            List<Genre> genreList = await EFData.GetGenresList();
            List<DisplayItem> displayItems = new();
            foreach (Genre genre in genreList)
            {
                DisplayItem item = new();
                item.ID = genre.id;
                item.Name = genre.name;
                item.Genre = genre;
                displayItems.Add(item);
            }
            displayItems.OrderBy(s=>s.Name).ToList();
            Genres = new(displayItems);
        }

        public async void SelectItem(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button?.DataContext as DisplayItem;
            Genre selectedGenre = selectedItem.Genre;
            _selectedTrack.genre = selectedGenre;
            this.DialogResult = true;
        }

        public async void CreateNew(object sender, EventArgs e)
        {
            CreateItemDialog dialog = new CreateItemDialog("New Genre");

            // Show the dialog and check if it was accepted (OK button clicked)
            if (dialog.ShowDialog() == true)
            {
                // Get the returned value from the dialog
                string newItem = dialog.ItemType;
                Genre newGenre = await EFData.AddGenreRequest(newItem);
                if (newGenre != null) {
                    _selectedTrack.genre = newGenre;
                    this.DialogResult = true;
                }
            }
        }

        public void CancelAndExit(object sender, EventArgs e)
        {
            Close();
        }

        public class DisplayItem
        {
            public string Name { get; set; }
            public int ID { get; set; }
            public Genre Genre { get; set; }
            
        }
    }
}
