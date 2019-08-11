/*----------------------------------------------------------------
    Copyright(C) 2018 Senparc

    文件名：FormFileData.cs
    文件功能描述：FormFileData 的异常


    创建标识：Senparc - 20190811

----------------------------------------------------------------*/

using Senparc.CO2NET.Utilities.HttpUtility.HttpPost;
using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// FormFileData 的异常
    /// </summary>
    public class FileValueException : BaseException
    {
        /// <summary>
        /// FormFileData 的异常
        /// </summary>
        /// <param name="formFileData">FormFileData实体，不可为 null</param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public FileValueException(FormFileData formFileData, string message, Exception inner = null, bool logged = false)
            : base($"FormFileData 异常：{message}，FileName：{formFileData.FileName}", inner, logged)
        {
        }
    }
}
