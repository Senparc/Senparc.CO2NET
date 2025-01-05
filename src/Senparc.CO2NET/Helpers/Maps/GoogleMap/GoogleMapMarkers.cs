#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc
    
    FileName：GoogleMapMarkers.cs
    File Function Description：Google Map
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface
----------------------------------------------------------------*/

namespace Senparc.CO2NET.Helpers.GoogleMap
{
    /// <summary>
    /// Marker size
    /// </summary>
    public enum GoogleMapMarkerSize
    {
        Default = mid,
        tiny = 0, mid = 1, small = 2
    }

    /// <summary>
    /// Google Map Marker
    /// </summary>
    public class GoogleMapMarkers
    {
        /// <summary>
        /// (Optional) Specify the marker size from the set {tiny, mid, small}. If the size parameter is not set, the marker will be displayed in its default (regular) size.
        /// </summary>
        public GoogleMapMarkerSize Size { get; set; }
        /// <summary>
        /// (Optional) Specify a 24-bit color (e.g., color=0xFFFFCC) or one of the predefined colors from the set {black, brown, green, purple, yellow, blue, gray, orange, red, white}.
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// (Optional) Specify an uppercase alphanumeric character from the set {A-Z, 0-9}.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        public double Y { get; set; }
    }
}
