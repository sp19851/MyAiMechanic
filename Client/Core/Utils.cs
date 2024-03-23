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
        public async static Task<Vehicle> CreateVehicle(Model model, Vector3 coords, float heading, string plate, Ped driver = null)
        {
            var vehicle = await World.CreateVehicle(model, coords, heading);

            if (driver != null)
            {
                driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }
            
            vehicle.IsRadioEnabled = false;
            vehicle.IsTaxiLightOn = true;
            API.SetVehicleNumberPlateText(vehicle.Handle, plate);
            return vehicle;

        }

        
        
        #endregion
    }
}
