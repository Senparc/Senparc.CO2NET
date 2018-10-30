/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：FileQuickSorter.cs
    文件功能描述：Senparc.Common.SDK 文件快速排序类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 文件快速排序类
    /// </summary>
    public class FileQuickSorter
    {
        public FileQuickSorter()
        {
        }

        /// <summary>
        /// 快速排序算法
        /// </summary>
        /// 快速排序为不稳定排序,时间复杂度O(nlog2n),为同数量级中最快的排序方法
        /// <param name="arr">划分的数组</param>
        /// <param name="low">数组低端上标</param>
        /// <param name="high">数组高端下标</param>
        /// <returns></returns>
        static int Partition(FileInfo[] arr, int low, int high)
        {
            //进行一趟快速排序,返回中心轴记录位置
            // arr[0] = arr[low];
            FileInfo pivot = arr[low];//把中心轴置于arr[0]
            while (low < high)
            {
                while (low < high && arr[high].CreationTime <= pivot.CreationTime)
                    --high;
                //将比中心轴记录小的移到低端
                Swap(ref arr[high], ref arr[low]);
                while (low < high && arr[low].CreationTime >= pivot.CreationTime)
                    ++low;
                Swap(ref arr[high], ref arr[low]);
                //将比中心轴记录大的移到高端
            }
            arr[low] = pivot; //中心轴移到正确位置
            return low;  //返回中心轴位置
        }
        static void Swap(ref FileInfo i, ref FileInfo j)
        {
            FileInfo t;
            t = i;
            i = j;
            j = t;
        }
        /// <summary>
        /// 快速排序算法
        /// </summary>
        /// 快速排序为不稳定排序,时间复杂度O(nlog2n),为同数量级中最快的排序方法
        /// <param name="arr">划分的数组</param>
        /// <param name="low">数组低端上标</param>
        /// <param name="high">数组高端下标</param>
        public static void QuickSort(FileInfo[] arr, int low, int high)
        {
            if (low <= high - 1)//当 arr[low,high]为空或只一个记录无需排序
            {
                int pivot = Partition(arr, low, high);
                QuickSort(arr, low, pivot - 1);
                QuickSort(arr, pivot + 1, high);
            }
        }
    }
}
