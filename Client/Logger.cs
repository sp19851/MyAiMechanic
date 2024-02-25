using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Logger
    {
        #region Colors

        public const string Black = "^0";
        public const string DarkRed = "^1";
        public const string Green = "^2";
        public const string Yellow = "^3";
        public const string Blue = "^4";
        public const string Cyan = "^5";
        public const string Pink = "^6";
        public const string White = "^7";
        //public const string Red = "^8";
        public const string Gray = "^9";

        #endregion

        #region Console log

        public static void WriteLine(string value) => Debug.WriteLine(value);
        public static void WriteLog(string value) => Debug.WriteLine(value);
        public static void WriteLog(string value, params string[] args) => Debug.WriteLine(value, args);
        public static void Info(string value) => Debug.WriteLine("^5" + value);
        public static void Warn(object value) => Debug.WriteLine("^3" + value);
        public static void Error(string value) => Debug.WriteLine("^8" + value);

        #endregion
    }
}
