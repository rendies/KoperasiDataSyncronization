using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KoperasiDataSyncronization
{
    public class DataEncryption
    {
        public string secret_key { get; set; }

        public string secret_iv { get; set; }

        public DataEncryption()
        {

        }

        public DataEncryption(string key, string iv)
        {
            secret_iv = iv;
            secret_key = key;
        }
        /* public string Encrypt(String plainText)
         {

             try
             {
                 AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                 aesProvider.KeySize = 256;
                 aesProvider.BlockSize = 128;
                 aesProvider.Mode = CipherMode.CBC;
                 aesProvider.Padding = PaddingMode.PKCS7;

                 byte[] byteText = Encoding.UTF8.GetBytes(plainText);
                 byte[] byteKey = Encoding.UTF8.GetBytes(secret_key.PadRight(32, '\0'));
                 if (byteKey.Length > 32)
                 {
                     byte[] bytePass = new byte[32];
                     Buffer.BlockCopy(byteKey, 0, bytePass, 0, 32);
                     byteKey = bytePass;
                 }
                 byte[] byteIV = Encoding.UTF8.GetBytes(ComputeSha256Hash(secret_iv).Substring(0,16));
                 if (byteIV.Length > 16)
                 {
                     byte[] byteInit = new byte[16];
                     Buffer.BlockCopy(byteIV, 0, byteInit, 0, 16);
                     byteIV = byteInit;
                 }

                 aesProvider.Key = byteKey;
                 aesProvider.IV = byteIV;

                 byte[] byteData = aesProvider.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
                 return Convert.ToBase64String(byteData);
             }
             catch (Exception ex)
             {
                 return ex.Message;
             }
         }*/

        public String Encrypt(String plainText)
        {

            var key = Encoding.UTF8.GetBytes(ComputeSha256Hash(secret_key).Substring(0, 32));
            var iv = Encoding.UTF8.GetBytes(ComputeSha256Hash(secret_iv).Substring(0, 16));
            
            AesCryptoServiceProvider rj = new AesCryptoServiceProvider();

            rj.Mode = CipherMode.CBC;
            rj.KeySize = 256;
            rj.BlockSize = 128;
            rj.Padding = PaddingMode.PKCS7;
            
            var enrypt = rj.CreateEncryptor(key, iv);
            byte[] byteInput = Encoding.UTF8.GetBytes(plainText);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            var memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, enrypt, CryptoStreamMode.Write);
            cryptoStream.Write(byteInput, 0, byteInput.Length);
            cryptoStream.FlushFinalBlock();
            //Console.WriteLine(Convert.ToBase64String(memStream.ToArray()));

            // Return the encrypted data as a string
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToBase64String(memStream.ToArray())));
        }


        public string Decrypt(string cipherText)
        {
            cipherText = Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));

            var key = Encoding.UTF8.GetBytes(ComputeSha256Hash(secret_key).Substring(0, 32));
            var iv = Encoding.UTF8.GetBytes(ComputeSha256Hash(secret_iv).Substring(0, 16));

            RijndaelManaged rijn = new RijndaelManaged();

            rijn.Mode = CipherMode.CBC;
            rijn.KeySize = 256;
            rijn.BlockSize = 128;
            rijn.Padding = PaddingMode.PKCS7;

            String result;


            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (ICryptoTransform decryptor = rijn.CreateDecryptor(key, iv))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader swDecrypt = new StreamReader(csDecrypt))
                        {
                            result = swDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            rijn.Clear();

            // Return the decrypted data as a string
            return result;
        }

        static String ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(rawData));
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
