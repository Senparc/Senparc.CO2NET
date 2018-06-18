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
        string str = "盛派在你身边";
        string baseString = "77u/55ub5rS+5Zyo5L2g6Lqr6L65";

        private MemoryStream GetStream(string content)
        {
            var ms = new MemoryStream();//模拟一个已经存在的Stream
            var sw = new StreamWriter(ms, encoding: Encoding.UTF8);//写入
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

            #region 测试异步方法

            Console.WriteLine("=== 测试异步方法 ===");
            Task.Run(async () =>
            {
                var result = await StreamUtility.GetBase64StringAsync(GetStream(str));
                Assert.AreEqual(baseString, result);

                Console.WriteLine("=== 异步完成 ===");
            }).GetAwaiter().GetResult();

            #endregion
        }

        [TestMethod]
        public void GetStreamFromBase64String()
        {
            {
                //不保存文件
                var ms = StreamUtility.GetStreamFromBase64String(baseString, null);

                Assert.AreEqual(21, ms.Length);

                //保存文件
                var file = UnitTestHelper.RootPath + "GetStreamFromBase64String.txt";
                var ms2 = StreamUtility.GetStreamFromBase64String(baseString, file);
                Assert.AreEqual(21, ms.Length);

                Assert.IsTrue(File.Exists(file));

                //读取文件
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var content = sr.ReadToEnd();
                        Assert.AreEqual(str, content);
                    }
                }

                File.Delete(file);//删除文件
            }


            #region 测试异步方法
            Console.WriteLine("=== 测试异步方法 ===");
            Task.Run(async () =>
            {
                //不保存文件
                var ms = await StreamUtility.GetStreamFromBase64StringAsync(baseString, null);
                Assert.AreEqual(21, ms.Length);

                //保存文件
                var file = UnitTestHelper.RootPath + "GetStreamFromBase64String_Async.txt";
                var ms2 = await StreamUtility.GetStreamFromBase64StringAsync(baseString, file);
                Assert.AreEqual(21, ms.Length);

                Assert.IsTrue(File.Exists(file));

                //读取文件
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        var content = await sr.ReadToEndAsync();
                        Assert.AreEqual(str, content);
                    }
                }

                File.Delete(file);//删除文件

                Console.WriteLine("=== 异步完成 ===");
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
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(file, str));//检查内容已经被记录

                File.Delete(file);//删除文件
            }

            #region 测试异步方法
            Task.Run(async () =>
            {
                var stream = GetStream(str);
                var file = UnitTestHelper.RootPath + "SaveFileFromStreamTest.txt";
                await StreamUtility.SaveFileFromStreamAsync(stream, file);

                Assert.IsTrue(File.Exists(file));
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(file, str));//检查内容已经被记录

                File.Delete(file);//删除文件
                Console.WriteLine("=== 异步完成 ===");
            }).GetAwaiter().GetResult();
            #endregion
        }
    }
}
