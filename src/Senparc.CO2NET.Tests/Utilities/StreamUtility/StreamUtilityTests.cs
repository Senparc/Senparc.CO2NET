using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Utilities;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class StreamUtilityTests
    {
        string str = "Senparc is by your side"; 
        string baseString = "77u/U2VucGFyYyBpcyBieSB5b3VyIHNpZGU=";

        private MemoryStream GetStream(string content)
        {
            var ms = new MemoryStream();// Simulate an existing Stream
            var sw = new StreamWriter(ms, encoding: Encoding.UTF8);// Write
            sw.Write(content);
            sw.Flush();
            return ms;
        }

        [TestMethod]
        public void GetBase64StringTest()
        {
            {
                var result = StreamUtility.GetBase64String(GetStream(str));
                Assert.AreEqual(baseString, result);
            }

            #region Test Asynchronous Method  
            Console.WriteLine("=== Test Asynchronous Method ===");
            Task.Run(async () =>
            {
                var result = await StreamUtility.GetBase64StringAsync(GetStream(str));
                Assert.AreEqual(baseString, result);
                Console.WriteLine("=== Asynchronous Completed ===");
            }).GetAwaiter().GetResult();
            #endregion  
        }

            [TestMethod]
        public void GetStreamFromBase64String()
        {
            {
                // Create a new file
                var ms = StreamUtility.GetStreamFromBase64String(baseString, null);

                Assert.AreEqual(26, ms.Length);

                // Open a file
                var file = UnitTestHelper.RootPath + "GetStreamFromBase64String.txt";
                var ms2 = StreamUtility.GetStreamFromBase64String(baseString, file);
                Assert.AreEqual(26, ms.Length);

                Assert.IsTrue(File.Exists(file));

                // Read a file
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var content = sr.ReadToEnd();
                        Assert.AreEqual(str, content);
                    }
                }

                File.Delete(file);// Delete a file
            }


            #region Test Asynchronous Method  
            Console.WriteLine("=== Asynchronous Completed ===");
            Task.Run(async () =>
            {
                // Create a new file
                var ms = await StreamUtility.GetStreamFromBase64StringAsync(baseString, null);
                Assert.AreEqual(26, ms.Length);

                // Open a file
                var file = UnitTestHelper.RootPath + "GetStreamFromBase64String_Async.txt";
                var ms2 = await StreamUtility.GetStreamFromBase64StringAsync(baseString, file);
                Assert.AreEqual(26, ms.Length);

                Assert.IsTrue(File.Exists(file));

                // Read a file
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var content = await sr.ReadToEndAsync();
                        Assert.AreEqual(str, content);
                    }
                }

                File.Delete(file);// Delete a file

                Console.WriteLine("=== Asynchronous Completed ===");
            }).GetAwaiter().GetResult();

            #endregion
        }

        [TestMethod]
        public void SaveFileFromStreamTest()
        {
            {
                var stream = GetStream(str);
                var file = UnitTestHelper.RootPath + "SaveFileFromStreamTest.txt";
                StreamUtility.SaveFileFromStream(stream, file);

                Assert.IsTrue(File.Exists(file));
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(file, str));// The file has already been recorded

                File.Delete(file);// Delete a file
            }

            #region Test Asynchronous Method  
            Task.Run(async () =>
            {
                var stream = GetStream(str);
                var file = UnitTestHelper.RootPath + "SaveFileFromStreamTest.txt";
                await StreamUtility.SaveFileFromStreamAsync(stream, file);

                Assert.IsTrue(File.Exists(file));
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(file, str));// The file has already been recorded

                File.Delete(file);// Delete a file
                Console.WriteLine("=== Asynchronous Completed ===");
            }).GetAwaiter().GetResult();
            #endregion
        }
    }
}
