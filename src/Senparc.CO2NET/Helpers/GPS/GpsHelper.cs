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
    
    FileName：GpsHelper.cs
    File Function Description：Handle coordinate distance
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface
----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// GPS helper class
    /// </summary>
    public class GpsHelper
    {
        #region Distance Measurement - Method 1

        //The following algorithm reference: https://blog.csdn.net/xiejm2333/article/details/73297004


        /// <summary>
        /// Calculate the distance between two GPS coordinates (unit: meters)
        /// </summary>
        /// <param name="n1">Latitude coordinate of the first point</param>
        /// <param name="e1">Longitude coordinate of the first point</param>
        /// <param name="n2">Latitude coordinate of the second point</param>
        /// <param name="e2">Longitude coordinate of the second point</param>
        /// <returns></returns>
        public static double Distance(double n1, double e1, double n2, double e2)
        {
            //double jl_jd = 102834.74258026089786013677476285;//Meters per longitude unit;
            //double jl_wd = 111712.69150641055729984301412873;//Meters per latitude unit; 
            //double b = Math.Abs((e1 - e2) * jl_jd);
            //double a = Math.Abs((n1 - n2) * jl_wd);
            //return Math.Sqrt((a * a + b * b));


            double Lat1 = Rad(n1); // Latitude
            double Lat2 = Rad(n2);

            double a = Lat1 - Lat2;//Difference in latitude between two points
            double b = Rad(e1) - Rad(e2); //Difference in longitude

            double s = 2 * Math.Asin(Math

                          .Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Pow(Math.Sin(b / 2), 2)));//Formula to calculate the distance between two points

            s = s * 6378137.0;//Arc length multiplied by the Earth's radius (radius in meters)

            s = Math.Round(s * 10000d) / 10000d;//Accurate distance value

            return s;
        }

        private static double Rad(double d)
        {
            return d * Math.PI / 180.00; //Convert degrees to radians
        }

        #endregion

        #region Distance Measurement - Method 2

        //Reference: https://blog.csdn.net/xiejm2333/article/details/73297004

        public static double HaverSin(double theta)
        {
            var v = Math.Sin(theta / 2);
            return v * v;
        }

        //Baidu Encyclopedia: 6371.393 km: https://baike.baidu.com/item/%E5%9C%B0%E7%90%83%E5%8D%8A%E5%BE%84/1037801?fr=aladdin
        static double EARTH_RADIUS = 6371393.0;//m Earth's radius average value, kilometers / also data: 6378137.0 meters

        /// <summary>
        /// Given longitude1, latitude1; longitude2, latitude2. Calculate the distance between two coordinates.
        /// </summary>
        /// <param name="lat1">Longitude1</param>
        /// <param name="lon1">Latitude1</param>
        /// <param name="lat2">Longitude2</param>
        /// <param name="lon2">Latitude2</param>
        /// <returns>Distance (kilometers)</returns>
        public static double Distance2(double lat1, double lon1, double lat2, double lon2)
        {
            //Use the haversine formula to calculate the distance between two points on the sphere.
            //Convert latitude and longitude to radians
            lat1 = ConvertDegreesToRadians(lat1);
            lon1 = ConvertDegreesToRadians(lon1);
            lat2 = ConvertDegreesToRadians(lat2);
            lon2 = ConvertDegreesToRadians(lon2);

            //Difference
            var vLon = Math.Abs(lon1 - lon2);
            var vLat = Math.Abs(lat1 - lat2);

            //h is the great circle distance in radians, great circle is a plane section of a sphere, its center is the center of the sphere, and it has the largest circumference.
            var h = HaverSin(vLat) + Math.Cos(lat1) * Math.Cos(lat2) * HaverSin(vLon);

            var distance = 2 * EARTH_RADIUS * Math.Asin(Math.Sqrt(h));

            return distance;
        }

        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns>Radians</returns>
        public static double ConvertDegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double ConvertRadiansToDegrees(double radian)
        {
            return radian * 180.0 / Math.PI;
        }


        #endregion

        /// <summary>
        /// Get latitude difference
        /// </summary>
        /// <param name="km">Kilometers</param>
        /// <returns></returns>
        public static double GetLatitudeDifference(double km)
        {
            return km * 1 / 111;
        }

        /// <summary>
        /// Get longitude difference
        /// </summary>
        /// <param name="km">Kilometers</param>
        /// <returns></returns>
        public static double GetLongitudeDifference(double km)
        {
            return km * 1 / 110;
        }
    }
}
