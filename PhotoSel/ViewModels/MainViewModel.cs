using PhotoSel.Commands;
using PhotoSel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoSel.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        IPhotoCache PhotoCache { get; }
        IDialogService DialogService { get; }

        public ObservableCollection<PhotoViewModel> Photos { get; } = new ObservableCollection<PhotoViewModel>();

        PhotoViewModel selectedPhoto;
        public PhotoViewModel SelectedPhoto
        {
            get => selectedPhoto;
            set
            {
                if (SetProperty(ref selectedPhoto, value))
                {
                    OnPropertyChanged(nameof(CurrentPhotoNumber));

                    // preload the next two images (asynchronously)
                    foreach (var p in Photos.Skip(CurrentPhotoNumber).Take(2))
                    {
                        p.PreloadImage();
                    }
                }
            }
        }

        string sourceFolder = "";
        public string SourceFolder { get => sourceFolder; set => SetProperty(ref sourceFolder, value); }

        public int CurrentIndex => Photos.IndexOf(SelectedPhoto);
        public int CurrentPhotoNumber => CurrentIndex + 1;
        public int PhotoCount => Photos.Count;

        public int SelectedPhotosCount => Photos.Count(x => x.IsSelected);


        public ICommand LoadFolderCommand { get; private set; }

        public ICommand BrowseForSourceFolderCommand { get; private set; }

        public ICommand SaveFileListCommand { get; private set; }

        public ICommand CopySelectedToFolderCommand { get; private set; }

        public ICommand PreviousPhotoCommand { get; private set; }
        public ICommand NextPhotoCommand { get; private set; }
        public ICommand ToggleSelectionCommand { get; private set; }

        public MainViewModel(IPhotoCache photoCache, IDialogService dialogService)            
        {
            PhotoCache = photoCache ?? throw new ArgumentNullException(nameof(photoCache));
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            InitCommands();
        }

        protected void InitCommands()
        {
            // commands
            LoadFolderCommand = new RelayCommand(() =>
            {
                if (Directory.Exists(SourceFolder))
                    LoadPhotosFromFolder(sourceFolder);
                else
                    DialogService.ShowError("The given folder does not exist or is not accessible.");
            }, () => !String.IsNullOrEmpty(sourceFolder));

            BrowseForSourceFolderCommand = new RelayCommand(() =>
            {
                string folder;
                if (DialogService.BrowseForFolder("Select source folder", out folder))
                {
                    SourceFolder = folder;
                    LoadFolderCommand.Execute(null);
                }
            });

            PreviousPhotoCommand = new RelayCommand(() => SelectedPhoto = Photos[CurrentIndex - 1], () => CurrentIndex > 0);
            NextPhotoCommand = new RelayCommand(() => SelectedPhoto = Photos[CurrentIndex + 1], () => CurrentIndex < Photos.Count - 1);

            ToggleSelectionCommand = new RelayCommand(() =>
            {
                SelectedPhoto.IsSelected = !SelectedPhoto.IsSelected;
                SaveFileListCommand.Execute(null);

                OnPropertyChanged(nameof(SelectedPhotosCount));
            }, () => SelectedPhoto != null);

            SaveFileListCommand = new RelayCommand(() => SaveSelectedFileList(sourceFolder + "\\__photosel.txt"), () => !String.IsNullOrEmpty(sourceFolder) && Photos.Count > 0);

            CopySelectedToFolderCommand = new RelayCommand(() =>
            {
                string targetPath;
                if (DialogService.BrowseForFolder("Select target folder", out targetPath))
                {
                    if (Directory.GetFiles(targetPath).FirstOrDefault() != null)
                    {
                        if (!DialogService.ShowConfirmation("The target folder is not empty, do you want to continue anyway?"))
                            return;
                    }

                    CopySelectedPhotosToFolder(targetPath);
                }
            }, () => Photos.Count > 0);
        }

        protected void LoadPhotosFromFolder(String folder)
        {
            Photos.Clear();

            foreach(var f in Directory.GetFiles(folder, "*.jpg"))
            {
                Photos.Add(new PhotoViewModel(f, PhotoCache));
            }            

            SelectedPhoto = Photos.FirstOrDefault();
            OnPropertyChanged(nameof(PhotoCount));

            if (File.Exists(sourceFolder + "\\__photosel.txt"))
            {
                LoadSelectedFileList(sourceFolder + "\\__photosel.txt");
            }            
        }

        protected void SaveSelectedFileList(string targetFile)
        {
            using (var writer = new StreamWriter(new FileStream(targetFile, FileMode.Create), Encoding.UTF8))
            {
                foreach (var sf in Photos.Where(x => x.IsSelected))
                    writer.WriteLine(sf.FileUri.AbsolutePath);
            }
        }

        protected void LoadSelectedFileList(string sourceFile)
        {
            using (var reader = new StreamReader(new FileStream(sourceFile, FileMode.Open), Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    string fname = reader.ReadLine();
                    var p = Photos.FirstOrDefault(x => x.FileUri.AbsolutePath == fname);
                    if (p != null)
                        p.IsSelected = true;
                }
            }

            OnPropertyChanged(nameof(SelectedPhotosCount));
        }

        protected void CopySelectedPhotosToFolder(String targetFolder)
        {
            foreach(var sel in Photos.Where(x => x.IsSelected))
            {
                File.Copy(sel.FilePath, targetFolder + "\\" + Path.GetFileName(sel.FilePath));
            }
        }

    }
}
