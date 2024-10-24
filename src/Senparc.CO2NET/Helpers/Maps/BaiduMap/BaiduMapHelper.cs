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
    
    FileName：BaiduMapHelper.cs
    File Function Description：Baidu Map Static Image API
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Refactor interface
----------------------------------------------------------------*/

/*
     Documentation：http://api.map.baidu.com/lbsapi/cloud/staticimg.htm
 */

using Senparc.CO2NET.Helpers.BaiduMap;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// Baidu Map Static Image API, Documentation：http://api.map.baidu.com/lbsapi/cloud/staticimg.htm
    /// </summary>
    public static class BaiduMapHelper
    {
        /// <summary>
        /// Get Baidu Map Static Image
        /// </summary>
        /// <param name="lng">Center longitude</param>
        /// <param name="lat">Center latitude</param>
        /// <param name="scale">The size of the returned image will be adjusted according to this flag. The value range is 1 or 2:
        ///1 means the returned image size is size= width * height;
        ///2 means the returned image is (width*2)*(height *2), and zoom is increased by 1
        ///Note: If zoom is at the maximum level, the returned image is (width*2)*(height*2), and zoom remains unchanged.</param>
        /// <param name="zoom">Map level. HD map range [3, 18]; SD map range [3,19]</param>
        /// <param name="markersList">Marker list, if null, no markers will be output</param>
        /// <param name="width">Image width. Value range: (0, 1024].</param>
        /// <param name="height">Image height. Value range: (0, 1024].</param>
        /// <returns></returns>
        public static string GetBaiduStaticMap(double lng, double lat, int scale, int zoom, IList<BaiduMarkers> markersList, int width = 400, int height = 300)
        {
            var url = new StringBuilder();
            url.Append("http://api.map.baidu.com/staticimage?");

            url.AppendFormat("center={0},{1}", lng, lat);
            url.AppendFormat("&width={0}", width);
            url.AppendFormat("&height={0}", height);
            url.AppendFormat("&scale={0}", scale);
            url.AppendFormat("&zoom={0}", zoom);

            if (markersList != null && markersList.Count > 0)
            {
                url.AppendFormat("&markers={0}", string.Join("|", markersList.Select(z => string.Format("{0},{1}", z.Longitude, z.Latitude)).ToArray()));
                url.AppendFormat("&markerStyles={0}", string.Join("|", markersList.Select(z => string.Format("{0},{1},{2}", z.Size.ToString(), z.Label, z.Color)).ToArray()));
            }

            return url.ToString();
        }
    }
}
