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
    /// Interaction logic for DetailDisplayPage.xaml
    /// </summary>
    public partial class DetailDisplayPage : Page
    {
        public string Type {  get; set; }
        public string Name { get; set; }

        public DetailDisplayPage(string type, string name)
        {
            this.Type = type;
            this.Name = name;
            InitializeComponent();
            this.DataContext = this;
            SetChart();
            SetText();
        }

        public void SetChart()
        {
            ChartPage chart = new(Type, Name);
            ChartFrame.Navigate(chart);
        }

        public void SetText()
        {
            DetailTextpage page = new(Type, Name);
            TextFrame.Navigate(page);
        }
    }
}
