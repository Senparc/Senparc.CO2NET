namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// Default API request method
    /// <para>Reference: <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods"/></para>
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

    /// <summary>
    /// Scope of ApiBind attribute
    /// </summary>
    public enum ApiBindOn
    {
        Class,
        Method
    }
}
