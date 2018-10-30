/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：ImportExcel.cs
    文件功能描述：Senparc.Common.SDK 导入Excel帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 导入Excel帮助类
    /// </summary>
    public class ImportExcel
    {
        /// <summary>
        /// Excel检查版本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string ConnectionString(string fileName)
        {
            bool isExcel2003 = fileName.EndsWith(".xls");
            string connectionString = string.Format(
                isExcel2003
                    ? "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;"
                    : "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES\"",
                fileName);
            return connectionString;
        }

        /// <summary>
        /// Excel导入数据源
        /// </summary>
        /// <param name="sheet">sheet</param>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(string sheet, string filename)
        {
            OleDbConnection myConn = new OleDbConnection(ConnectionString(filename));
            try
            {
                DataSet ds;
                string strCom = " SELECT * FROM [Sheet1$]";
                myConn.Open();
                OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
                ds = new DataSet();
                myCommand.Fill(ds);
                myConn.Close();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                myConn.Close();
                myConn.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Excel导入数据源
        /// </summary>
        /// <param name="sheet">sheet</param>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        public static DataTable ExcelSheetToDataTable(string sheet, string filename, string sheetname)
        {
            OleDbConnection myConn = new OleDbConnection(ConnectionString(filename));
            try
            {
                DataSet ds;
                string strCom = " SELECT * FROM [" + sheetname + "]";
                myConn.Open();
                OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
                ds = new DataSet();
                myCommand.Fill(ds);
                myConn.Close();
                return ds.Tables[0];
            }
            catch(Exception)
            {
                myConn.Close();
                myConn.Dispose();
                throw;
            }
        }

        /*  ExportExcelInfo<T>(IList<T> list, List<string> excelheader, IList<T> listGather,List<string> excludeNoUsePropety)
             * 下面导出Excel的函数经过重构，可以灵活使用，list需要导出的实体集合(必须传递),excelheader需要导出的Excel的表头(必须传递)
             * listGather如果需要将实体汇总显示在Excel最下面，则传递参数，否则不传递，excludeNoUsePropety排除掉某些实体中不需要导出
             * 的字段(可不导出) */

        /// <summary>
        /// NPOI实现导出Excel,传递参数导出集合和导出Excel的表头
        /// </summary>
        /// <typeparam name="T">导出Excel的实体集合</typeparam>
        /// <param name="list">需要导出Excel的List集合</param>
        /// <param name="excelheader">Excel表头</param>
        /// <returns></returns>
        public static byte[] ExportExcelInfo<T>(IList<T> list, List<string> excelheader) where T : class, new()
        {
            if (list.Count == 0 || excelheader.Count == 0)
            {
                throw new Exception("导出Excel的表头和实体必须传递，请您检查");
            }
            IWorkbook workbook = ExportToWorkBookExcel<T>(list, excelheader, null, null);
            //最终导出Excel给用户
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// NPOI实现导出Excel,传递参数导出集合和导出Excel的表头和Excel数据的汇总信息
        /// </summary>
        /// <typeparam name="T">导出Excel的实体集合</typeparam>
        /// <param name="list">需要导出Excel的List集合</param>
        /// <param name="excelheader">Excel表头</param>
        /// <param name="listGather">将list集合进行汇总后的信息</param>
        /// <returns></returns>
        public static byte[] ExportExcelInfo<T>(IList<T> list, List<string> excelheader, IList<T> listGather)
            where T : class, new()
        {
            if (list.Count == 0 || excelheader.Count == 0)
            {
                throw new Exception("导出Excel的表头和实体必须传递，请您检查");
            }
            IWorkbook workbook = ExportToWorkBookExcel<T>(list, excelheader, listGather, null);
            //最终导出Excel给用户
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// NPOI实现导出Excel,传递参数导出集合和导出Excel的表头和Excel数据的汇总信息和实体中不需要的属性排除
        /// </summary>
        /// <typeparam name="T">导出Excel的实体集合</typeparam>
        /// <param name="list">需要导出Excel的List集合</param>
        /// <param name="excelheader">Excel表头</param>
        /// <param name="listGather">将list集合进行汇总后的信息</param>
        /// <param name="excludeNoUsePropety">排除T实体中不需要的属性</param>
        /// <returns></returns>
        public static byte[] ExportExcelInfo<T>(IList<T> list, List<string> excelheader, IList<T> listGather,
            List<string> excludeNoUsePropety) where T : class, new()
        {
            if (list.Count == 0 || excelheader.Count == 0)
            {
                throw new Exception("导出Excel的表头和实体必须传递，请您检查");
            }
            IWorkbook workbook = ExportToWorkBookExcel<T>(list, excelheader, listGather, excludeNoUsePropety);
            //最终导出Excel给用户
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                bytes = memoryStream.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// 导出Excel内部功能实现，提供给前面三个方法进行调用
        /// </summary>
        /// <typeparam name="T">导出Excel的实体集合</typeparam>
        /// <param name="list">需要导出Excel的List集合</param>
        /// <param name="excelheader">Excel表头</param>
        /// <param name="listGather">将list集合进行汇总后的信息</param>
        /// <param name="excludeNoUsePropety">排除T实体中不需要的属性</param>
        /// <returns></returns>
        private static IWorkbook ExportToWorkBookExcel<T>(IList<T> list, List<string> excelheader, IList<T> listGather,
            List<string> excludeNoUsePropety) where T : class, new()
        {
            IWorkbook iWorkbook = new HSSFWorkbook();
            ISheet isSheet = iWorkbook.CreateSheet();
            var typeOf = typeof(T);
            var getProperties = typeOf.GetProperties();
            //第一步：默认首先设置表头信息
            IRow headRow = isSheet.CreateRow(0);
            for (int i = 0; i < excelheader.Count; i++)
            {
                headRow.CreateCell(i, CellType.String).SetCellValue(excelheader[i]);
            }

            //第二步：遍历所有的读取出来的数据添加到Excel中，添加集合
            var cellStyle = (HSSFCellStyle)iWorkbook.CreateCellStyle();
            var format = (HSSFDataFormat)iWorkbook.CreateDataFormat();
            for (int i = 0; i < list.Count; i++)
            {
                IRow temRow = isSheet.CreateRow(i + 1);
                //遍历一行的每一个单元格，填写内容到单元格中
                int num = 0; //控制在第几行显示第几个数据
                //得到显示或者不显示的字段(整个实体类型的集合)，然后循环去显示
                for (int j = 0; j < getProperties.Length; j++)
                {
                    var proName = getProperties[j].Name;
                    if (excludeNoUsePropety.Count != 0)
                    {
                        if (excludeNoUsePropety.Contains(proName))
                        {
                            continue;
                        }
                    }
                    try
                    {
                        var newCell = temRow.CreateCell(num++);
                        var vName = typeOf.GetProperty(proName).GetValue(list[i], null);
                        if (vName != null)
                        {
                            Type type = vName.GetType(); //增加数据类型转换，int和decimal转换后在Excel里面显示正常
                            switch (type.FullName)
                            {
                                case "System.Int32":
                                    newCell.SetCellValue(Convert.ToInt32(vName.ToString()));
                                    cellStyle.DataFormat = format.GetFormat("0");
                                    newCell.CellStyle = cellStyle;
                                    break;
                                case "System.Decimal":
                                    newCell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");
                                    newCell.SetCellValue(Convert.ToDouble(vName.ToString()));
                                    break;
                                default:
                                    newCell.SetCellValue(vName.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            newCell.SetCellValue("");
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new Exception("导出Excel出现错误了，错误原因：" + exception.Message);
                    }
                }
            }
            //第三步：判断是否含有汇总信息，如果含有汇总信息，则显示在Excel的最下面，如果没有汇总信息，则直接返回
            if (listGather.Count != 0)
            {
                IRow footRow = isSheet.CreateRow(list.Count + 1); //创建最后一行用来存放汇总信息
                int num = 0; //控制在第几行显示第几个数据
                for (int j = 0; j < getProperties.Length; j++)
                {
                    var properName = getProperties[j].Name;
                    if (excludeNoUsePropety.Count != 0) //证明需要除掉传递进来不需要的属性
                    {
                        if (excludeNoUsePropety.Contains(properName))
                        {
                            continue;
                        }
                    }
                    var footCell = footRow.CreateCell(num++);
                    var vName = typeOf.GetProperty(properName).GetValue(listGather[0], null);
                    if (vName != null)
                    {
                        Type type = vName.GetType(); //增加数据类型转换
                        switch (type.FullName)
                        {
                            case "System.Int32":
                                footCell.SetCellValue(Convert.ToInt32(vName.ToString()));
                                cellStyle.DataFormat = format.GetFormat("0");
                                footCell.CellStyle = cellStyle;
                                break;
                            case "System.Decimal":
                                footCell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");
                                footCell.SetCellValue(Convert.ToDouble(vName.ToString()));
                                break;
                            default:
                                footCell.SetCellValue(vName.ToString());
                                break;
                        }
                    }
                    else
                    {
                        footCell.SetCellValue("");
                    }
                }
            }
            return iWorkbook;
        }

        /// <summary>
        /// 读取Excel中的数据，将Excel中的数据按照一定的格式导入到数据库中(持续封装)
        /// 如何使用的描述：
        ///     (1):首先获取到上传到Excel文件:httpPostedFileBase httpPostedFileBase = Request.Files[0];
        ///     (2):规定的Excel的格式：var list = new List<string> {"Id", "Name", "Password", "Sex"};
        ///     (3):调用此方法如下：ExcelHelper.LeadSqlToExcelInfo<T>(httpPostedFileBase,list);
        ///     (4):调用完成之后进行循环操作读取到的数据
        /// </summary>
        /// <typeparam name="T">需要返回的实体对象</typeparam>
        /// <param name="httpPostedFileBase">上传的文件流信息</param>
        /// <param name="list">需要转换的格式</param>
        /// <returns>返回从Excel中读取到的集合信息返回</returns>
        public static IList<T> LeadSqlToExcelInfo<T>(HttpPostedFileBase httpPostedFileBase, List<string> list, int page) where T : class, new()
        {
            //获取该文件的扩展名
            string extenstion = Path.GetExtension(httpPostedFileBase.FileName);

            //如果文件的扩展名存在才能进行下面的步骤
            if (extenstion != null)
            {
                string extenstionName = extenstion.ToLower();
                if (extenstionName == ".xls" || extenstionName == ".xlsx")
                {
                    //执行进行读取Excel文件内容的实现
                    ExcelBaseHelper excelHelper = new ExcelBaseHelper(httpPostedFileBase.InputStream);
                    return excelHelper.ExcelToList<T>(list, page);
                }
            }
            return null;
        }
    }
}
