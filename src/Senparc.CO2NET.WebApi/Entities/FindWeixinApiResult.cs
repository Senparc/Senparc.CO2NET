using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// API搜索列表
    /// </summary>
    [Serializable]
    public class FindWeixinApiResult
    {
        public FindWeixinApiResult(string category, bool? isAsync, string keyword, IEnumerable<ApiItem> apiItemList)
        {
            Category = category;
            IsAsync = isAsync;
            Keyword = keyword;
            ApiItemList = apiItemList ?? new List<ApiItem>();
        }

        public string Category { get; set; }
        public bool? IsAsync { get; set; }
        public string Keyword { get; set; }
        public IEnumerable<ApiItem> ApiItemList { get; set; }

    }

    [Serializable]
    public class ApiItem
    {
        public ApiItem(string category, string fullMethodName, string paramsPart, string summary, bool? isAsync)
        {
            Category = category;
            FullMethodName = fullMethodName;
            ParamsPart = paramsPart;
            Summary = summary;
            IsAsync = isAsync;
        }

        public string Category { get; set; }
        /// <summary>
        /// 包含命名空间的方法名称（不包含参数）
        /// </summary>
        public string FullMethodName { get; set; }
        public string ParamsPart { get; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// 是否是异步方法
        /// </summary>
        public bool? IsAsync { get; set; }

    }
}
