using System;
using System.Security.Cryptography;

namespace PerpusApp.Source.General
{
    /// <summary>
    /// Random Hash Generated class
    /// </summary>
    public sealed class Crypto
    {
        /// <summary>
        /// Ukuran dari salt
        /// </summary>
        private const int SaltSize = 16;

        /// <summary>
        /// Ukuran dari hash
        /// </summary>
        private const int HashSize = 20;

        /// <summary>
        /// Membuat sebuah hash dari password
        /// </summary>
        /// <param name="password">password</param>
        /// <param name="iterations">jumlah iterations</param>
        /// <returns>hashed password</returns>
        public static string Hash(string password, int iterations)
        {
            //create salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            //create hash
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            //combine salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            //convert to base 64
            string base64Hash = Convert.ToBase64String(hashBytes);

            //format hash with extra information
            return string.Format("$Perpus${0}${1}", iterations, base64Hash);
        }
        /// <summary>
        /// Membuat sebuah hash dari password dengan 10000 iterations
        /// </summary>
        public static string Hash(string password)
        {
            return Hash(password, 10000);
        }

        /// <summary>
        /// Mengecek hash support
        /// </summary>
        /// <param name="hashString">Hash</param>
        /// <returns>apakah support?</returns>
        public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("$Perpus$");
        }

        /// <summary>
        /// verifikasi password dengan hash
        /// </summary>
        /// <param name="password">password</param>
        /// <param name="hashedPassword">hash</param>
        /// <returns>apakah bisa di verifikasi?</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            // check hash
            if(!IsHashSupported(hashedPassword))
            {
                throw new NotSupportedException("Password Salah!");
            }

            // extract iterations and Base64 string
            string[] splittedHashString = hashedPassword.Replace("$Perpus$", "").Split("$");
            int iterations = int.Parse(splittedHashString[0]);
            string base64Hash = splittedHashString[1];

            // get hashbytes
            byte[] hashBytes = Convert.FromBase64String(base64Hash);

            // get salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // create hash with given salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // get result
            for (var i = 0; i < HashSize; i++)
            {
                if(hashBytes[i + SaltSize] != hash[i])
                {
                    return false;  
                }
            }
            return true;
        }
    }
}