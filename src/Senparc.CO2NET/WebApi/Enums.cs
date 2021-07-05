namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// 默认 API 的请求方式
    /// <para>参考：<see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods"/></para>
    /// </summary>
    public enum ApiRequestMethod
    {
        GlobalDefault,
        Get,
        Head,
        Post,
        Put,
        Delete,
        //Connect,
        Options,
        //Trace,
        Patch
    }
}
