using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Script:BaseScript
    {
        public Main Main { get; }
        public string Name { get; }

        public Script(Main main, string configFile = "", bool needRestart = false)
        {
            Main = main;
            Name = GetType().Name;

            if (!string.IsNullOrEmpty(configFile))
            {
                //Config = Configuration.ParseToDynamic("config/" + configFile);
            }
            //NeedRestart = needRestart;
        }

        

        [EventHandler("onResourceStop")]
        private void OnResourceStop(string resourceName)
        {
            if (resourceName == Constant.ResourceName)
            {
                GC.Collect();
                GC.SuppressFinalize(this);
            }
        }
    }
}
