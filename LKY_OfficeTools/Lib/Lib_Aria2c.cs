﻿/*
 *      [LKY Common Tools] Copyright (C) 2022 liukaiyuan@sjtu.edu.cn Inc.
 *      
 *      FileName : Lib_Aria2c.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_OfficeTools.Common;
using System;
using System.IO;
using static LKY_OfficeTools.Lib.Lib_AppInfo.AppPath;
using static LKY_OfficeTools.Lib.Lib_AppLog;

namespace LKY_OfficeTools.Lib
{
    /// <summary>
    /// Aria2c 类库
    /// </summary>
    internal class Lib_Aria2c
    {
        /// <summary>
        /// 使用 Aria2c 下载单个文件。
        /// 返回值：1~下载完成；0~因lot_aria2c.exe丢失导致无法下载；-1~因为非已知的原因下载失败。
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="save_to"></param>
        /// <returns></returns>
        internal static int DownFile(string uri, string save_to)
        {
            try
            {
                //指定路径
                string aria2c_path = Documents.SDKs.SDKs_Root + @"\Aria2c\lot_aria2c.exe";

                if (!File.Exists(aria2c_path))
                {
                    new Log($"     × {aria2c_path} 文件丢失！", ConsoleColor.DarkRed);
                    return 0;
                }

                string file_path = new FileInfo(save_to).DirectoryName;     //保存的文件路径，不含文件名
                string filename = new FileInfo(save_to).Name;               //保存的文件名

                //设置命令行
                string aria2c_params = $"{uri} --dir=\"{file_path}\" --out=\"{filename}\"" +
                    $" --continue=true --max-connection-per-server=5 --check-integrity=true --file-allocation=none --console-log-level=error";
                //new Log(aria2c_params);

                var down_result = Com_ExeOS.Run.Exe(aria2c_path, aria2c_params);
                if (down_result == -920921)
                {
                    throw new Exception();
                }

                return 1;
            }
            catch (Exception Ex)
            {
                new Log(Ex.ToString());
                return -1;
            }
        }
    }
}
