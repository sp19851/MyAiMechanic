using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public class TaxiController:Script
    {
        private EconomyController economyController;
        
        private Ped taxi;
        private Vehicle taxiVehicle;
        private Blip taxiVehicleBlip;
        private List<Model> taxiModels = new List<Model> {new Model("G_M_Y_MexGoon_03"), new Model("CSB_Chin_goon"), new Model("A_M_Y_BeachVesp_01")};

        //private List<Model> taxiVehicleModels = new List<Model> {new Model("taxi")};

        public TaxiController(Main main):base(main)
        {
            economyController = Main.GetScript<EconomyController>();
            
        }
        internal bool IsRuning() => taxiVehicle == null ? false : true;
        internal async void Reset()
        {
            var blip = taxiVehicleBlip.Handle;
            API.RemoveBlip(ref blip);
            API.TaskVehicleDriveWander(taxi.Handle, taxiVehicle.Handle, 17.0f, 1074528293);
            await Delay(10000);
            var veh = taxiVehicle.Handle;
            API.SetEntityAsNoLongerNeeded(ref veh);
            var ped = taxi.Handle;
            API.SetPedAsNoLongerNeeded(ref ped);
            
            taxiVehicle = null;
            taxi = null;
            taxiVehicleBlip = null;
        }
        internal async void CreateCar(Vector3 position)
        {
            if (await economyController.CanPay(Constant.PriceTaxi) == false)
            {
                return;
            }

            var playerPed = Game.PlayerPed;
            
            var rnd = new Random();
            var modelPed = taxiModels[rnd.Next(0, taxiModels.Count())];
            var modelVehicle = "taxi";
            var newPos = playerPed.Position - Game.PlayerPed.ForwardVector * 100;
            var node = World.GetNextPositionOnStreet((Vector2)newPos, true);




            taxi = await Utils.CreatePed(modelPed, node);
            while (taxi == null) await Delay(500);
            taxiVehicle = await Utils.CreateVehicle(modelVehicle, node, playerPed.Heading, taxi);
            if (taxiVehicle != null)
            {
                taxiVehicle.PlaceOnGround();
                taxiVehicleBlip = taxiVehicle.AttachBlip();
                Tick += UpdateDrivingToTarget;
                if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "Диспетчер. Механик выехал к Вам. Ожидайте!", "blue", "Info");
                else Screen.ShowNotification("Диспетчер. Механик выехал к Вам. Ожидайте!");
                taxiVehicle.IsSirenActive = true;
                taxiVehicle.IsSirenSilent = true;
            }
        }

        private async Task UpdateDrivingToTarget()
        {
            if (taxi == null || taxiVehicle == null)
            {
                return;
            }
            var targetPos = Game.PlayerPed.Position;    
            API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);
            var dst = Vector3.Distance(taxiVehicle.Position, (Vector3)targetPos);

            if (dst > 70 && dst <= 100f)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if (dst > 35 && dst <= 70f)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if (dst > 10 && dst <= 35)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 5f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.IgnoreRoads, 1f, 1);

            }
            if (dst <= 10f)
            {
                Tick -= UpdateDrivingToTarget;
                Screen.ShowSubtitle("Машина подана", 5000);
            }
            await Delay(1000);
        }

        #region Events
        [EventHandler("onResourceStop")]
        internal void OnResourceStop()
        {
            Reset();
        }

       


        #endregion
    }
}
