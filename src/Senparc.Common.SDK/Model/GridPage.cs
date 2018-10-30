/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：GridPage.cs
    文件功能描述：Senparc.Common.SDK 表格分页


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 表格分页
    /// </summary>
    public class GridPage
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        public string orderType { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string orderField { get; set; }
        /// <summary>
        /// 每页行数
        /// </summary>
        public int pageRows { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int curPage { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int totalRecords { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotaPage
        {
            get
            {
                if (totalRecords > 0)
                {
                    return this.totalRecords % this.pageRows == 0 ? this.totalRecords / this.pageRows : this.totalRecords / this.pageRows + 1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}
