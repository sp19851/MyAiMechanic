using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    static class Utils
    {
        #region CreateBlock
        public async static Task<Ped> CreatePed(Model model, Vector3 coords, float heading = 0f)
        {
            return await World.CreatePed(model, coords, heading);
        }
        public async static Task<Vehicle> CreateVehicle(Model model, Vector3 coords, float heading, Ped driver = null)
        {
            var vehicle = await World.CreateVehicle(model, coords, heading);

            if (driver != null)
            {
                driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }
            var rnd = new Random();
            var plate = $"AIMECH{rnd.Next(10,99)}";
            API.SetVehicleNumberPlateText(vehicle.Handle, plate);
            return vehicle;

        }

        
        
        #endregion
    }
}
