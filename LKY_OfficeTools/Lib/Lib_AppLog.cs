﻿/*
 *      [LKY Common Tools] Copyright (C) 2022 liukaiyuan@sjtu.edu.cn Inc.
 *      
 *      FileName : Lib_AppLog.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_OfficeTools.Common;
using System;
using System.IO;
using static LKY_OfficeTools.Lib.Lib_AppInfo.AppPath;
using static LKY_OfficeTools.Lib.Lib_OfficeInfo.OfficeLocalInfo;

namespace LKY_OfficeTools.Lib
{
    /// <summary>
    /// 日志类库
    /// </summary>
    internal class Lib_AppLog
    {
        /// <summary>
        /// 产生记录类库
        /// </summary>
        internal class Log
        {
            /// <summary>
            /// 日志文件保存的位置
            /// </summary>
            internal static string log_filepath = null;

            /// <summary>
            /// 安装失败时注册表 导出后的文件路径
            /// </summary>
            internal static string reg_install_error { get; set; }

            /// <summary>
            /// 所有日志的文字记录
            /// </summary>
            internal static string log_info { get; set; }

            /*
            /// <summary>
            /// 运行错误的截屏文件列表
            /// </summary>
            internal static List<string> error_screen_path = new List<string>();
            */

            /// <summary>
            /// 日志输出的类型
            /// </summary>
            internal enum Output_Type
            {
                /// <summary>
                /// 仅展示文本，不写入log
                /// </summary>
                Display,

                /// <summary>
                /// 仅写入log文件，不展现文字
                /// </summary>
                Write,

                /// <summary>
                /// 既展示文字，又写入log
                /// </summary>
                Display_Write
            }

            /// <summary>
            /// 重载：输出实现日志
            /// </summary>
            internal Log(string str, ConsoleColor str_color, Output_Type output_type = Output_Type.Display_Write)
            {
                try
                {
                    //判断是否需要显示文字
                    if (output_type == Output_Type.Display || output_type == Output_Type.Display_Write)
                    {
                        Console.ForegroundColor = str_color;
                        Console.WriteLine(str);

                        //输出后恢复颜色
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    //需要输出日志文件时，进行判断
                    if (output_type == Output_Type.Write || output_type == Output_Type.Display_Write)
                    {
                        string datatime_format = DateTime.Now.ToString("s").Replace("T", "_").Replace(":", "-");


                        //服务模式，写出到日志文件
                        if (Lib_AppState.Current_RunMode == Lib_AppState.RunMode.Service)
                        {
                            //为空时，创建日志路径
                            if (string.IsNullOrEmpty(log_filepath))
                            {
                                string file_name = "service_" + datatime_format + ".log";
                                log_filepath = $"{Documents.Logs}\\{file_name}";
                            }

                            //目录不存在时创建目录
                            Directory.CreateDirectory(new FileInfo(log_filepath).DirectoryName);

                            //文件不存在时创建&写入
                            File.AppendAllText(log_filepath, $"{datatime_format}, {str}\n");
                        }


                        //将日志记录在内存中
                        string now_log = $"{datatime_format}, {str.Replace("\n", "")}";
                        if (str.Contains("×"))
                        {
                            now_log = $"<font color=red><b>{now_log}</b></font>";          //有错误标红、加粗
                        }
                        else if (str.Contains("Exception"))
                        {
                            now_log = $"<font color=\"#ff4c00\">{now_log}</font>";       //抛出异常用橙红色
                        }

                        log_info += now_log + "<br />";

                        /*
                        //出现错误时，增加附加信息
                        if (str.Contains("×"))
                        {
                            string err_filename = datatime_format + ".png";
                            err_filename = $"{Lib_AppInfo.Path.Dir_Log}\\{err_filename}";
                            if (Com_SystemOS.Screen.CaptureToSave(err_filename))
                            {
                                error_screen_path.Add(err_filename);
                            }
                        }
                        */
                    }
                }
                catch
                {
                    return;
                }
            }

            /// <summary>
            /// 将日志信息保存到文件，不在UI中显示日志内容。
            /// 一般用于记录Bug或者记录错误。
            /// </summary>
            /// <param name="err_str"></param>
            internal Log(string err_str)
            {
                try
                {
                    //整合格式
                    string msg = $"---------- [Error Log: BEGIN] ----------\n{err_str}\n---------- [END] ----------";

                    //非服务模式，替换换行符
                    if (Lib_AppState.Current_RunMode != Lib_AppState.RunMode.Service)
                    {
                        msg = msg.Replace("\n", "<br />");
                    }

                    //输出日志
                    new Log(msg, ConsoleColor.Gray, Output_Type.Write);
                }
                catch
                {
                    return;
                }
            }

            /// <summary>
            /// 因为安装 Office 错误产生的注册表日志
            /// </summary>
            internal Log(InstallState install_error)
            {
                try
                {
                    //安装出错后，会记录系统目前office注册表情况
                    //导出注册表 Office 信息
                    string office_reg_path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office";

                    //依据不同的安装错误生成不同的注册表文件名
                    string reg_filename = "error";

                    //包含未安装标记
                    if (install_error == InstallState.None)
                    {
                        reg_filename += "_none";
                    }

                    //包含不同版本标记
                    if (install_error == InstallState.Diff)
                    {
                        reg_filename += "_diff";
                    }

                    //包含多版本标记
                    if (install_error == InstallState.Multi)
                    {
                        reg_filename += "_multi";
                    }

                    //包含安装正确标记
                    if (install_error == InstallState.Correct)
                    {
                        reg_filename += "_correct";
                    }

                    //合成最终注册表路径
                    reg_install_error = Documents.Logs + $@"\{reg_filename}.reg";

                    //生成注册表信息
                    Com_SystemOS.Register.ExportReg(office_reg_path, reg_install_error);

                    return;
                }
                catch (Exception Ex)
                {
                    new Log(Ex.ToString());
                    return;
                }
            }

            /// <summary>
            /// 清理所有日志及错误文件
            /// </summary>
            /// <returns></returns>
            internal static bool Clean()
            {
                try
                {
                    /*
                    //清理日志
                    if (log_filepath != null)
                    {
                        try
                        {
                            File.Delete(log_filepath);
                        }
                        catch (Exception Ex)
                        {
                            new Log(Ex.ToString());
                        }
                    }
                    */

                    /*
                    //清理错误截屏
                    if (error_screen_path != null)
                    {
                        foreach (var now_file in error_screen_path)
                        {
                            try
                            {
                                File.Delete(now_file);
                            }
                            catch (Exception Ex)
                            {
                                new Log(Ex.ToString());
                            }
                        }
                    }
                    */

                    //清理整个Log文件夹
                    if (Directory.Exists(Documents.Logs))
                    {
                        try
                        {
                            Directory.Delete(Documents.Logs, true);
                        }
                        catch (Exception Ex)
                        {
                            new Log(Ex.ToString());
                        }
                    }

                    return true;
                }
                catch (Exception Ex)
                {
                    new Log(Ex.ToString());
                    return false;
                }
            }
        }
    }
}
