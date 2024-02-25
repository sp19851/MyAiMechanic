using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Configuration
    {
        public static JObject Parse(string filePath)
        {

            try
            {
                //Logger.Warn($"file {API.GetCurrentResourceName()} {filePath}");
                var json = API.LoadResourceFile(API.GetCurrentResourceName(), filePath + ".json");
                return JObject.Parse(json);
            }
            catch
            {
                // Log.WriteLog((string)Lang.Current["Client.Config.FileNotFound"]);
                throw new FileNotFoundException();
            }
        }

        public static dynamic ParseToDynamic(string filePath)
        {
            try
            {
                //Debug.WriteLine($"filePath {filePath} {API.GetCurrentResourceName()}");
                var json = API.LoadResourceFile(API.GetCurrentResourceName(), filePath + ".json");
                return JsonConvert.DeserializeObject(json);
            }
            catch
            {
                // Log.WriteLog((string)Lang.Current["Client.Config.FileNotFound"]);
                throw new FileNotFoundException();
            }
        }

        public static Dictionary<string, string> ParseToDictionary(string filePath)
        {
            try
            {
                var json = API.LoadResourceFile(API.GetCurrentResourceName(), filePath + ".json");
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch
            {
                // Log.WriteLog((string)Lang.Current["Client.Config.FileNotFound"]);
                throw new FileNotFoundException();
            }
        }

        public static JArray ParseToArray(string filePath)
        {
            try
            {
                var json = API.LoadResourceFile(API.GetCurrentResourceName(), filePath + ".json");
                return JArray.Parse(json);
            }
            catch
            {
                // Log.WriteLog((string)Lang.Current["Client.Config.FileNotFound"]);
                throw new FileNotFoundException();
            }
        }
    }

    public static class Configuration<T>
    {
        public static T Parse(string filePath)
        {

            // Logger.WriteLog((string)Lang.Current["Client.Config.FileLoaded"], filePath);


            var json = API.LoadResourceFile(API.GetCurrentResourceName(), filePath + ".json");

            //Logger.Error($"Configuration {json} ");


            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
