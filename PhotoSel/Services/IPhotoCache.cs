using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhotoSel.Services
{
    interface IPhotoCache
    {
        /// <summary>
        /// Indicates whether the given image ha sbeen cached already
        /// </summary>        
        bool ContainsImage(String filepath);

        /// <summary>
        /// Returns the image synchronously,
        /// if the image is already cached, the cached version is returned, otherwise the image is loaded
        /// </summary>        
        ImageSource GetImage(String filepath);

        /// <summary>
        /// Preloads the image and adds it to the cache asynchronously
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        Task CacheImage(String filepath);

        /// <summary>
        /// Returns a thumbnail (low-resolution version) of the given image file
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        ImageSource GetThumbnail(String filepath);
    }
}
