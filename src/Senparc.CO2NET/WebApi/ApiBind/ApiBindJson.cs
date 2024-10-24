namespace Senparc.CO2NET.ApiBind
{
    /// <summary>
    /// ApiBind Json format settings
    /// </summary>
    public class ApiBindJson
    {
        /// <summary>
        /// Globally unique name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Parameters
        /// </summary>
        public object[] parameters { get; set; }
    }
}
