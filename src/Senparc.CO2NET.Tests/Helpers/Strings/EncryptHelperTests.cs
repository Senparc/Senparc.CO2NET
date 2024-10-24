using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Senparc.CO2NET.Helpers.Tests
{
    [TestClass]
    public class EncryptHelperTests
    {
        string encypStr = "Senparc";

        #region SHA Related

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

            //Encryption method
            var result = EncryptHelper.GetMD5(encypStr, Encoding.UTF8);
            Assert.AreEqual(exceptMD5Result, result);

            //Write data
            result = EncryptHelper.GetMD5(encypStr);
            Assert.AreEqual(exceptMD5Result, result);

            //Small write
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
                    Assert.AreEqual(exceptMD5Result/*Write data*/, result);

                    result = EncryptHelper.GetMD5(ms, false);
                    Assert.AreEqual(exceptMD5Result.ToLower()/*Small write*/, result);
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
                //Encryption-CBC
                var inputBytes = Encoding.UTF8.GetBytes(encypStr);
                var iv = Encoding.UTF8.GetBytes("SENPARC_IV;SENPA");//16 bytes

                var encryptResult = Convert.ToBase64String(EncryptHelper.AESEncrypt(inputBytes, iv, key));
                Console.WriteLine("Result:" + encryptResult);
                Assert.AreEqual("Q0l9E//huAwYXzYmxMWusw==", encryptResult);


                //Encryption-CBC
                inputBytes = Convert.FromBase64String(encryptResult);
                var decryptResult = Encoding.UTF8.GetString(EncryptHelper.AESDecrypt(inputBytes, iv, key));
                Assert.AreEqual(encypStr, decryptResult);
            }

            {
                //Encryption-CEB
                var encryptResult = EncryptHelper.AESEncrypt(encypStr, key);
                Console.WriteLine("CEB encrypt: " + encryptResult);
                //Assert.AreEqual("raQCWEp5ngocSs5R8srxkg==", encryptResult);
                Assert.IsTrue(encryptResult.Length > 0);

                //Encryption-CEB
                var cebResult = EncryptHelper.AESDecrypt(encryptResult, key);
                Console.WriteLine("CEB decrypt: " + cebResult);
                Assert.AreEqual(encypStr, cebResult);
            }
        }

        [TestMethod]
        public void AESEncryptForTenpayV3Test()
        {
            var key = "TheKey";//Use the same Key for encryption and decryption
            {
                //TenPayV3 uses the same req_info for encryption and decryption
                var input = "6scm3lyoIZj2YLSVosQsd7xziXw9vJb9w9A5jY0LUNM0O5g9T3MoNgJ5A2xXD26M44rPjGsQXLYxIIxMJWWLPmdXef0xq+b1XKMaKA49H/ft1+82bKPNQS9dYK7RBQ6cvfFjBJMrSvseyWE5ASGfMLg9psnMdU1sC7DMSRSxMRrw7Vzkuvu2QWbK1SA26fehtqHphKoW1pZNy7fDnQb3j+vUeZTDhzbc2g0kspo9JQS60p0L79Aj9Gl15OTreXEplMi4nAU/E4ULptjtF+ylicF0pHKmjsjMufSxYnaBaGZmLlioaigZt1RTWBO90D2NmodFCm7muyGcuCbdfvLhB6Yde8KfVM/yhnC0b42iwi0ASwjCA+jlVIm9ys6Wxrz1lSAXcRF06+ySXgGRXBMpdIitW59Hx4zS0UIATXes9U1TDaZXGYrZDZM02GkqYAX4KiqpmhKC+PNtGrtbPNZbwWWtSl+UE6h4QyPv2cPdRPRMyGlzabauriMiNALF8bDaNTn6K1Nf3tA3nKWh1oemvjCYvT9+mUI8jnyEsXVjnakCOJyKoCIzNgJwliUIV4GXPIauWPFbbG3Tbtm8AAv3FC1yAvdustwLreiXOvOXgvZnSXIV4xLgUfjFWzoc9crwMXd8gJJFW0YAjhF+78WJ4yvDklg/oZnlXUo/ZEnjRdM2AxVTaAHuVNyi3tGMBDustRotkLbAKlR/GW/GaQRF0t8fagJJrEvbOkyrA+NUTCTHOpJ2Yi4YWoj9M2Zar4cXkKixOkx+PpgKMgMffOEnFAe1oTxI8ZwOrxgAjN8O9kPoXecQ2TaP4OyN/4vNxMZMjM/ksmSgAilvEj91PYLme4MY5WjUunLQxdiNx2ZgJj4+b1xyN+thQaYjN34XM97Ao7xZxVlexxN3SspOUvtKQ9Wn3T6c9UAgl184yNYrV/ZJ2xWwpeVyL1H/h29tQxxBjg1SIA1wLda3fRvWIszpqL5OWVUMzQztE4egmVuU8txrMkAEqOhFE1cdzIm7GFJL08IZnMslEs0em/+tJIw8igmQvihNrKwgtDbR78Lsrv84Tpll9qL76PqLrgqaYQuU";

                var md5Str = EncryptHelper.GetLowerMD5(key, Encoding.UTF8);
                var result = EncryptHelper.AESDecrypt(input, md5Str);

                Console.WriteLine(result);
            }
        }
        #endregion


        #region ֤�����

        internal const string EXCEPT_RESULT = "MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDee2pWWXyO6nhEdf5nj/MvQj0/tDNBrwCT+JK38ZY8xIzNbIj7J2+oBPimkFpjtETRr1YoV4BDSxt0/3E0Rdb6+gTGhdYavXM7aYA7qY/EA1eI81Zp32Hfsvnl07tFybA1W/m2CRlJQIlmXBYurd9F0Os3MSZjRBoWyrd9ISgsKcco+wdeJkxGW12MoYNVZutAlXZBoNDisjZLc2koCi7fDspsjezOTUcBnbKtTohMB7lYaFORMItELXCQ6rHwIE8zO3sYzHIkM3MR8ilSJ59CnLh0R53uqkQJ7AGVgbv97XsUMjnPiWT4B9xzvR4KGWnMs0tBr/J/0mfZlRqqOLE5AgMBAAECggEAL9XjUDufX28kervP/l5iEDgyyR6qoqXI/wfELA6imeA80EFTJYUeKccf21hQRv28ikUjxjrCFjXw6l/97BpUFdRp8HFYTpmLTCvr6WgUxDVfvc9sNglUlu95caPrsR6jZ2WmNDCSokBhCoQkNNcnmXBJEq3brh43ac0eVKYraAsQ4G+CHhdzmbB8XLc+lg2qekqXOLiNRNCm9FAJ1669EiJrK2ry9rWFZvP9d5DuyqL9djX0ff+DqUvvQiV3usQlmeJdRVAgajWJYyVec0g442wY5xKfGZ0K+l8wp2ne5nxz7F6XBONWpTrpO8EUVN0ix5nnzBvQKSNDaIeFncEkMQKBgQD1JxSP+Ky3I1dGQpXpMXC3WOYiJyZqv5BmH3A+XKErEmEXwa0CfXnxhnS0yA8Ipp4SFuu3ynynhw/2/Ca1fXd3TRG0Xg0oVNSbN8Oscrj8GnCu2yCM/RM/5sJFyc/sH+hExyApSfqmnUINLytMBtJDRKOXEeC1d/z1ET8c1mkJhQKBgQDoU4nfxtMsN1gPkHhdvvH1xcPB/mUknjzCi/336YDfmABtgZ4kcK7HVDhDW7+JnB1m66xptCHfCWiIkWQwxDK19h1wChigxbqY1aTkvlGMQQaNQYVHxPMPjqhl1WxCt20IW72dRpmruPGMggZmhBiX5vmIOIl87VHLvCo0c9FdJQKBgQCwCjkszWCRPhKMxIHL65HKR08ylTR0EU2K1+aNEY02VcNdANnQ4POxKWEi9Eo/Zw45ZTYtS31J+6XOMPFHAGrKQ5CEGcmO/aOSNnAPpG4LspzaI0Zzl8O77mPxI2NoZt0ujmMc4x/XhzOILigENx3D6kUi1VasWRZPkOvmNF1G1QKBgQCNTWHqDM+bcP3KWaAbxGr9hI8Pil6R6vwhh2usQQT0+UopUFCS8UYcTgj6Tu8sDxuC4Yw3rrO4p8xAY82AK5R8P3igEEPyZNCc7DQiO+71UwddGqCpigwbRjT92tTBrzZNgx7MbYhBfXbMcrjZ2TXsDbtvMpPMu7qoI4W36UlJUQKBgQCzRtp0q5bq9mMic01F17Hhq5xnGExs3EMA18USh4p0Xh2eX0klKI2CskPPr7uRUiuTTg0o4dZ+W91hIQw3WVTFWfod1KfijjV2RaFqE9iW1/iCarTC3NOCtPIr8iJZmPRbqQH8Ja4GOsTza50y5eo+YRmwhWcLFbA7/WTcvNZ5qw==";

        [TestMethod()]
        public void GetCertStringFromFileTest()
        {
            var certPath = Senparc.CO2NET.Utilities.ServerUtility.ContentRootMapPath("~/apiclient_key.pem");
            var certContent = Senparc.CO2NET.Helpers.EncryptHelper.GetCertStringFromFile(certPath);

            Console.WriteLine(certContent);

            Assert.AreEqual(EXCEPT_RESULT, certContent);
        }

        [TestMethod]
        public void GetCertStringTest()
        {
            var fileContent = @"-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDee2pWWXyO6nhE
df5nj/MvQj0/tDNBrwCT+JK38ZY8xIzNbIj7J2+oBPimkFpjtETRr1YoV4BDSxt0
/3E0Rdb6+gTGhdYavXM7aYA7qY/EA1eI81Zp32Hfsvnl07tFybA1W/m2CRlJQIlm
XBYurd9F0Os3MSZjRBoWyrd9ISgsKcco+wdeJkxGW12MoYNVZutAlXZBoNDisjZL
c2koCi7fDspsjezOTUcBnbKtTohMB7lYaFORMItELXCQ6rHwIE8zO3sYzHIkM3MR
8ilSJ59CnLh0R53uqkQJ7AGVgbv97XsUMjnPiWT4B9xzvR4KGWnMs0tBr/J/0mfZ
lRqqOLE5AgMBAAECggEAL9XjUDufX28kervP/l5iEDgyyR6qoqXI/wfELA6imeA8
0EFTJYUeKccf21hQRv28ikUjxjrCFjXw6l/97BpUFdRp8HFYTpmLTCvr6WgUxDVf
vc9sNglUlu95caPrsR6jZ2WmNDCSokBhCoQkNNcnmXBJEq3brh43ac0eVKYraAsQ
4G+CHhdzmbB8XLc+lg2qekqXOLiNRNCm9FAJ1669EiJrK2ry9rWFZvP9d5DuyqL9
djX0ff+DqUvvQiV3usQlmeJdRVAgajWJYyVec0g442wY5xKfGZ0K+l8wp2ne5nxz
7F6XBONWpTrpO8EUVN0ix5nnzBvQKSNDaIeFncEkMQKBgQD1JxSP+Ky3I1dGQpXp
MXC3WOYiJyZqv5BmH3A+XKErEmEXwa0CfXnxhnS0yA8Ipp4SFuu3ynynhw/2/Ca1
fXd3TRG0Xg0oVNSbN8Oscrj8GnCu2yCM/RM/5sJFyc/sH+hExyApSfqmnUINLytM
BtJDRKOXEeC1d/z1ET8c1mkJhQKBgQDoU4nfxtMsN1gPkHhdvvH1xcPB/mUknjzC
i/336YDfmABtgZ4kcK7HVDhDW7+JnB1m66xptCHfCWiIkWQwxDK19h1wChigxbqY
1aTkvlGMQQaNQYVHxPMPjqhl1WxCt20IW72dRpmruPGMggZmhBiX5vmIOIl87VHL
vCo0c9FdJQKBgQCwCjkszWCRPhKMxIHL65HKR08ylTR0EU2K1+aNEY02VcNdANnQ
4POxKWEi9Eo/Zw45ZTYtS31J+6XOMPFHAGrKQ5CEGcmO/aOSNnAPpG4LspzaI0Zz
l8O77mPxI2NoZt0ujmMc4x/XhzOILigENx3D6kUi1VasWRZPkOvmNF1G1QKBgQCN
TWHqDM+bcP3KWaAbxGr9hI8Pil6R6vwhh2usQQT0+UopUFCS8UYcTgj6Tu8sDxuC
4Yw3rrO4p8xAY82AK5R8P3igEEPyZNCc7DQiO+71UwddGqCpigwbRjT92tTBrzZN
gx7MbYhBfXbMcrjZ2TXsDbtvMpPMu7qoI4W36UlJUQKBgQCzRtp0q5bq9mMic01F
17Hhq5xnGExs3EMA18USh4p0Xh2eX0klKI2CskPPr7uRUiuTTg0o4dZ+W91hIQw3
WVTFWfod1KfijjV2RaFqE9iW1/iCarTC3NOCtPIr8iJZmPRbqQH8Ja4GOsTza50y
5eo+YRmwhWcLFbA7/WTcvNZ5qw==
-----END PRIVATE KEY-----";

            var certContent = Senparc.CO2NET.Helpers.EncryptHelper.GetCertString(fileContent);

            Console.WriteLine(certContent);

            Assert.AreEqual(EXCEPT_RESULT, certContent);
        }

        #endregion
    }
}
