using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Utils for cryptography.
/// </summary>
namespace VVVV.Utils.Crypto
{
    /// <summary>
    /// StringHasher.
    /// </summary>
    public class StringHasher
    {
        private MD5CryptoServiceProvider FMd5 = new MD5CryptoServiceProvider();
        private SHA1CryptoServiceProvider FSha = new SHA1CryptoServiceProvider();

        public StringHasher()
        {
        }
        
        public string ToMD5(string input)
        {
            byte[] strarray = Encoding.Default.GetBytes(input);
            
            //Compute Hash
            byte[] hash = FMd5.ComputeHash(strarray);
            
            StringBuilder sb = new StringBuilder();
            foreach (byte hex in hash)
            {
                sb.Append(hex.ToString("x2"));
            }
            
            return sb.ToString();
        }
        
        public string ToSHA(string input)
        {
            byte[] strarray = Encoding.Default.GetBytes(input);
            
            //Compute Hash
            byte[] hash = FSha.ComputeHash(strarray);
            
            StringBuilder sb = new StringBuilder();
            foreach (byte hex in hash)
            {
                sb.Append(hex.ToString("x2"));
            }
            
            return sb.ToString();
        }
    }
}
