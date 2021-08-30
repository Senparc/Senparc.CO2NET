using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class EncryptHelperTests
    {
        string encypStr = "Senparc";

        #region SHA相关

        [TestMethod]
        public void GetSha1Test()
        {
            var result = EncryptHelper.GetSha1("jsapi_ticket=sM4AOVdWfPE4DxkXGEs8VMCPGGVi4C3VM0P37wVUCFvkVAy_90u5h9nbSlYy3-Sl-HhTdfl2fzFy1AOcHKP7qg&noncestr=Wm3WZYTPz0wzccnW&timestamp=1414587457&url=http://mp.weixin.qq.com?params=value");
            Assert.AreEqual("0f9de62fce790f9a083d5c99e95740ceb90c27ed", result);

            result = EncryptHelper.GetSha1(encypStr);
            Assert.AreEqual("6d5c5d6c9d36c88cf47e1b97b813b74c78111cb2", result);
        }

        [TestMethod]
        public void GetHmacSha256Test()
        {
            var msg = "{\"foo\":\"bar\"}";
            var sessionKey = "o0q0otL8aEzpcZL/FT9WsQ==";
            var result = EncryptHelper.GetHmacSha256(msg, sessionKey);
            Assert.AreEqual("654571f79995b2ce1e149e53c0a33dc39c0a74090db514261454e8dbe432aa0b", result);
        }

        #endregion

        #region MD5

        [TestMethod()]
        public void GetMD5Test()
        {
            string exceptMD5Result = "8C715F421744218AB5B9C8E8D7E64AC6";

            //常规方法
            var result = EncryptHelper.GetMD5(encypStr, Encoding.UTF8);
            Assert.AreEqual(exceptMD5Result, result);

            //重写方法
            result = EncryptHelper.GetMD5(encypStr);
            Assert.AreEqual(exceptMD5Result, result);

            //小写
            result = EncryptHelper.GetLowerMD5(encypStr, Encoding.UTF8);
            Assert.AreEqual(exceptMD5Result.ToLower(), result);

            //Stream
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sr = new StreamWriter(ms))
                {
                    sr.Write(encypStr);
                    sr.Flush();

                    result = EncryptHelper.GetMD5(ms);
                    Assert.AreEqual(exceptMD5Result/*大写*/, result);

                    result = EncryptHelper.GetMD5(ms, false);
                    Assert.AreEqual(exceptMD5Result.ToLower()/*小写*/, result);
                }
            }
        }

        #endregion

        #region AES

        [TestMethod]
        public void AESEncryptTest()
        {
            var key = "SENPARC_KEY";
            {
                //加密-CBC
                var inputBytes = Encoding.UTF8.GetBytes(encypStr);
                var iv = Encoding.UTF8.GetBytes("SENPARC_IV;SENPA");//16字节

                var encryptResult = Convert.ToBase64String(EncryptHelper.AESEncrypt(inputBytes, iv, key));
                Console.WriteLine("Result:" + encryptResult);
                Assert.AreEqual("Q0l9E//huAwYXzYmxMWusw==", encryptResult);


                //解密-CBC
                inputBytes = Convert.FromBase64String(encryptResult);
                var decryptResult = Encoding.UTF8.GetString(EncryptHelper.AESDecrypt(inputBytes, iv, key));
                Assert.AreEqual(encypStr, decryptResult);
            }

            {
                //加密-CEB
                var encryptResult = EncryptHelper.AESEncrypt(encypStr, key);
                Console.WriteLine("CEB encrypt：" + encryptResult);
                //Assert.AreEqual("raQCWEp5ngocSs5R8srxkg==", encryptResult);
                Assert.IsTrue(encryptResult.Length > 0);

                //解密-CEB
                var cebResult = EncryptHelper.AESDecrypt(encryptResult, key);
                Console.WriteLine("CEB decrypt：" + cebResult);
                Assert.AreEqual(encypStr, cebResult);
            }


        }

        [TestMethod]
        public void AESEncryptForTenpayV3Test()
        {
            var key = "TheKey";
            {
                //TenPayV3（旧）中的 req_info
                var input = "6scm3lyoIZj2YLSVosQsd7xziXw9vJb9w9A5jY0LUNM0O5g9T3MoNgJ5A2xXD26M44rPjGsQXLYxIIxMJWWLPmdXef0xq+b1XKMaKA49H/ft1+82bKPNQS9dYK7RBQ6cvfFjBJMrSvseyWE5ASGfMLg9psnMdU1sC7DMSRSxMRrw7Vzkuvu2QWbK1SA26fehtqHphKoW1pZNy7fDnQb3j+vUeZTDhzbc2g0kspo9JQS60p0L79Aj9Gl15OTreXEplMi4nAU/E4ULptjtF+ylicF0pHKmjsjMufSxYnaBaGZmLlioaigZt1RTWBO90D2NmodFCm7muyGcuCbdfvLhB6Yde8KfVM/yhnC0b42iwi0ASwjCA+jlVIm9ys6Wxrz1lSAXcRF06+ySXgGRXBMpdIitW59Hx4zS0UIATXes9U1TDaZXGYrZDZM02GkqYAX4KiqpmhKC+PNtGrtbPNZbwWWtSl+UE6h4QyPv2cPdRPRMyGlzabauriMiNALF8bDaNTn6K1Nf3tA3nKWh1oemvjCYvT9+mUI8jnyEsXVjnakCOJyKoCIzNgJwliUIV4GXPIauWPFbbG3Tbtm8AAv3FC1yAvdustwLreiXOvOXgvZnSXIV4xLgUfjFWzoc9crwMXd8gJJFW0YAjhF+78WJ4yvDklg/oZnlXUo/ZEnjRdM2AxVTaAHuVNyi3tGMBDustRotkLbAKlR/GW/GaQRF0t8fagJJrEvbOkyrA+NUTCTHOpJ2Yi4YWoj9M2Zar4cXkKixOkx+PpgKMgMffOEnFAe1oTxI8ZwOrxgAjN8O9kPoXecQ2TaP4OyN/4vNxMZMjM/ksmSgAilvEj91PYLme4MY5WjUunLQxdiNx2ZgJj4+b1xyN+thQaYjN34XM97Ao7xZxVlexxN3SspOUvtKQ9Wn3T6c9UAgl184yNYrV/ZJ2xWwpeVyL1H/h29tQxxBjg1SIA1wLda3fRvWIszpqL5OWVUMzQztE4egmVuU8txrMkAEqOhFE1cdzIm7GFJL08IZnMslEs0em/+tJIw8igmQvihNrKwgtDbR78Lsrv84Tpll9qL76PqLrgqaYQuU";

                var md5Str = EncryptHelper.GetLowerMD5(key, Encoding.UTF8);
                var result = EncryptHelper.AESDecrypt(input, md5Str);

                Console.WriteLine(result);
            }



        }
        #endregion

    }
}
