﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Altman.Common.AltData;
using Altman.Controls;
using Altman.Model;
using Eto.Drawing;
using Eto.Forms;
using PluginFramework;

namespace Plugin_ShellCmder
{
    public class ShellCmderControl : Panel
    {
        private IHost _host;
        private ShellCmderService _shellCmder;
        private Shell _shellData;
        private InternalCommand _internalCommand;

        private bool _isWin;
        private string _currentDir;

        private CmdBox shellTextBox_Cmder;
        public ShellCmderControl(IHost host, Shell data)
        {
            Init();
            this._host = host;
            this._shellData = data;

            shellTextBox_Cmder.CommandEntered+=shellTextBox_Cmder_CommandEntered;
            shellTextBox_Cmder.Prompt = "SECTools";
            ConnectOneShell();
        }

        void Init()
        {
            shellTextBox_Cmder = new CmdBox()
            {
                IsWin = true,
                Prompt = ">>> >>> >>> > > > > > > ",
                ShellTextBackColor = Colors.Black,
                ShellTextForeColor = Color.FromArgb(224, 224, 224)
            };

            var layout = new DynamicLayout { Padding = new Padding(0) };
            layout.Add(shellTextBox_Cmder);

            this.Content = layout;
        }

        /// <summary>
        /// 回车执行事件
        /// </summary>
        private void shellTextBox_Cmder_CommandEntered(object sender, CmdBox.CommandEnteredEventArgs e)
        {
            string command = e.Command;
            Thread thread = new Thread(() => this.ExecuteCmd(command));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 连接一句话
        /// </summary>
        private void ConnectOneShell()
        {
            try
            {
                //初始化ShellCmder
                _shellCmder = new ShellCmderService(_host,_shellData);
                //初始化内部命令
                _internalCommand = new InternalCommand(shellTextBox_Cmder, _shellCmder);
                //获取系统信息
                OsInfo info = _shellCmder.GetSysInfo();
                string str = string.Format("操作系统平台：{0}    当前用户：{1}", info.Platform, info.CurrentUser);
                //设置系统平台
                if (info.DirSeparators == @"\")
                {
                    _isWin = true;
                }
                else
                {
                    _isWin = false;
                }
                //设置当前目录
                _currentDir = info.ShellDir;
                //cmder的系统平台
                shellTextBox_Cmder.IsWin = _isWin;
                //设置提示信息
                shellTextBox_Cmder.Prompt = _currentDir;
                shellTextBox_Cmder.PrintCommandResult(str);        
            }
            catch(Exception ex)
            {
                shellTextBox_Cmder.PrintCommandResult(ex.Message);
            }       
        }
        /// <summary>
        /// 执行cmd
        /// </summary>
        private void ExecuteCmd(string command)
        {
            if (!_internalCommand.ProcessInternalCommand(command))
            {
                try
                {
                    if (_shellCmder != null)
                    {
                        CmdResult cmdResult = _shellCmder.ExecuteCmd("", command, _currentDir, _isWin);
                        //设置当前目录
                        _currentDir = cmdResult.CurrentDir;
                        //设置提示信息
                        shellTextBox_Cmder.Prompt = _currentDir;
                        shellTextBox_Cmder.PrintCommandResult(cmdResult.Result);
                    }
                }
                catch (Exception ex)
                {
                    shellTextBox_Cmder.PrintCommandResult("[Error]"+ex.Message);
                }
            }
        }
    }
}