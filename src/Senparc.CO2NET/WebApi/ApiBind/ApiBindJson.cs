namespace Senparc.CO2NET.ApiBind
{
    /// <summary>
    /// ApiBind Json 设置格式
    /// </summary>
    public class ApiBindJson
    {
        /// <summary>
        /// 全局唯一名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object[] parameters { get; set; }
    }
}
