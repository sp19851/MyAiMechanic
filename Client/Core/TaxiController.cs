using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
            API.DeleteWaypoint();
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
            var plate = $"AITAXI{rnd.Next(10, 99)}";
            taxiVehicle = await Utils.CreateVehicle(modelVehicle, node, playerPed.Heading, plate, taxi);
            if (taxiVehicle != null)
            {
                taxiVehicle.PlaceOnGround();
                taxiVehicleBlip = taxiVehicle.AttachBlip();
                taxiVehicleBlip.Sprite = BlipSprite.Cab;
                taxiVehicleBlip.Color = BlipColor.Yellow;
                API.SetEntityLights(taxiVehicle.Handle, true);
                Tick += UpdateDrivingToTarget;
                if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "ТАКСИ", "", 3500, "Диспетчер. Такси выехало к Вам. Ожидайте!", "blue", "Info");
                else Screen.ShowNotification("Диспетчер. Такси выехало к Вам. Ожидайте!");
                taxiVehicle.IsSirenActive = true;
                taxiVehicle.IsSirenSilent = true;
                taxiVehicle.LockStatus = VehicleLockStatus.Unlocked;
                API.SetPedKeepTask(taxi.Handle, true);

                API.SetAmbientVoiceName(taxi.Handle, "A_M_M_EASTSA_02_LATINO_FULL_01");
                API.SetBlockingOfNonTemporaryEvents(taxi.Handle, true);
            }
        }

        private async Task UpdateDrivingToTarget()
        {
            if (taxi == null || taxiVehicle == null)
            {
                return;
            }
            var targetPos = Game.PlayerPed.Position;    
            API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 5f, 1);
            var dst = Vector3.Distance(taxiVehicle.Position, (Vector3)targetPos);

            if (dst > 70 && dst <= 100f)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if (dst > 35 && dst <= 70f)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if (dst > 15 && dst <= 35)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 10f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.IgnoreRoads, 1f, 1);

            }
            if (dst > 7 && dst <= 15f)
            {

                API.TaskVehicleDriveToCoord(taxi.Handle, taxiVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 5f, 0, (uint)taxiVehicle.GetHashCode(), (int)DrivingStyle.IgnoreRoads, 1f, 1);
              
                Logger.Warn(dst);
            }
            if (dst <= 7f)
            {
                Logger.Warn(dst);
                Screen.ShowSubtitle("Машина подана", 3000);
                Tick += UpdateSeating;
                Tick -= UpdateDrivingToTarget;
              
            }
            await Delay(500);
        }

        private async Task UpdateSeating()
        {
            if (Game.IsControlJustPressed(0, Control.Enter))
            {
                //Game.PlayerPed.Task.EnterVehicle(taxiVehicle, VehicleSeat.RightRear, -1, 0f, 1);
                API.TaskEnterVehicle(Game.PlayerPed.Handle, taxiVehicle.Handle, -1, 2, 1f, 1, 0);
                Logger.Warn($"EnterVehicle");
                taxiVehicle.IsTaxiLightOn = false;

               
                
                
                await Delay(1000);

            }
            if (!API.DoesBlipExist(API.GetFirstBlipInfoId(8)))
            {
                Screen.ShowSubtitle("Для начала поездки отметьте точку на карте, нажмите - E", 2000);


            }
            else
            {
                Tick -= UpdateSeating;
                Tick += UpdateDrivingToGps;
            }
        }

        private async Task UpdateDrivingToGps()
        {
            var waypointCoords = World.GetWaypointBlip();
            float z;
            float dst;
            if (waypointCoords == null)
            {
                dst = 0;
            }
            else
            {
                z = World.GetGroundHeight(waypointCoords.Position);
                dst = Vector3.Distance(waypointCoords.Position, Game.PlayerPed.Position);
            }

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                
                API.PlayAmbientSpeech1(taxi.Handle, "TAXID_BEGIN_JOURNEY", "SPEECH_PARAMS_FORCE_NORMAL");
                if (dst > 8000f)
                {
                    if (Constant.Framework == "Core")
                    {
                        TriggerEvent("Notification.AddAdvanceNotif", "ТАКСИ", "", 3500, "Это слишком за пределом зоны моего таксопарка. Попробуй разбить на две поездки!", "orange", "Info");
                    }
                    else
                    {
                        Screen.ShowNotification("Это слишком за пределом зоны моего таксопарка. Попробуй разбить на две поездки!");
                    }
                }
                else
                {
                    taxi.Task.DriveTo(taxiVehicle, waypointCoords.Position, 7f, 30f, (int)DrivingStyle.Normal);
                }
            }
            if (Game.IsControlJustPressed(0, Control.Jump))
            {

                API.PlayAmbientSpeech1(taxi.Handle, "TAXID_SPEED_UP", "SPEECH_PARAMS_FORCE_NORMAL");
                taxi.Task.DriveTo(taxiVehicle, waypointCoords.Position, 7f, 60f, (int)DrivingStyle.AvoidTraffic);
                
            }
            if (Game.IsControlJustPressed(0, Control.FrontendRright))
            {
                taxiVehicle.Speed = 0f;
                API.ClearPedTasks(taxi.Handle);
                API.PlayAmbientSpeech1(taxi.Handle, "TAXID_CLOSE_AS_POSS", "SPEECH_PARAMS_FORCE_NORMAL");
                API.TaskVehicleTempAction(taxi.Handle, taxiVehicle.Handle, 6, 2000);
                API.SetVehicleHandbrake(taxiVehicle.Handle, true);
                API.SetVehicleEngineOn(taxiVehicle.Handle, false, true, false);
                API.SetPedKeepTask(taxiVehicle.Handle, true);
                
                if (Constant.Framework == "Core")
                {
                    TriggerEvent("Notification.AddAdvanceNotif", "ТАКСИ", "", 3500, "Заказ прерван", "red", "Info");
                }
                else
                {
                    Screen.ShowNotification("Заказ прерван");
                }
                API.TaskLeaveVehicle(Game.PlayerPed.Handle, taxiVehicle.Handle, 512);
                Tick -= UpdateDrivingToGps;
                await Delay(3000);
                Reset();
            }
            //Logger.Warn($"UpdateDrivingToGps dst {dst} taxiVehicle.Speed {taxiVehicle.Speed}");
            if (dst <= 8f)
            {
                if (taxiVehicle.Speed == 0)
                {
                    API.ClearPedTasks(taxi.Handle);
                    API.PlayAmbientSpeech1(taxi.Handle, "TAXID_CLOSE_AS_POSS", "SPEECH_PARAMS_FORCE_NORMAL");
                    API.TaskVehicleTempAction(taxi.Handle, taxiVehicle.Handle, 6, 2000);
                    API.SetVehicleHandbrake(taxiVehicle.Handle, true);
                    API.SetVehicleEngineOn(taxiVehicle.Handle, false, true, false);
                    API.SetPedKeepTask(taxiVehicle.Handle, true);
                    if (Constant.Framework == "Core")
                    {
                        TriggerEvent("Notification.AddAdvanceNotif", "ТАКСИ", "", 3500, "Мы прибыли на место", "green", "Info");
                    }
                    else
                    {
                        Screen.ShowNotification("Мы прибыли на место");
                    }
                    API.TaskLeaveVehicle(Game.PlayerPed.Handle, taxiVehicle.Handle, 512);
                    Tick -= UpdateDrivingToGps;
                    await Delay(3000);
                    Reset();
                }
            }

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
