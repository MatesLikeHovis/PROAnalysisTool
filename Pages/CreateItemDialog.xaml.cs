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
using System.Windows.Shapes;

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for CreateItemDialog.xaml
    /// </summary>
    public partial class CreateItemDialog : Window
    {

        public string ItemType { get; private set; }
        public CreateItemDialog(string itemVariety)
        {
            InitializeComponent();
            ItemType = itemVariety;
            ItemVarietyBox.Content = "New " + ItemType;
            this.Title = "Create New " + ItemType;
            ItemTypeTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ItemType = ItemTypeTextBox.Text;
            DialogResult = true; // Close the dialog and indicate a result
        }
        private void ItemTypeTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                OkButton_Click(sender, e); // Trigger the same logic as the OK button click
            }
        }
    }
}
