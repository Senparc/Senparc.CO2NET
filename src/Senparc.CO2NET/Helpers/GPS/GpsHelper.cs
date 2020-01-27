#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc
    
    文件名：GpsHelper.cs
    文件功能描述：处理坐标距离
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// GPS 帮助类
    /// </summary>
    public class GpsHelper
    {
        #region 测距 - 方法1

        //以下算法参考：https://blog.csdn.net/xiejm2333/article/details/73297004


        /// <summary>
        /// 计算两点GPS坐标的距离（单位：米）
        /// </summary>
        /// <param name="n1">第一点的纬度坐标</param>
        /// <param name="e1">第一点的经度坐标</param>
        /// <param name="n2">第二点的纬度坐标</param>
        /// <param name="e2">第二点的经度坐标</param>
        /// <returns></returns>
        public static double Distance(double n1, double e1, double n2, double e2)
        {
            //double jl_jd = 102834.74258026089786013677476285;//每经度单位米;
            //double jl_wd = 111712.69150641055729984301412873;//每纬度单位米; 
            //double b = Math.Abs((e1 - e2) * jl_jd);
            //double a = Math.Abs((n1 - n2) * jl_wd);
            //return Math.Sqrt((a * a + b * b));


            double Lat1 = Rad(n1); // 纬度
            double Lat2 = Rad(n2);

            double a = Lat1 - Lat2;//两点纬度之差
            double b = Rad(e1) - Rad(e2); //经度之差

            double s = 2 * Math.Asin(Math

                          .Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Pow(Math.Sin(b / 2), 2)));//计算两点距离的公式

            s = s * 6378137.0;//弧长乘地球半径（半径为米）

            s = Math.Round(s * 10000d) / 10000d;//精确距离的数值

            return s;
        }

        private static double Rad(double d)
        {
            return d * Math.PI / 180.00; //角度转换成弧度
        }

        #endregion

        #region 测距 方法2

        //参考：https://blog.csdn.net/xiejm2333/article/details/73297004

        public static double HaverSin(double theta)
        {
            var v = Math.Sin(theta / 2);
            return v * v;
        }

        //百度百科： 6371.393 km ：https://baike.baidu.com/item/%E5%9C%B0%E7%90%83%E5%8D%8A%E5%BE%84/1037801?fr=aladdin
        static double EARTH_RADIUS = 6371393.0;//m 地球半径 平均值，千米 / 也有数据：6378137.0米

        /// <summary>
        /// 给定的经度1，纬度1；经度2，纬度2. 计算2个经纬度之间的距离。
        /// </summary>
        /// <param name="lat1">经度1</param>
        /// <param name="lon1">纬度1</param>
        /// <param name="lat2">经度2</param>
        /// <param name="lon2">纬度2</param>
        /// <returns>距离（公里、千米）</returns>
        public static double Distance2(double lat1, double lon1, double lat2, double lon2)
        {
            //用haversine公式计算球面两点间的距离。
            //经纬度转换成弧度
            lat1 = ConvertDegreesToRadians(lat1);
            lon1 = ConvertDegreesToRadians(lon1);
            lat2 = ConvertDegreesToRadians(lat2);
            lon2 = ConvertDegreesToRadians(lon2);

            //差值
            var vLon = Math.Abs(lon1 - lon2);
            var vLat = Math.Abs(lat1 - lat2);

            //h is the great circle distance in radians, great circle就是一个球体上的切面，它的圆心即是球心的一个周长最大的圆。
            var h = HaverSin(vLat) + Math.Cos(lat1) * Math.Cos(lat2) * HaverSin(vLon);

            var distance = 2 * EARTH_RADIUS * Math.Asin(Math.Sqrt(h));

            return distance;
        }

        /// <summary>
        /// 将角度换算为弧度。
        /// </summary>
        /// <param name="degrees">角度</param>
        /// <returns>弧度</returns>
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
        /// 获取维度差
        /// </summary>
        /// <param name="km">千米</param>
        /// <returns></returns>
        public static double GetLatitudeDifference(double km)
        {
            return km * 1 / 111;
        }

        /// <summary>
        /// 获取经度差
        /// </summary>
        /// <param name="km">千米</param>
        /// <returns></returns>
        public static double GetLongitudeDifference(double km)
        {
            return km * 1 / 110;
        }
    }
}
