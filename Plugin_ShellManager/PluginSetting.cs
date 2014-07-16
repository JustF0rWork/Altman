﻿using Altman.Plugins;

namespace Plugin_ShellManager
{
    public class PluginSetting : IPluginSetting
    {
        public bool IsAutoLoad
        {
            get { return true; }
        }
        public bool IsNeedShellData
        {
            get { return false; }
        }

        public bool IsShowInRightContext
        {
            get { return false; }
        }
    }
}
