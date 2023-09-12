﻿/*
 *      [LKY Common Tools] Copyright (C) 2022 liukaiyuan@sjtu.edu.cn Inc.
 *      
 *      FileName : Lib_OfficeProcess.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_OfficeTools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static LKY_OfficeTools.Common.Com_ExeOS.KillExe;
using static LKY_OfficeTools.Lib.Lib_AppInfo;
using static LKY_OfficeTools.Lib.Lib_AppLog;

namespace LKY_OfficeTools.Lib
{
    internal class Lib_OfficeProcess
    {
        /// <summary>
        /// Office 所有版本常用进程列表
        /// </summary>
        internal static List<string> Process_List
        {
            get
            {
                try
                {
                    Stream office_processes_res = Assembly.GetExecutingAssembly().
                    GetManifestResourceStream(AppDevelop.NameSpace_Top /* 当命名空间发生改变时，此值也需要调整 */
                    + ".Resource.Office_Processes.list");
                    StreamReader office_processes_sr = new StreamReader(office_processes_res);
                    string office_processes = office_processes_sr.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(office_processes))
                    {
                        List<string> office_processes_list = new List<string>();
                        string[] p_info = office_processes.Replace("\r", "").Split('\n');      //分割出进程数组
                        if (p_info != null && p_info.Length > 0)
                        {
                            foreach (var now_process in p_info)
                            {
                                office_processes_list.Add(now_process);
                            }

                            return office_processes_list;
                        }
                    }
                    return null;
                }
                catch (Exception Ex)
                {
                    new Log(Ex.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// 从 Office List 文件中找到目前计算机正在运行的进程名
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetRuningProcess()
        {
            try
            {
                List<string> runing_office_process = new List<string>();
                foreach (var now_p in Process_List)
                {
                    if (Com_ExeOS.Info.IsRun(now_p))
                    {
                        runing_office_process.Add(now_p);
                    }
                }

                return runing_office_process;
            }
            catch (Exception Ex)
            {
                new Log(Ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 结束 Office 进程类库
        /// </summary>
        internal class KillOffice
        {
            /// <summary>
            /// 结束所有 Office 常用进程。
            /// </summary>
            /// <returns></returns>
            internal static bool All()
            {
                try
                {
                    //轮询结束每个进程（等待其结束）
                    foreach (var now_p in Process_List)
                    {
                        //new Log($"     >> 等待 {now_p.ToLower()} 进程关闭中 ...", ConsoleColor.DarkYellow);
                        Com_ExeOS.KillExe.ByExeName(now_p, KillMode.Try_Friendly, true);
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
