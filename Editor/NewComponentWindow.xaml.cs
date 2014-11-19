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

namespace Editor
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class NewComponentWindow : Window
    {
        private Engine m_engine;
        private int columns;
        public NewComponentWindow(Engine mainEngine)
        {
            InitializeComponent();
            m_engine = mainEngine;
            columns = 1;
        }

        private void plusClicked(object sender, RoutedEventArgs e)
        {
            Button b1 = new Button();
            TextBox tb1 = new TextBox();
            ComboBox cb = new ComboBox();
            TextBox tb2 = new TextBox();
            Button b2 = new Button();
            b1.SetValue(Grid.ColumnProperty,0);
            b1.SetValue(Grid.RowProperty,3);
        }
    }
}
