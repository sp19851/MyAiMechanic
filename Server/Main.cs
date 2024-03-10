using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Main:BaseScript
    {
        public Main()
        {
            Debug.WriteLine("Скритп AIMech | Запущен");

            //var qbcore = Exports["qb-core"].GetCoreObject();
            //Debug.WriteLine($"qbcore {JsonConvert.SerializeObject(qbcore)}");
        }
    }
}
