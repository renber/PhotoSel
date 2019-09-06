using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PhotoSel.Services
{
    class DefaultPhotoCache : IPhotoCache
    {
        public int CacheSize { get; }

        public int TargetImageWidth { get; }

        List<CachedImage> images = new List<CachedImage>();

        /// <summary>
        /// Creates a PhotoCache with cache size 5 and targetImageWidth 1000
        /// </summary>
        public DefaultPhotoCache()
            : this(5, 1000)
        {
            // --
        }

        public DefaultPhotoCache(int cacheSize, int targetImageWidth)
        {
            CacheSize = cacheSize;
            TargetImageWidth = targetImageWidth;
        }

        public bool ContainsImage(String filepath)
        {            
            return images.Exists(x => x.FilePath == filepath);
        }

        public ImageSource GetImage(String filepath)
        {
            var img = images.FirstOrDefault(x => x.FilePath == filepath);
            if (img != null)
                return img.ImageSource;

            TrimCache();

            // load images with a lower resolution to preserve memory
            img = new CachedImage(filepath, TargetImageWidth);
            images.Add(img);
            img.Load();

            return img.ImageSource;
        }

        private void TrimCache()
        {
            if (images.Count >= CacheSize)
            {
                while (images.Count >= CacheSize)
                {
                    images[0].Dispose();
                    images.RemoveAt(0);
                }
            }
        }

        public async Task CacheImage(String filepath)
        {
            if (ContainsImage(filepath))
            {
                return;
            }

            await Task.Run(() =>
            {
               TrimCache();

               var img = new CachedImage(filepath, TargetImageWidth);
               images.Add(img);
               img.Load();
            });
        }

        public ImageSource GetThumbnail(String filepath)
        {
            // atm: thumbnails are not freed

            ImageSource img = null;

            try
            {
                using (var fstream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                {
                    var dirs = ImageMetadataReader.ReadMetadata(fstream);
                    img = ReadThumbnailFromExif(filepath, dirs.OfType<ExifThumbnailDirectory>().FirstOrDefault());
                }
            } catch (Exception e)
            {
                // TODO: log exception
                img = null;
            }

            if (img == null)
            {
                // there is no embedded thumbnail -> generate one
                var c = new CachedImage(filepath, 80);
                c.Load();
                img = c.ImageSource;
            }

            return img;
        }

        protected ImageSource ReadThumbnailFromExif(String filename, ExifThumbnailDirectory thumbnails)
        {
            if (thumbnails == null)
                return null;

            //Assert.IsTrue(thumbnails.GetDescription(ExifThumbnailDirectory.TagCompression).StartsWith("JPEG"));
            long thumbnailOffset = Int64.Parse(thumbnails.GetDescription(ExifThumbnailDirectory.TagThumbnailOffset).Split(' ')[0]);
            const int maxIssue35Offset = 12;
            int thumbnailLength = Int32.Parse(thumbnails.GetDescription(ExifThumbnailDirectory.TagThumbnailLength).Split(' ')[0]) + maxIssue35Offset;
            byte[] thumbnail = new byte[thumbnailLength];
            using (FileStream imageStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                imageStream.Seek(thumbnailOffset, SeekOrigin.Begin);
                imageStream.Read(thumbnail, 0, thumbnailLength);
            }

            // work around Metadata Extractor issue #35
            //Assert.IsTrue(thumbnailLength > maxIssue35Offset + 1);
            int issue35Offset = 0;
            for (int offset = 0; offset <= maxIssue35Offset; ++offset)
            {
                // 0xffd8 is the JFIF start of image segment indicator
                if ((thumbnail[offset] == 0xff) && (thumbnail[offset + 1] == 0xd8))
                {
                    issue35Offset = offset;
                    break;
                }
            }

            using (MemoryStream thumbnailStream = new MemoryStream(thumbnail, issue35Offset, thumbnailLength - issue35Offset, false))
            {
                JpegBitmapDecoder jpegDecoder = new JpegBitmapDecoder(thumbnailStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                WriteableBitmap writeableBitmap = new WriteableBitmap(jpegDecoder.Frames[0]);
                writeableBitmap.Freeze();
                return writeableBitmap;                
            }
        }        
    }

    /// <summary>
    /// Wraps an ImageSource which can be easily disposed
    /// </summary>

    class CachedImage : IDisposable
    {        
        public string FilePath { get; }

        public ImageSource ImageSource { get; private set; }

        private Stream Stream { get; set; }

        int RequiredWidth { get; }

        public CachedImage(string filepath, int requiredWidth)
        {
            FilePath = filepath;
            RequiredWidth = requiredWidth;

            Stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);                        
        }

        public void Load()
        {
            try
            {
                BitmapImage bimg = new BitmapImage();

                bimg.BeginInit();
                bimg.CreateOptions = BitmapCreateOptions.None;
                bimg.CacheOption = BitmapCacheOption.OnLoad;
                bimg.DecodePixelWidth = RequiredWidth;
                bimg.StreamSource = Stream;
                bimg.EndInit();
                bimg.Freeze();

                ImageSource = bimg;
            } catch (Exception e)
            {
                ImageSource = null;
            }            
        }

        public void Dispose()
        {
            ImageSource = null;
            if (Stream != null)
            {                
                Stream.Dispose();
                Stream = null;                
            }
        }
    }
}
