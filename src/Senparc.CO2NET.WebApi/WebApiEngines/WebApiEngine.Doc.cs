using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
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
            var dir = Path.Combine(/*SiteConfig.WebRootPath, "..", "App_Data",*/ appDataPath, "ApiDocXml");// ServerUtility.ContentRootMapPath("~/App_Data/ApiDocXml");
            WriteLog($"检查目录：{dir}");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                WriteLog($"创建目录：{dir}");
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
            var pattern = @"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})";
            var result = regexForDoc.Match(pattern);
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


        private void BuildXmlDoc(string methodName, MethodInfo methodInfo, TypeBuilder tb, ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection)
        {
            //生成文档
            var docMethodName = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";//以(结尾确定匹配到完整的方法名

            WriteLog($"\t search for docName:  {docMethodName}");//\t\tSDK Method：{apiMethodInfo.ToString()}

            if (docMembersCollection.ContainsKey(docMethodName))
            {
                var docMethodInfo = docMembersCollection[docMethodName];
                // like: "M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
                var newAttrName = $"M:{tb.FullName}.{methodName}{docMethodInfo.ParamsPart}";

                docMethodInfo.NameAttr.SetValue(newAttrName);
                //WriteLog($"\t change document name:  {attr.Value} -> {newAttrName}");
            }
        }

        #endregion

    }
}
