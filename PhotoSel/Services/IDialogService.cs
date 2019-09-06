using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSel.Services
{
    interface IDialogService
    {
        void ShowMessage(string message);

        void ShowError(string message);

        bool ShowConfirmation(string message);

        bool BrowseForFolder(string title, out string selectedFolder);
    }
}
