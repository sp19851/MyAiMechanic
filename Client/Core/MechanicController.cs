
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
        private Ped mechanic;
        private Vehicle mechanicVehicle;
        private Blip mechanicVehicleBlip;
        private Vehicle targetVehicle;
        private Vector3 targetPos;
        private decimal playerMoney;
        private decimal playerBank;
        private bool playerEconomyDataReady = false;
        private bool QbCoreCanPay = false;

        private List<Model> mechanicModels = new List<Model> {new Model("MP_M_WareMech_01"), new Model("IG_Mechanic_02"), new Model("MP_F_BennyMech_01"),
                                                                new Model("U_M_Y_SmugMech_01"),  new Model("S_M_Y_XMech_02_MP")};
            

        public MechanicController(Main main):base(main)
        {
            
        }
        public bool IsMechanicISRuning() => mechanicVehicle == null ? false : true;

        private async Task UpdateDrivingToTarget()
        {

            if (mechanic == null || mechanicVehicle == null) 
            {
                //Logger.Error($" UpdateDrivingToTarget null {mechanic == null} {mechanicVehicle == null}");
                return;
            }
            
            API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)mechanicVehicle.GetHashCode(), 283, 1f, 1);
            var dst = Vector3.Distance(mechanicVehicle.Position, (Vector3)targetPos);
            //Logger.Warn($" UpdateDrivingToTarget dst {dst}");
            if (dst > 70)
            {

                API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 20f, 0, (uint)mechanicVehicle.GetHashCode(), 283, 1f, 1);

            }
            if ( dst > 10 && dst <= 70f)
            {

                API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 5f, 0, (uint)mechanicVehicle.GetHashCode(), 287, 1f, 1);

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
            //var engine = API.GetWorldPositionOfEntityBone(targetVehicle.Handle, API.GetEntityBoneIndexByName(targetVehicle.Handle, "bonnet"));
            //var coord = engine + targetVehicle.ForwardVector * 2;
            var coord = targetVehicle.Position + targetVehicle.ForwardVector * 2;
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
                
                World.DrawMarker(MarkerType.CarSymbol, coord, new Vector3(0,0,0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(255,255,0));
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
            Screen.ShowNotification("Готово! Думаю, что до сервиса дотянешь");
            Pay();
        }

        private async Task<bool> CanPay(decimal amount)
        {
            var price = decimal.Parse(Constant.Config["PriceSimple"].ToString());
            if (Constant.Config["Framework"].ToString() == "Core")
            {
                playerEconomyDataReady = false;
                TriggerEvent("Economy.GetEconomyData");
                while (!playerEconomyDataReady) await Delay(500);
                if (playerBank >= price)
                {
                    return true;
                }
                if (playerMoney >= price)
                {
                    return true;
                }
                TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "У Вас нет средств для оплаты", "crimson", "Warning");
                return false;
            }
            else if (Constant.Config["Framework"].ToString() == "QBCore")
            {
                playerEconomyDataReady = false;
                TriggerServerEvent("MyAiMech:server:IsCanPay", price);
                while (!playerEconomyDataReady) await Delay(500);
                return QbCoreCanPay;
                

            }
            return false;
        }

        private async void Pay()
        {
            if (Constant.Config["Framework"] == null) 
            {
                Logger.Error("Оплата не возможна, так как в config.json не указан фреймворк");
                return;
            }
            var price = decimal.Parse(Constant.Config["PriceSimple"].ToString());
            if (Constant.Config["Framework"].ToString() == "Core")
            {
                playerEconomyDataReady = false;
                TriggerEvent("Economy.GetEconomyData");
                while (!playerEconomyDataReady) await Delay(500);
               
                if (await CanPay(price))
                {
                    if (playerBank >= price)
                    {
                        TriggerEvent("Economy.RemoveBank", price);
                        return;
                    }
                    if (playerMoney >= price)
                    {
                        TriggerEvent("Economy.RemoveCash", price);
                        return;
                    }
                }
                return;
            } else if (Constant.Config["Framework"].ToString() == "QBCore") 
            {
                playerEconomyDataReady = false;
                TriggerServerEvent("MyAiMech:server:IsCanPay", price);
                while (!playerEconomyDataReady) await Delay(500);
                if (playerBank >= price)
                {
                    TriggerServerEvent("MyAiMech:server:RemoveBank", price);
                    return;
                }
                if (playerMoney >= price)
                {
                    TriggerServerEvent("MyAiMech:server:RemoveCash", price);
                    return;
                }
            }
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
            if (Constant.Config["Framework"] == null)
            {
                Logger.Error("Ты не можешь вызвать механика, потому, что не сможешь оплатить услугу, так как в config.json не указан фреймворк");
                return;
            }
           
                
            var price = decimal.Parse(Constant.Config["PriceSimple"].ToString());

            if (await CanPay(price) == false)
            {
                //TriggerEvent("Notification.AddAdvanceNotif", "МЕХАНИК", "", 3500, "У Вас нет средств для оплаты", "crimson", "Warning");
                return;
            }
            if (Game.PlayerPed.CurrentVehicle == null) 
            {
                Screen.ShowNotification("Нужно быть в машине");
                await Delay(3000);
                return;
            }
            if (Game.PlayerPed.SeatIndex != VehicleSeat.Driver)
            {
                Screen.ShowNotification("Нужно быть за рулем");
                await Delay(3000);
                return;
            }
            targetVehicle = Game.PlayerPed.CurrentVehicle;
            Model modelVehiicle = new Model("FLATBED");
            var rnd = new Random();
            var modelPed = mechanicModels[rnd.Next(0, mechanicModels.Count())];
            var newPos = targetVehicle.Position - Game.PlayerPed.ForwardVector * 100;
            var node = World.GetNextPositionOnStreet((Vector2)newPos, true);



            
            mechanic = await Utils.CreateMechanicPed(modelPed, node);
            while (mechanic == null) await Delay(500);
            mechanicVehicle = await Utils.CreateMechanicVehicle(modelVehiicle, node, targetVehicle.Heading, mechanic);
            if (mechanicVehicle != null )
            {
                mechanicVehicle.PlaceOnGround();
                mechanicVehicleBlip = mechanicVehicle.AttachBlip();
                targetPos = (Vector3)targetpos;
                Tick += UpdateDrivingToTarget;
                Screen.ShowNotification("Диспетчер. Механик выехал к Вам. Ожидайте!");   
                mechanicVehicle.IsSirenActive = true;
                mechanicVehicle.IsSirenSilent = true;
            }
        }


        //[EventHandler(Events.CreateCar)]
        #region Events
        #region Core
        [EventHandler("Economy.OnGetEconomyData")]
        public void OnGetEconomyData(decimal money, decimal bank)
        {
            playerMoney = money;
            playerBank = bank;
            playerEconomyDataReady = true;
        }
        #endregion
        #region QBCore
        [EventHandler("MyAiMech:client:IsCanPay")]
        public void IsCanPay(bool canpay, decimal bank, decimal money)
        {
            QbCoreCanPay = canpay;
            playerMoney = money;
            playerBank = bank;
            playerEconomyDataReady = true;
        }
        #endregion

        [EventHandler("onResourceStop")]
        public void OnResourceStop()
        {
            LeaveTarget();
        }


        #endregion
    }
}
