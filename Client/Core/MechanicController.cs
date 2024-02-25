
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
    public class MechanicController : Script
    {
        private Ped mechanic;
        private Vehicle mechanicVehicle;
        private Blip mechanicVehicleBlip;
        private Vehicle targetVehicle;
        private Vector3 targetPos;

        private List<Model> mechanicModels = new List<Model> {new Model("MP_M_WareMech_01"), new Model("IG_Mechanic_02"), new Model("MP_F_BennyMech_01"),
                                                                new Model("U_M_Y_SmugMech_01"),  new Model("S_M_Y_XMech_02_MP")};
            

        public MechanicController(Main main):base(main)
        {
            
        }
        private async Task UpdateDrivingToTarget()
        {
            Logger.WriteLine(Logger.Gray + Vector3.Distance(mechanicVehicle.Position, Game.PlayerPed.Position));
            //API.TaskVehicleDriveToCoord(mechanic.Handle,mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 10f, 0, (uint)mechanicVehicle.GetHashCode(), 786603, 1f, 1);
            API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 10f, 0, (uint)mechanicVehicle.GetHashCode(), 411, 1f, 1);
            if (Vector3.Distance(mechanicVehicle.Position, (Vector3)targetPos)<= 10f)
            {
                Tick -= UpdateDrivingToTarget;
                API.TaskLeaveVehicle(mechanic.Handle, mechanicVehicle.Handle, 0);
                var engine = API.GetWorldPositionOfEntityBone(targetVehicle.Handle, API.GetEntityBoneIndexByName(targetVehicle.Handle, "engine"));
                //API.TaskGoToCoordAnyMeans(mechanic.Handle, engine.X, engine.Y, engine.Z, 2.0f, 0, false, 1074528293, 0xbf800000);
                //API.TaskVehicleDriveToCoord(mechanic.Handle, mechanicVehicle.Handle, targetPos.X, targetPos.Y, targetPos.Z, 10f, 0, (uint)mechanicVehicle.GetHashCode(), 786603, 1f, 1);
                Tick += UpdateWalkingToTarget;
            }
            await Delay(1000);
        }

        private async Task UpdateWalkingToTarget()
        {
            var engine = API.GetWorldPositionOfEntityBone(targetVehicle.Handle, API.GetEntityBoneIndexByName(targetVehicle.Handle, "engine"));
            if (Vector3.Distance(mechanicVehicle.Position, (Vector3)targetPos) > 1f)
            {
                API.TaskGoToCoordAnyMeans(mechanic.Handle, engine.X, engine.Y, engine.Z, 2.0f, 0, false, 1074528293, 0xbf800000);
              
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
                //RepairVehicle(targetVehicle.Handle, targetVehicle.Handle, (mechanic.Handle);
                LeaveTarget();
            }
        }
        public async void LeaveTarget()
        {
            API.TaskVehicleDriveWander(mechanic.Handle, mechanicVehicle.Handle, 17.0f, 1074528293);

            var veh = mechanicVehicle.Handle;
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
            Model modelVehiicle = new Model("FLATBED");
            var rnd = new Random();
            var modelPed = mechanicModels[rnd.Next(0, mechanicModels.Count())];
            var newPos = Game.PlayerPed.Position + Vector3.Forward * 500;
            var node = World.GetNextPositionOnStreet((Vector2)newPos, true);

            


            mechanic = await Utils.CreateMechanicPed(modelPed, node);
            mechanicVehicle = await Utils.CreateMechanicVehicle(modelVehiicle, node, 0f, mechanic);
            if (mechanicVehicle != null)
            {
                mechanicVehicle.PlaceOnGround();
                mechanicVehicleBlip = mechanicVehicle.AttachBlip();
                targetPos = (Vector3)targetpos;
                Tick += UpdateDrivingToTarget;
                Screen.ShowNotification("Диспетчер. Механик выехал к Вам. Ожидайте!");    
            }
        }

       
        //[EventHandler(Events.CreateCar)]

    }
}
