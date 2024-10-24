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
        ///// Xml node
        ///// </summary>
        public XElement Element => NameAttr.Parent;
        /// <summary>
        /// name attribute, e.g.: name="M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
        /// </summary>
        public XAttribute NameAttr { get; }

        /// <summary>
        /// parameter part, e.g.: (System.String,System.String,System.String,System.Int32)
        /// </summary>
        public string ParamsPart { get; }
    }
}
