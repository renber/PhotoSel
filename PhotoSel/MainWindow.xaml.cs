using PhotoSel.Commands;
using PhotoSel.Services;
using PhotoSel.ViewModels;
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

namespace PhotoSel
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // ensure that bound keys invoke commands (when other controls have focus)
            foreach (InputBinding inputBinding in this.InputBindings)
            {
                if (inputBinding.Gesture is KeyGesture keyGesture)
                {
                    if (keyGesture is DelayKeyGesture delayGesture)
                    {
                        if (delayGesture.KeysMatch(sender, e))
                        {
                            if (delayGesture.Matches(sender, e))
                            {
                                ExecuteInputCommand(inputBinding);
                            }

                            // handled even if not executed, to keep the delay
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        if (keyGesture.Matches(this, e))
                        {
                            e.Handled = true;
                            ExecuteInputCommand(inputBinding);                            
                        }
                    }
                }
            }
        }

        private void ExecuteInputCommand(InputBinding inputBinding)
        {
            if (inputBinding.Command != null && inputBinding.Command.CanExecute(inputBinding.CommandParameter))
            {
                inputBinding.Command.Execute(inputBinding.CommandParameter);
            }
        }
    }
}
