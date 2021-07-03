using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Senparc.CO2NET.WebApi
{
    public class DocMembersCollectionValue
    {
        public DocMembersCollectionValue(/*XElement element,*/ XAttribute nameAttr, string paramsPart)
        {
            //Element = element;
            NameAttr = nameAttr;
            ParamsPart = paramsPart;
        }

        ///// <summary>
        ///// Xml 节点
        ///// </summary>
        //public XElement Element => NameAttr.Parent;
        /// <summary>
        /// name 属性，如： name="M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
        /// </summary>
        public XAttribute NameAttr { get; }

        /// <summary>
        /// 参数部分，如：(System.String,System.String,System.String,System.Int32)
        /// </summary>
        public string ParamsPart { get; }
    }
}
