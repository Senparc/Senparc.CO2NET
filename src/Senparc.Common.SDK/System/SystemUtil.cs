/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SystemUtil.cs
    文件功能描述：Senparc.Common.SDK 资源消息处理


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using Senparc.Common.SDK;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 封装Messages_CN资源文件,使其改动就能够处理语言信息
    /// </summary>
    public static class SystemUtil
    {
        /// <summary>
        /// 根据statuscode返回查询到的汉字信息并且提交给前台
        /// </summary>
        /// <param name="statuscode">标识符</param>
        /// <returns></returns>
        public static string getMessage(int statuscode)
        {
            string result = Message_CN.OK;
            switch (statuscode)
            {
                //系统错误(1000以内)
                case 200:
                    result = Message_CN.OK;
                    break;
                case 201:
                    result = Message_CN.Error;
                    break;
                case 202:
                    result = Message_CN.Empty;
                    break;
                case 301:
                    result = Message_CN.NoLogin;
                    break;
                case 400:
                    result = Message_CN.NoAddress;
                    break;
                case 500:
                    result = Message_CN.ServerData;
                    break;
                case 800:
                    result = Message_CN.ParaNoPass;
                    break;
                case 803:
                    result = Message_CN.NoError;
                    break;
                case 901:
                    result = Message_CN.SystemError;
                    break;
                case 903:
                    result = Message_CN.FileInfoNoExist;
                    break;
                case 904:
                    result = Message_CN.UploadFileWriteFailed;
                    break;
                //用户错误信息
                case 1000:
                    result = Message_CN.UserPwdNoError;
                    break;
                case 1001:
                    result = Message_CN.UserNoExist;
                    break;
                case 1002:
                    result = Message_CN.UserIsExist;
                    break;
                case 1003:
                    result = Message_CN.UserDelete;
                    break;
                case 1004:
                    result = Message_CN.OldPwdSameAsNewPwd;
                    break;
                case 1005:
                    result = Message_CN.UserBeBanned;
                    break;
                case 1006:
                    result = Message_CN.RegisterVerificationCodeNoExist;
                    break;
                case 1007:
                    result = Message_CN.UserNoLogin;
                    break;
                default:
                    result = Message_CN.NoError;
                    break;
            }
            return result;
        }
    }
}