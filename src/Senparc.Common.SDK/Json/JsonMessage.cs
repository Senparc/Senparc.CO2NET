/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：JsonMessage.cs
    文件功能描述：Senparc.Common.SDK 返回消息类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 返回消息
    /// </summary>
    public class JsonMessage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 连连支付返回json格式
    /// </summary>
    public class LLPayJsonMessage
    {
        /// <summary>
        /// 交易结果代码
        /// </summary>
        public string ret_code { get; set; }
        /// <summary>
        /// 交易结果描述
        /// </summary>
        public string ret_msg { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 返回带模型的Json信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonMessage<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回的数据模型
        /// </summary>
        public T Data { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 返回带模型的Json信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonListMessage<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回对象模型
        /// </summary>
        public Object Obj { get; set; }
        /// <summary>
        /// 返回的数据模型
        /// </summary>
        public List<T> Data { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 返回Json信息
    /// </summary>
    public class JsonListMessage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回的数据模型
        /// </summary>
        public string Data { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 返回Json信息
    /// </summary>
    public class JsonDicMessage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回的数据模型
        /// </summary>
        public Dictionary<object, object> Data { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }

    /// <summary>
    /// 返回带对象的Json信息
    /// </summary>
    public class JsonObjectMessage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 结果编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回的数据模型
        /// </summary>
        public object Data { get; set; }

        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }
}
