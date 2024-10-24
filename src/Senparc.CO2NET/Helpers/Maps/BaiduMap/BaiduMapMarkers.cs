#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc
    
    FileName：BaiduMapMarkers.cs
    File Function Description：Baidu Map
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Refactor interface
----------------------------------------------------------------*/

namespace Senparc.CO2NET.Helpers.BaiduMap
{
    /// <summary>
    /// Marker size
    /// </summary>
    public enum BaiduMarkerSize
    {
        Default = m,
        s = 0, m = 1, l = 2
    }

    /// <summary>
    /// Baidu Map Marker
    /// </summary>
    public class BaiduMarkers
    {
        /// <summary>
        /// (Optional) Three values: s, m, l.
        /// </summary>
        public BaiduMarkerSize Size { get; set; }
        /// <summary>
        /// (Optional) Color = [0x000000, 0xffffff] or use CSS defined color names.
        /// black 0x000000 
        /// silver 0xC0C0C0 
        /// gray 0x808080 
        /// white 0xFFFFFF 
        /// maroon 0x800000
        /// red 0xFF0000 
        /// purple 0x800080 
        /// fuchsia 0xFF00FF 
        /// green 0x008000
        /// lime 0x00FF00 
        /// olive 0x808000 
        /// yellow 0xFFFF00 
        /// navy 0x000080 
        /// blue 0x0000FF
        /// teal 0x008080 
        /// aqua 0x00FFFF
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// (Optional) Specify a single uppercase alphanumeric character from the set {A-Z, 0-9}. Defaults to A if not specified.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Custom icon URL, currently only supports png32 format. When setting a custom icon, ignore Size, Color, and Label properties, and only set this property with a preceding -1, e.g., markerStyles=-1, http://api.map.baidu.com/images/marker_red.png. Icon size must be less than 5k, otherwise it may fail to load.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Longitude (corresponds to X in GoogleMap)
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Latitude (corresponds to Y in GoogleMap)
        /// </summary>
        public double Latitude { get; set; }
    }
}
