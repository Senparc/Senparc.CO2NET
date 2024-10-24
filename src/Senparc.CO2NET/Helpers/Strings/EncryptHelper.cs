#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc
 
    Creation Identifier: Senparc - 20160808
    File Function Description: Security helper class, provides SHA-1, AES algorithms, etc.

    Modification Identifier: Senparc - 20170130
    Modification Description: v4.9.15 Added AES encryption and decryption algorithms
    
    Modification Identifier: Senparc - 20170313
    Modification Description: v4.11.4 Modified EncryptHelper.GetSha1(string encypStr) method algorithm
      
    Modification Identifier: Senparc - 20170313
    Modification Description: v4.14.3 Refactored MD5 generation method and provided lowercase MD5 method
    
    Modification Identifier: Senparc - 20180101
    Modification Description: v4.18.10 Added EncryptHelper.GetHmacSha256() method to support "mini-games" signature


    
    ----  CO2NET   ----

    Modification Identifier: Senparc - 20180601
    Modification Description: v5.0.0 Introduced Senparc.CO2NET

    Modification Identifier: Senparc - 20210831
    Modification Description: v1.5.1 Added and enriched encryption methods in EncryptHelper (SHA1, AesGcmDecrypt, CRC32)

    Modification Identifier: Senparc - 20221220
    Modification Description: v2.1.5 Added EncryptHelper.GetCertString() and GetCertStringFromFile() methods

    Modification Identifier: Senparc - 20240511
    Modification Description: v2.4.1 Added refresh parameter to SenparcDI.GetServiceProvider() method

----------------------------------------------------------------*/


using Senparc.CO2NET.HttpUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// Security helper class, provides SHA-1 algorithm, etc.
    /// </summary>
    public class EncryptHelper
    {
        #region SHA相关

        /// <summary>
        /// Encrypt string using SHA-1 algorithm (lowercase)
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <returns></returns>
        public static string GetSha1(string encypStr)
        {
            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(encypStr));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }

            return GetSha1(encypStr, false);

            //byte[] strRes = Encoding.Default.GetBytes(encypStr);
            //HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            //strRes = iSHA.ComputeHash(strRes);
            //StringBuilder enText = new StringBuilder();
            //foreach (byte iByte in strRes)
            //{
            //    enText.AppendFormat("{0:x2}", iByte);
            //}
        }

        /// <summary>
        /// Encrypt string using SHA-1 algorithm (default uppercase)
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="toUpper">Whether to return uppercase result, true: uppercase, false: lowercase</param>
        /// <param name="encoding">Encoding</param>
        /// <returns></returns>
        public static string GetSha1(string encypStr, bool toUpper = true, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(encoding.GetBytes(encypStr));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }
            if (!toUpper)
            {
                return enText.ToString();
            }
            else
            {
                return enText.ToString().ToUpper();
            }

            //byte[] strRes = Encoding.Default.GetBytes(encypStr);
            //HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            //strRes = iSHA.ComputeHash(strRes);
            //StringBuilder enText = new StringBuilder();
            //foreach (byte iByte in strRes)
            //{
            //    enText.AppendFormat("{0:x2}", iByte);
            //}
        }

        /// <summary>
        /// Encrypt stream using SHA-1 algorithm (default uppercase)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="toUpper">Whether to return uppercase result, true: uppercase, false: lowercase</param>
        /// <returns></returns>
        public static string GetSha1(Stream stream, bool toUpper = true)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sha1 = SHA1.Create();
            var sha1Arr = sha1.ComputeHash(stream);
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }

            var result = enText.ToString();
            if (!toUpper)
            {
                return result;
            }
            else
            {
                return result.ToUpper();
            }
        }

        #region 弃用算法


        //        /// <summary>
        //        /// Signature algorithm
        //        /// </summary>
        //        /// <param name="str"></param>
        //        /// <returns></returns>
        //        public static string GetSha1(string str)
        //        {
        //            //Create SHA1 object
        //#if NET462
        //            SHA1 sha = new SHA1CryptoServiceProvider();
        //#else
        //            SHA1 sha = SHA1.Create();
        //#endif

        //            //Convert mystr to byte[] 
        //            ASCIIEncoding enc = new ASCIIEncoding();
        //            byte[] dataToHash = enc.GetBytes(str);
        //            //Hash operation
        //            byte[] dataHashed = sha.ComputeHash(dataToHash);
        //            //Convert result to string
        //            string hash = BitConverter.ToString(dataHashed).Replace("-", "");
        //            return hash;
        //        }

        #endregion


        /// <summary>
        /// HMAC SHA256 encryption
        /// </summary>
        /// <param name="message">Original message to be encrypted. When providing service for mini-program SessionKey signature, message is the data packet of this POST request (usually JSON). Specifically, for GET requests, message is an empty string.</param>
        /// <param name="secret">Secret key (e.g., SessionKey of mini-program)</param>
        /// <returns></returns>
        public static string GetHmacSha256(string message, string secret)
        {
            message = message ?? "";
            secret = secret ?? "";
            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                StringBuilder enText = new StringBuilder();
                foreach (var b in hashmessage)
                {
                    enText.AppendFormat("{0:x2}", b);
                }
                return enText.ToString();
            }
        }

        #endregion

        #region MD5

        /// <summary>
        /// Get uppercase MD5 signature result
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="encoding">Encoding</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, Encoding encoding)
        {
            string retStr;

#if NET462
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();
#else
            MD5 m5 = MD5.Create();
#endif

            //Create md5 object
            byte[] inputBye;
            byte[] outputBye;

            //Convert string to byte array using specified encoding.
            try
            {
                inputBye = encoding.GetBytes(encypStr);
            }
            catch
            {
                inputBye = Encoding.GetEncoding("utf-8").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        /// <summary>
        /// Get uppercase MD5 signature result
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="charset">Encoding</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, string charset = "utf-8")
        {
            charset = charset ?? "utf-8";
            try
            {
                //Use specified encoding
                return GetMD5(encypStr, Encoding.GetEncoding(charset));
            }
            catch
            {
                //Use UTF-8 encoding
                return GetMD5("utf-8", Encoding.GetEncoding(charset));

                //#if NET462
                //                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
                //#else
                //                inputBye = Encoding.GetEncoding(936).GetBytes(encypStr);
                //#endif
            }
        }

        /// <summary>
        /// Get MD5 signature result
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="toUpper">Whether to return uppercase result, true: uppercase, false: lowercase</param>
        /// <returns></returns>
        public static string GetMD5(Stream stream, bool toUpper = true)
        {
            stream.Position = 0;

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] ret = md5.ComputeHash(stream);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ret.Length; i++)
            {
                sb.Append(ret[i].ToString(toUpper ? "X2" : "x2"));
            }

            string md5str = sb.ToString();
            return md5str;
        }

        /// <summary>
        /// Get lowercase MD5 signature result
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="encoding">Encoding</param>
        /// <returns></returns>
        public static string GetLowerMD5(string encypStr, Encoding encoding)
        {
            return GetMD5(encypStr, encoding).ToLower();
        }

        #endregion

        #region CRC32

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="toUpper">Whether to return uppercase result, true: uppercase, false: lowercase</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetCrc32(string encypStr, bool toUpper = true, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            Crc32 calculator = new Crc32();
            byte[] buffer = calculator.ComputeHash(encoding.GetBytes(encypStr));
            calculator.Clear();
            //Convert byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }

            return toUpper ? toUpper.ToString().ToUpper() : sb.ToString();
        }

        /// <summary>
        /// Get CRC32 encrypted string
        /// </summary>
        /// <param name="encypStr">String to be encrypted</param>
        /// <param name="toUpper">Whether to return uppercase result, true: uppercase, false: lowercase</param>
        /// <returns></returns>
        public static string GetCrc32(Stream stream, bool toUpper = true)
        {
            Crc32 calculator = new Crc32();
            byte[] buffer = calculator.ComputeHash(stream);
            calculator.Clear();
            //Convert byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("x2"));
            }

            return toUpper ? toUpper.ToString().ToUpper() : sb.ToString();
        }

        #endregion

        #region AES - CBC

        /// <summary>
        /// AES encryption (default CBC mode)
        /// </summary>
        /// <param name="inputdata">Input data</param>
        /// <param name="iv">Vector</param>
        /// <param name="strKey">Encryption key</param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            //Block encryption algorithm   
#if NET462
            SymmetricAlgorithm des = Rijndael.Create();
#else
            SymmetricAlgorithm des = Aes.Create();
#endif

            byte[] inputByteArray = inputdata;//Get byte array to be encrypted       
                                              //Set key and key vector
            des.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            des.IV = iv;

            //Console.WriteLine(des.Mode);//CBC

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    byte[] cipherBytes = ms.ToArray();//Get encrypted byte array   
                    //cs.Close();
                    //ms.Close();
                    return cipherBytes;
                }
            }
        }


        /// <summary>
        /// AES decryption (default CBC mode)
        /// </summary>
        /// <param name="inputdata">Input data</param>
        /// <param name="iv">Vector</param>
        /// <param name="strKey">Key</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] inputdata, byte[] iv, string strKey)
        {
#if NET462
            SymmetricAlgorithm des = Rijndael.Create();
#else
            SymmetricAlgorithm des = Aes.Create();
#endif

            des.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            des.IV = iv;
            byte[] decryptBytes = null;// new byte[inputdata.Length];
            using (MemoryStream ms = new MemoryStream(inputdata))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (MemoryStream originalMemory = new MemoryStream())
                    {
                        Byte[] Buffer = new Byte[1024];
                        Int32 readBytes = 0;
                        while ((readBytes = cs.Read(Buffer, 0, Buffer.Length)) > 0)
                        {
                            originalMemory.Write(Buffer, 0, readBytes);
                        }

                        decryptBytes = originalMemory.ToArray();

                        #region 废弃的方法

                        //cs.Read(decryptBytes, 0, decryptBytes.Length);
                        ////cs.Close();
                        ////ms.Close();

                        #endregion
                    }
                }
            }
            return decryptBytes;
        }
        #endregion

        #region AES - CEB

        /// <summary>
        /// AES encryption (no vector, CEB mode, key length=128)
        /// </summary>
        /// <param name="str">Plaintext (to be encrypted)</param>
        /// <param name="key">Ciphertext</param>
        /// <returns></returns>
        public static string AESEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key.PadRight(32)),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);


            //#if NET462
            //            SymmetricAlgorithm des = Rijndael.Create();
            //#else
            //            SymmetricAlgorithm des = Aes.Create();
            //#endif

            //            byte[] inputByteArray = Encoding.UTF8.GetBytes(str);
            //            des.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
            //            des.Mode = CipherMode.ECB;
            //            des.Padding = PaddingMode.PKCS7;
            //            des.KeySize = 128;

            //            using (MemoryStream ms = new MemoryStream())
            //            {
            //                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            //                {
            //                    cs.Write(inputByteArray, 0, inputByteArray.Length);
            //                    cs.FlushFinalBlock();
            //                    byte[] cipherBytes = ms.ToArray();//Get encrypted byte array   
            //                    //cs.Close();
            //                    //ms.Close();
            //                    return Convert.ToBase64String(cipherBytes);
            //                }
            //            }

        }

        /// <summary>  
        /// AES decryption (no vector, CEB mode, key length=128)
        /// </summary>  
        /// <param name="data">Encrypted plaintext (note: Base64 encoded)</param>  
        /// <param name="key">Key</param>  
        /// <returns>Plaintext</returns>  
        public static string AESDecrypt(string data, string key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] bKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
            //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );  
            //mStream.Seek( 0, SeekOrigin.Begin );  


            //RijndaelManaged aes = new RijndaelManaged();
#if NET462
            SymmetricAlgorithm aes = Rijndael.Create();
#else
            SymmetricAlgorithm aes = Aes.Create();
#endif

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using (var sr = new StreamReader(cryptoStream))
            {
                var str = sr.ReadToEnd();
                return str;
            }

            //            try
            //            {
            //                byte[] tmp = new byte[encryptedBytes.Length + 32];
            //                int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
            //                byte[] ret = new byte[len];
            //                Array.Copy(tmp, 0, ret, 0, len);
            //                var cc = cryptoStream.Length;
            //                return Encoding.UTF8.GetString(ret);
            //            }
            //            finally
            //            {
            //#if NET462
            //                cryptoStream.Close();
            //                mStream.Close();
            //                aes.Clear();
            //#else
            //                //cryptoStream.();
            //                //mStream.Close();
            //                //aes.Clear();
            //#endif
            //            }
        }

        #endregion

#if NETSTANDARD2_1_OR_GREATER
        #region AES - GCM
        //TODO: Unit test

        /// <summary>
        /// Decrypt ciphertext content from WeChat payment interface
        /// </summary>
        /// <returns></returns>
        public static string AesGcmDecrypt(string aes_key, string nonce, string associated_data, string content)
        {
            //Convert data needed for decryption to Bytes
            var keyBytes = Encoding.UTF8.GetBytes(aes_key);
            var nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedBytes = associated_data == null ? null : Encoding.UTF8.GetBytes(associated_data);

            //AEAD_AES_256_GCM decryption
            var encryptedBytes = Convert.FromBase64String(content);
            //tag size is 16
            var cipherBytes = encryptedBytes[..^16];
            var tag = encryptedBytes[^16..];
            var decryptedData = new byte[cipherBytes.Length];
            using var cipher = new AesGcm(keyBytes);
            cipher.Decrypt(nonceBytes, cipherBytes, tag, decryptedData, associatedBytes);
            var decrypted_string = Encoding.UTF8.GetString(decryptedData);

            return decrypted_string;
        }


        #endregion
#endif

        #region 证书相关

        /// <summary>
        /// Get certificate content from certificate file content (single line string)
        /// </summary>
        /// <param name="fileContent"></param>
        public static string GetCertString(string fileContent)
        {
            Regex regex = new Regex(@"(--([^\r\n])+--[\r\n]{0,1})|[\r\n]");
            var certString = regex.Replace(fileContent, "");
            return certString;
        }

        /// <summary>
        /// Get certificate content from certificate file (single line string)
        /// </summary>
        /// <param name="filePath">File absolute path</param>
        public static string GetCertStringFromFile(string filePath)
        {
            var fileContent = File.ReadAllText(filePath);
            return GetCertString(fileContent);
        }

        #endregion
    }
}
