/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName: FormFileData.cs
    File Function Description: Exception of FormFileData


    Creation Identifier: Senparc - 20190811

----------------------------------------------------------------*/

using Senparc.CO2NET.Utilities.HttpUtility.HttpPost;
using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// Exception of FormFileData
    /// </summary>
    public class FileValueException : BaseException
    {
        /// <summary>
        /// Exception of FormFileData
        /// </summary>
        /// <param name="formFileData">FormFileData entity, cannot be null</param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public FileValueException(FormFileData formFileData, string message, Exception inner = null, bool logged = false)
            : base($"FormFileData 异常：{message}，FileName：{formFileData.FileName}", inner, logged)
        {
        }
    }
}
