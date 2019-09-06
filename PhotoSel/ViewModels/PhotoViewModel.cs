using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoSel.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PhotoSel.ViewModels
{
    class PhotoViewModel : ViewModelBase
    {
        // restrict the maximum number of parallel thumbail loader tasks
        static SemaphoreSlim thumbnailLoadSemaphore = new SemaphoreSlim(5, 5);

        IPhotoCache PhotoCache { get; }

        Dispatcher dispatcher;

        public Uri FileUri { get; }

        public String FilePath { get; }

        public string Filename { get; }

        bool isSelected = false;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        double rotation = 0;
        public double Rotation { get => rotation; set => SetProperty(ref rotation, value); }        

        public ImageSource Image
        {
            get
            {                
                if (PhotoCache.ContainsImage(FilePath))
                {
                    return PhotoCache.GetImage(FilePath);
                } else
                {
                   PhotoCache.CacheImage(FilePath).ContinueWith((t) =>
                   {
                       OnPropertyChanged();
                   });
                    return null;
                }                                
            }            
        }

        bool isLoadingThumbnail;
        public bool IsLoadingThumbnail { get => isLoadingThumbnail; private set => SetProperty(ref isLoadingThumbnail, value); }

        public bool thumbnailLoaded;

        ImageSource thumbnailImage;
        public ImageSource ThumbnailImage
        {
            get
            {
                if (!thumbnailLoaded && !isLoadingThumbnail)
                {
                    IsLoadingThumbnail = true;
                    LoadThumbnail().ConfigureAwait(false);
                }

                return thumbnailImage;
            }
            private set
            {
                thumbnailLoaded = true;
                IsLoadingThumbnail = false;
                SetProperty(ref thumbnailImage, value);                
            }
        }

        public PhotoViewModel(String filepath, IPhotoCache photoCache)
        {            
            PhotoCache = photoCache ?? throw new ArgumentNullException(nameof(photoCache));

            FilePath = filepath;
            FileUri = new Uri(filepath);
            Filename = Path.GetFileName(filepath);

            dispatcher = Dispatcher.CurrentDispatcher;

            ReadMetaData().ConfigureAwait(false);
        }

        public async Task PreloadImage()
        {
            if (!PhotoCache.ContainsImage(FilePath))
                await PhotoCache.CacheImage(FilePath);
        }

        protected async Task LoadThumbnail()
        {
            IsLoadingThumbnail = true;
            thumbnailLoaded = false;
            
             await thumbnailLoadSemaphore.WaitAsync();
             try
            { 
                await Task.Run(() =>
                {
                    ImageSource img = PhotoCache.GetThumbnail(FilePath);
                    dispatcher.InvokeAsync(() => ThumbnailImage = img);
                });
            } finally
            {
                thumbnailLoadSemaphore.Release();
            }
        }

        protected async Task ReadMetaData()
        {
            await Task.Run(() =>
          {
              using (var fstream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
              {
                  try
                  {
                      var dirs = ImageMetadataReader.ReadMetadata(fstream);                      
                      ReadOrientation(dirs.OfType<ExifIfd0Directory>().FirstOrDefault(), dispatcher);
                  } catch (Exception e)
                  {
                      // TODO: log
                  }
              }
          });
        }
      
        protected void ReadOrientation(ExifIfd0Directory exif, Dispatcher dispatcher)
        {
            if (exif == null)
                return;

            short orientation;
            if (exif.TryGetInt16(ExifIfd0Directory.TagOrientation, out orientation))
            {
                dispatcher.InvokeAsync(() => SetRotationFromExifValue(orientation));
            }
        }

        protected void SetRotationFromExifValue(short val)
        {
            if (val == 3 || val == 4)
                Rotation = 180;
            else if (val == 5 || val == 6)
                Rotation = 90;
            else if (val == 7 || val == 8)
                Rotation = 270;
            else
                Rotation = 0;
            /*
            if (val == 2 || val == 4 || val == 5 || val == 7)
                rot |= RotateFlipType.RotateNoneFlipX;

            if (rot != RotateFlipType.RotateNoneFlipNone)
                img.RotateFlip(rot);*/
        }

    }
}
