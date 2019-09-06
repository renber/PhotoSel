using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoSel.Services
{
    class DefaultDialogService : IDialogService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message)
        {            
            return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public bool BrowseForFolder(string title, out string selectedFolder)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = title;
            dialog.EnsurePathExists = true;   

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolder = dialog.FileName;
                return true;
            } else
            {
                selectedFolder = null;
                return false;
            }
        }
    }
}
