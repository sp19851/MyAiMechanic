using CitizenFX.Core;

using Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Main : BaseScript
    {
        private List<Script> scripts = new List<Script>();
        public static EventHandlerDictionary Handlers { get; private set; }
        public Main()
        {
            Handlers = EventHandlers;

            Constant.Config = Configuration.Parse("config");
            Init();
        }

        private void Init()
        {
            LoadScript(new MechanicController(this));
            LoadScript(new Commands(this));
        }
        #region DO NOT TOUCH THIS

        public bool ScriptIsStarted<T>() => GetScript<T>() != null;
        public bool ScriptIsStarted(string scriptName) => GetScript(scriptName) != null;
        public T GetScript<T>() => (T)System.Convert.ChangeType(scripts.Find(x => x.GetType() == typeof(T)), typeof(T));
        public Script GetScript(string scriptName) => scripts.Find(x => x.Name == scriptName);
        public List<Script> GetScripts() => scripts;

        public void LoadScript(Script script)
        {
            if (!scripts.Exists(x => x.Name == script.Name))
            {
                Logger.Info($"Скрипт загружен | {script.Name}");
                scripts.Add(script);
                RegisterScript(script);
            }
            else
            {
                Logger.Error($"Ошибка загрузки скрипта | {script.Name}");
            }
        }
        public void UnloadScript(Script script)
        {
            if (scripts.Exists(x => x.Name == script.Name))
            {
                Logger.Info($"Скрипт загружен | {script.Name}" );

                UnregisterScript(script);
                scripts.Remove(script);
            }
            else
            {
                Logger.Info($"Ошибка выгрузки скрипта | {script.Name}");
            }
        }

        #endregion
    }
}
