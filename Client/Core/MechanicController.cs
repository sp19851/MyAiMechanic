
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public class MechanicController : Script
    {
        private EconomyController economyController;
        
        private Ped mechanic;
        private Vehicle mechanicVehicle;
        private Blip mechanicVehicleBlip;
        private Vehicle targetVehicle;
        private Vector3 targetPos;
        
        

        private List<Model> mechanicModels = new List<Model> {new Model("MP_M_WareMech_01"), new Model("IG_Mechanic_02"), new Model("MP_F_BennyMech_01"),
                                                                new Model("U_M_Y_SmugMech_01"),  new Model("S_M_Y_XMech_02_MP")};

        private List<Model> mechanicVehicleModels = new List<Model> {new Model("Sadler"), new Model("bobcatXL"), new Model("FLATBED"),
                                                                new Model("towtruck3"),  new Model("towtruck4")};
        


        public MechanicController(Main main):base(main)
        {
            economyController = main.GetScript<EconomyController>();
        }
        public bool IsRuning() => mechanicVehicle == null ? false : true;

        private async Task UpdateDrivingToTarget()
        {

            if (mechanic == null || mechanicVehicle == null) 
            {
                //Logger.Error($" UpdateDrivingToTarget null {mechanic == null} {mechanicVehicle == null}");
                return;
            }
            
            API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)mechanicVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);
            var dst = Vector3.Distance(mechanicVehicle.Position, (Vector3)targetPos);
            //Logger.Warn($" UpdateDrivingToTarget dst {dst}");
            if (dst > 70 && dst <=100f)
            {

                API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)mechanicVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if (dst > 35 && dst <= 70f)
            {

                API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)mechanicVehicle.GetHashCode(), (int)DrivingStyle.Normal, 1f, 1);

            }
            if ( dst > 10 && dst <= 35)
            {

                API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 5f, 0, (uint)mechanicVehicle.GetHashCode(), (int)DrivingStyle.IgnoreRoads, 1f, 1);

            }
            if (dst <= 10f)
            {
                await LeaveVehicle();
            }
            await Delay(1000);
        }

        private async Task LeaveVehicle()
        {
            Tick -= UpdateDrivingToTarget;
            API.TaskLeaveVehicle(mechanic.Handle, mechanicVehicle.Handle, 0);
            await Delay(3000);
            var engine = API.GetWorldPositionOfEntityBone(targetVehicle.Handle, API.GetEntityBoneIndexByName(targetVehicle.Handle, "bonnet"));
            var coord = targetVehicle.Position + targetVehicle.ForwardVector * 3;
            //API.TaskGoToCoordAnyMeans(mechanic.Handle, engine.X, engine.Y, engine.Z, 1.0f, 0, false, 786492, 0xbf800000);
            mechanic.DrivingStyle = DrivingStyle.Normal;
            mechanic.Task.GoTo(coord);
            await Delay(5000);
            //API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 10f, 0, (uint)mechanicVehicle.GetHashCode(), 786603, 1f, 1);
            mechanic.Task.ClearSecondary(); //ClearAll();
            Tick += UpdateWalkingToTarget;
        }

        private async Task UpdateWalkingToTarget()
        {
            if (mechanic == null || mechanicVehicle == null)
            {
                //Logger.Error($" UpdateDrivingToTarget null {mechanic == null} {mechanicVehicle == null}");
                return;
            }
            var engine = API.GetWorldPositionOfEntityBone(targetVehicle.Handle, API.GetEntityBoneIndexByName(targetVehicle.Handle, "bonnet"));
            var coord = engine + targetVehicle.ForwardVector * 2;
            //var coord = targetVehicle.Position + targetVehicle.ForwardVector * 2;
            //Logger.Warn($"UpdateWalkingToTarget {Vector3.Distance(mechanic.Position, coord)}");
            mechanic.DrivingStyle = DrivingStyle.ShortestPath;
            if (Vector3.Distance(mechanic.Position, coord) > 1f)
            {
                
                if (API.GetNavmeshRouteResult(mechanic.Handle) != 3)
                {
                    mechanic.Task.GoTo(coord, true);
                }
                else
                {
                    mechanic.Task.GoTo(coord);
                }
                World.DrawMarker(MarkerType.CarSymbol, coord, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(255, 255, 0));
                World.DrawMarker(MarkerType.HorizontalCircleFat, engine, new Vector3(0,0,0), new Vector3(0, 0, 0), new Vector3(0.2f, 0.2f, 0.2f), System.Drawing.Color.FromArgb(255,0,0));
            }
            else
            {
                Tick -= UpdateWalkingToTarget;
                API.SetVehicleUndriveable(targetVehicle.Handle, true);
                API.TaskTurnPedToFaceCoord(mechanic.Handle, targetPos.X, targetPos.Y, targetPos.Z, -1);
                await Delay(1000);
                API.TaskStartScenarioInPlace(mechanic.Handle, "PROP_HUMAN_BUM_BIN", 0, true);
                API.SetVehicleDoorOpen(targetVehicle.Handle, 4, false, false);
                await Delay(10000);
                API.ClearPedTasks(mechanic.Handle);
                API.SetVehicleDoorShut(targetVehicle.Handle, 4,  false);
                RepairVehicle();
                LeaveTarget();
            }
        }
        public async void RepairVehicle()
        {
            targetVehicle.EngineHealth = 800f;
            if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "Готово! Думаю, что до сервиса дотянешь", "blue", "Info");
            else Screen.ShowNotification("Нужно быть за рулем");

            economyController.Pay(Constant.PriceMechanic);
        }

        

        public async void LeaveTarget()
        {
            API.TaskVehicleDriveWander(mechanic.Handle, mechanicVehicle.Handle, 17.0f, 1074528293);

            var veh = mechanicVehicle.Handle;
            mechanicVehicle.IsSirenActive = false;
            mechanicVehicle.IsSirenSilent = false;
            API.SetEntityAsNoLongerNeeded(ref veh);
            var ped = mechanic.Handle;
            API.SetPedAsNoLongerNeeded(ref ped);
            var blip = mechanicVehicleBlip.Handle;
            API.RemoveBlip(ref blip);
            mechanicVehicle = null;
            mechanic = null;
            targetVehicle = null;
            mechanicVehicleBlip = null;


        }
        public async void CreateCar(Vector3 targetpos)
        {
            if (await economyController.CanPay(Constant.PriceMechanic) == false)
            {
                //TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "У Вас нет средств для оплаты", "crimson", "Warning");
                return;
            }
            if (Game.PlayerPed.CurrentVehicle == null) 
            {
                if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "Нужно быть в машине", "crimson", "Warning");
                else Screen.ShowNotification("Нужно быть в машине");
                await Delay(3000);
                return;
            }
            if (Game.PlayerPed.SeatIndex != VehicleSeat.Driver)
            {
                if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "Нужно быть за рулем", "crimson", "Warning");
                else Screen.ShowNotification("Нужно быть за рулем");
                await Delay(3000);
                return;
            }
            targetVehicle = Game.PlayerPed.CurrentVehicle;
            //Model modelVehicle = new Model("FLATBED");
            var rnd = new Random();
            var modelPed = mechanicModels[rnd.Next(0, mechanicModels.Count())];
            var modelVehicle = mechanicVehicleModels[rnd.Next(0, mechanicVehicleModels.Count())];
            var newPos = targetVehicle.Position - Game.PlayerPed.ForwardVector * 100;
            var node = World.GetNextPositionOnStreet((Vector2)newPos, true);



            
            mechanic = await Utils.CreatePed(modelPed, node);
            while (mechanic == null) await Delay(500);
            var plate = $"AIMECH{rnd.Next(10, 99)}";
            mechanicVehicle = await Utils.CreateVehicle(modelVehicle, node, targetVehicle.Heading, plate, mechanic);
            if (mechanicVehicle != null )
            {
                mechanicVehicle.PlaceOnGround();
                mechanicVehicleBlip = mechanicVehicle.AttachBlip();
                mechanicVehicleBlip.Sprite = BlipSprite.Repair;
                targetPos = (Vector3)targetpos;
                Tick += UpdateDrivingToTarget;
                if (Constant.Framework == "Core") TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "Диспетчер. Механик выехал к Вам. Ожидайте!", "blue", "Info");
                else Screen.ShowNotification("Диспетчер. Механик выехал к Вам. Ожидайте!");
                mechanicVehicle.IsSirenActive = true;
                mechanicVehicle.IsSirenSilent = true;
            }
        }


        //[EventHandler(Events.CreateCar)]
        #region Events
     

        [EventHandler("onResourceStop")]
        public void OnResourceStop()
        {
            LeaveTarget();
        }


        #endregion
    }
}
