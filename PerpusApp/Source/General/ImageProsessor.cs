using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Data;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.General
{
    /// <summary>
    /// Class untuk memperoses Image file
    /// </summary>
    public class ImageProsessor
    {
        private static string[] allowedExtension = {".jpg",".jpeg",".png"};

        /// <summary>
        /// Mengecek ekstensi file
        /// </summary>
        /// <param name="u_file">File dari input form</param>
        public static void CheckExtension(IFormFile u_file)
        {
            FormFile file = (FormFile)u_file;
            if(file.Length == 0) throw new Exception("", new Exception("Data is not added. Incorrect photo file"));
            string fileExt = Path.GetExtension(file.FileName).ToLower();
            if(!allowedExtension.Contains(fileExt)) throw new Exception("", new Exception("Data is not added. Incorrect photo file"));
        }

        /// <summary>
        /// Convert image menjadi thumbnail image
        /// </summary>
        /// <param name="u_file">File dari input form</param>
        /// <param name="userType">Nama tipe user</param>
        /// <param name="username">Nama user</param>
        /// <returns>photo file & photo name</returns>
        public static UserPhoto ConvertToThumb(IFormFile u_file, string userType, string username)
        {
            UserPhoto up = new UserPhoto();
            try
            {
                using (var memorystream = new MemoryStream())
                {
                    FormFile file = (FormFile)u_file;
                    file.CopyTo(memorystream);
                    Image originalImage = Image.FromStream(memorystream);
                    Image thumbnailImage = originalImage.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
                    ImageConverter imageConverter = new ImageConverter();
                    up.up_photo = (byte[])imageConverter.ConvertTo(thumbnailImage, typeof(byte[]));
                    string fileExt = Path.GetExtension(file.FileName).ToLower();
                    up.up_filename = userType + username + DateTime.Now.ToString() + fileExt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return up;
        }
    }
}