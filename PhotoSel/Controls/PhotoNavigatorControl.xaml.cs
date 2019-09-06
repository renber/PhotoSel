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

namespace PhotoSel.Controls
{
    /// <summary>
    /// Interaktionslogik für PhotoNavigatorControl.xaml
    /// </summary>
    public partial class PhotoNavigatorControl : UserControl
    {
        public PhotoNavigatorControl()
        {
            InitializeComponent();
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // as previous and next are handled by the main window, suppress keyboard arrow navigation
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
