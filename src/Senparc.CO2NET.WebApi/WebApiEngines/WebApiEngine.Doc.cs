using Senparc.CO2NET.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Senparc.CO2NET.WebApi
{
    public partial class WebApiEngine
    {
        #region Doc 文档相关

        /// <summary>
        /// 获取全局统一 docName
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string GetDocName(string category)
        {
            return $"{category}-v{ApiAssemblyVersions[category]}";
        }

        /// <summary>
        /// 检查并创建物理目录
        /// </summary>
        /// <param name="appDataPath">App_Data 文件夹路径</param>
        internal void TryCreateDir(string appDataPath)
        {
            //WriteLog($"检查目录：{appDataPath}");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
                WriteLog($"创建目录：{appDataPath}");
            }
        }

        /// <summary>
        /// 从 xml 中获取方法名和参数的正则
        /// </summary>
        private static Regex regexForDoc = new Regex(@"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})", RegexOptions.Compiled);

        /// <summary>
        /// 获取 DocName
        /// </summary>
        /// <param name="nameAttr"></param>
        /// <returns></returns>
        public DocMethodInfo GetDocMethodInfo(XAttribute nameAttr)
        {
            //var pattern = @"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})";
            var result = regexForDoc.Match(nameAttr.Value);
            if (result.Success && result.Groups["docName"] != null && result.Groups["paramsPart"] != null)
            {
                return new DocMethodInfo(result.Groups["docName"].Value, result.Groups["paramsPart"].Value);
            }

            return new DocMethodInfo(null, null);

            //以下方法速度略慢：
            /*
            if (xmlAttr.StartsWith("M:"))
            {
                var methodLastIndex = xmlAttr.IndexOf("(");
                return (xmlAttr[2..methodLastIndex], xmlAttr[methodLastIndex..]);
            }

            return (null, null);
            */
        }

        /// <summary>
        /// 获取 XML 中所有 Member
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private async IAsyncEnumerable<XElement> GetXmlMembers(XElement root)
        {
            foreach (var item in root.Element("members").Elements("member"))
            {
                yield return item;
            }
        }
        class ApiXmlInfo
        {
            public XDocument Document { get; set; }
            public ConcurrentDictionary<string, DocMembersCollectionValue> DocMembersCollection { get; set; }
                = new ConcurrentDictionary<string, DocMembersCollectionValue>();
        }

        private static ConcurrentDictionary<string, ApiXmlInfo> sourceApiXmlCollection = new ConcurrentDictionary<string, ApiXmlInfo>();
        private static ConcurrentDictionary<string, XDocument> dynamicApiXmlCollection = new ConcurrentDictionary<string, XDocument>();

        private static ConcurrentDictionary<string, string> omitApiXmlList = new ConcurrentDictionary<string, string>();


        /// <summary>
        /// 生成新的 XML 文档内容
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodInfo"></param>
        /// <param name="tb"></param>
        /// <param name="docMembersCollection"></param>
        /// <returns></returns>
        private async Task BuildXmlDoc(string category, string methodName, MethodInfo methodInfo, TypeBuilder tb)
        {
            if (!BuildXml)
            {
                return;
            }

            //查找文档是否已经缓存
            var sourceAssemblyName = methodInfo.DeclaringType.Assembly.GetName().Name;
            if (omitApiXmlList.ContainsKey(sourceAssemblyName))
            {
                return;//如果确定当前 XML 需要被忽略（如不存在，则不进行处理）
            }

            //获取或初始化 XML 信息
            ApiXmlInfo apiXmlInfo = await TryGetApiXmlInfo(category, sourceAssemblyName).ConfigureAwait(false);
            if (apiXmlInfo == null)
            {
                return;//没有文件存在，忽略
            }

            var dynamicAssemblyName = ApiAssemblyNames[category];
            XDocument dynamicDocument = null;
            dynamicDocument = await TryGetDynamicDocument(dynamicAssemblyName, dynamicDocument).ConfigureAwait(false);

            var xmlMembers = dynamicDocument.Root.Element("members");

            //生成文档
            var docMethodName = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";//以(结尾确定匹配到完整的方法名

            //WriteLog($"\t search for docName:  {docMethodName}");//\t\tSDK Method：{apiMethodInfo.ToString()}

            if (apiXmlInfo.DocMembersCollection.ContainsKey(docMethodName))
            {
                var docMethodInfo = apiXmlInfo.DocMembersCollection[docMethodName];
                // like: "M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
                var newAttrName = $"M:{tb.FullName}.{methodName}{docMethodInfo.ParamsPart}";

                var newElement = new XElement("member");
                newElement.Add(new XAttribute("name", newAttrName));
                newElement.Add(docMethodInfo.Element.Elements());

                xmlMembers.Add(newElement);
                //docMethodInfo.NameAttr.SetValue(newAttrName);
                //WriteLog($"\t change document name:  {attr.Value} -> {newAttrName}");
            }
        }

        /// <summary>
        /// 获取或初始化 ApiXmlInfo
        /// </summary>
        /// <param name="category"></param>
        /// <param name="sourceAssemblyName"></param>
        /// <returns></returns>
        private async Task<ApiXmlInfo> TryGetApiXmlInfo(string category, string sourceAssemblyName)
        {
            ApiXmlInfo apiXmlInfo;
            if (!sourceApiXmlCollection.ContainsKey(sourceAssemblyName))
            {
                #region 使用内嵌资源
                //var sourceName = $"Senparc.Xncf.WeixinManager.App_Data.ApiDocXml.{xmlFileName}";//嵌入资源地址
                //var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceName);
                //var useXml = sourceStream?.Length > 0;
                #endregion
                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                using (await cache.BeginCacheLockAsync("BuildXmlDoc", "sourceApiXmlCollection").ConfigureAwait(false))
                {
                    if (!sourceApiXmlCollection.ContainsKey(sourceAssemblyName))
                    {
                        var xmlFileName = $"{sourceAssemblyName}.xml";//XML 文件名
                        var xmlFilePath = Path.Combine(_docXmlPath, xmlFileName);
                        if (File.Exists(xmlFilePath))
                        {
                            apiXmlInfo = new();
                            using (var fs = new FileStream(xmlFilePath, FileMode.Open))
                            {
                                apiXmlInfo.Document = await XDocument.LoadAsync(fs, LoadOptions.None, Task.Factory.CancellationToken).ConfigureAwait(false);
                            }

                            var root = apiXmlInfo.Document.Root;
                            root.Element("assembly").Element("name").Value = sourceAssemblyName;

                            var docMembers = GetXmlMembers(root);// root.Element("members").Elements("member");

                            double dtlong = 0;

                            //var dtDoc = SystemTime.Now;
                            await foreach (var x in docMembers)
                            {
                                if (x.HasAttributes)
                                {
                                    var nameAttr = x.FirstAttribute;
                                    var dt00 = SystemTime.Now;
                                    var docMethodInfo = GetDocMethodInfo(nameAttr);
                                    dtlong += SystemTime.DiffTotalMS(dt00);
                                    if (docMethodInfo.MethodName != null && docMethodInfo.ParamsPart != null)
                                    {
                                        //记录索引信息
                                        apiXmlInfo.DocMembersCollection[docMethodInfo.MethodName] = new DocMembersCollectionValue(/*x, */nameAttr, docMethodInfo.ParamsPart);
                                        //Console.WriteLine("record docMembersCollection:" + docMethodInfo.MethodName);

                                        //记录接口信息，用于搜索
                                        var isAsync = docMethodInfo.MethodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase) ||
                                                        docMethodInfo.MethodName.Contains("Async``", StringComparison.OrdinalIgnoreCase);//是否是异步方法
                                        _findWeixinApiService.Value.RecordApiItem(category, docMethodInfo.MethodName, docMethodInfo.ParamsPart,
                                            x.Element("summary")?.Value, isAsync);
                                    }
                                }
                            }
                            sourceApiXmlCollection[sourceAssemblyName] = apiXmlInfo;
                        }
                        else
                        {
                            //没有文件存在，忽略
                            omitApiXmlList.TryAdd(sourceAssemblyName, null);
                            return null;
                        }
                    }
                    else
                    {
                        apiXmlInfo = sourceApiXmlCollection[sourceAssemblyName];
                    }

                    //WriteLog($"docMembersCollection init cost:{SystemTime.DiffTotalMS(dtDoc)}ms");
                    //WriteLog("Document Root Name:" + root.Name);
                    //WriteLog($"find docMembers:{docMembers.Count()}");
                }
            }
            else
            {
                apiXmlInfo = sourceApiXmlCollection[sourceAssemblyName];
            }
            return apiXmlInfo;
        }


        /// <summary>
        /// 获取动态生成的 XML
        /// </summary>
        /// <param name="dynamicAssemblyName"></param>
        /// <param name="dynamicDocument"></param>
        /// <returns></returns>
        private static async Task<XDocument> TryGetDynamicDocument(string dynamicAssemblyName, XDocument dynamicDocument)
        {
            if (!dynamicApiXmlCollection.ContainsKey(dynamicAssemblyName))
            {
                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                using (await cache.BeginCacheLockAsync("BuildXmlDoc", "dynamicApiXmlCollection").ConfigureAwait(false))
                {
                    if (!dynamicApiXmlCollection.ContainsKey(dynamicAssemblyName))
                    {
                        XElement root = new("doc");
                        root.Add(new XElement("assembly", new XElement("name", dynamicAssemblyName)));
                        root.Add(new XElement("members"));
                        dynamicDocument = new(root);
                        dynamicApiXmlCollection[dynamicAssemblyName] = dynamicDocument;
                    }
                    else
                    {
                        dynamicDocument = dynamicApiXmlCollection[dynamicAssemblyName];
                    }
                }
            }
            else
            {
                dynamicDocument = dynamicApiXmlCollection[dynamicAssemblyName];
            }

            return dynamicDocument;
        }


        /// <summary>
        /// 保存动态接口的 XML 文件
        /// </summary>
        /// <returns></returns>
        internal void SaveDynamicApiXml()
        {
            if (!BuildXml)
            {
                return;
            }

            var dynamicFilePath = GetDynamicFilePath(_docXmlPath);
            if (!Directory.Exists(dynamicFilePath))
            {
                Directory.CreateDirectory(dynamicFilePath);
            }

            foreach (var item in dynamicApiXmlCollection)
            {
                #region 保存新的 Xml 文件


                var newDocFileName = Path.Combine(dynamicFilePath, $"{item.Key}.xml");

                Console.WriteLine("newDocFileName：" + newDocFileName);
                try
                {
                    //using (XmlWriter xw = XmlWriter.Create(newDocFile, new XmlWriterSettings() { Async = true }))
                    //{
                    //    await document.SaveAsync(xw, new CancellationToken()).ConfigureAwait(false);//保存
                    //}
                    item.Value.Save(newDocFileName);

                    WriteLog($"new document file saved: {newDocFileName}");
                }
                catch (Exception ex)
                {
                    WriteLog($"save document xml faild: {ex.Message}\r\n{ex}");
                }

                #endregion
            }
        }

        #endregion
    }
}
