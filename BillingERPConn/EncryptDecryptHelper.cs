using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BillingERPConn
{
    public static class EncryptDecryptHelper
    {
       

        public static string Decrypt(string cipherText)
        {
            string str = "#*!@";
            string str1 = "123!@*";
            string str2 = "MD5";
            int num = 1;
            string str3 = "@1B2ll4Ge5F6n7X8";
            int num1 = 256;
            byte[] bytes = Encoding.ASCII.GetBytes(str3);
            byte[] numArray = Encoding.ASCII.GetBytes(str1);
            byte[] numArray1 = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(str, numArray, str2, num);
            byte[] bytes1 = passwordDeriveByte.GetBytes(num1 / 8);
            RijndaelManaged rijndaelManaged = new RijndaelManaged()
            {
                Mode = CipherMode.CBC
            };
            ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(bytes1, bytes);
            MemoryStream memoryStream = new MemoryStream(numArray1);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
            byte[] numArray2 = new byte[(int)numArray1.Length];
            int num2 = cryptoStream.Read(numArray2, 0, (int)numArray2.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(numArray2, 0, num2);
        }

        public static string Encrypt(string plainText)
        {
            string str = "#*!@";
            string str1 = "123!@*";
            string str2 = "MD5";
            int num = 1;
            string str3 = "@1B2ll4Ge5F6n7X8";
            int num1 = 256;
            byte[] bytes = Encoding.ASCII.GetBytes(str3);
            byte[] numArray = Encoding.ASCII.GetBytes(str1);
            byte[] bytes1 = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(str, numArray, str2, num);
            byte[] numArray1 = passwordDeriveByte.GetBytes(num1 / 8);
            RijndaelManaged rijndaelManaged = new RijndaelManaged()
            {
                Mode = CipherMode.CBC
            };
            ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(numArray1, bytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes1, 0, (int)bytes1.Length);
            cryptoStream.FlushFinalBlock();
            byte[] array = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(array);
        }

        public static bool ValidateLoginPassword(string inputPassword, string dbPassword, bool encryption)
        {
            string str = "";
            str = (!encryption ? inputPassword : EncryptDecryptHelper.Encrypt(inputPassword));
            return (str != dbPassword ? false : true);
        }
    }
}
