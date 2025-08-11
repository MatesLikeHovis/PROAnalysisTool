using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using WPFBMI.Models;

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for SelectLibraryDialog.xaml
    /// </summary>
    public partial class SelectLibraryDialog : Window
    {

        public ObservableCollection<DisplayItem> Libraries { get; set; }
        private RootTrack _selectedTrack;
        public RootTrack UpdatedTrack => _selectedTrack;
        public SelectLibraryDialog(RootTrack selectedTrack)
        {
            InitializeComponent();
            _selectedTrack = selectedTrack;
            this.DataContext = this;
            PopulateLibraryList();
        }
        public async void PopulateLibraryList()
        {
            List<Library> genreList = await EFData.GetLibrariesList();
            List<DisplayItem> displayItems = new();
            foreach (Library library in genreList)
            {
                DisplayItem item = new();
                item.ID = library.id;
                item.Name = library.name;
                item.Library = library;
                displayItems.Add(item);
            }
            displayItems.OrderBy(s => s.Name).ToList();
            Libraries = new(displayItems);
        }

        public async void SelectItem(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button?.DataContext as DisplayItem;
            Library selectedLibrary = selectedItem.Library;
            _selectedTrack.library = selectedLibrary;
            this.DialogResult = true;
        }

        public async void CreateNew(object sender, EventArgs e)
        {
            CreateItemDialog dialog = new CreateItemDialog("New Library");

            // Show the dialog and check if it was accepted (OK button clicked)
            if (dialog.ShowDialog() == true)
            {
                // Get the returned value from the dialog
                string newItem = dialog.ItemType;
                Library newLibrary = await EFData.AddLibraryRequest(newItem);
                if (newLibrary != null)
                {
                    _selectedTrack.library = newLibrary;
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
            public Library Library { get; set; }

        }
    }
}