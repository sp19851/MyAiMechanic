using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public class Commands:Script
    {
        private MechanicController mechanicController;
        private TaxiController taxiController;

        public Commands(Main main):base(main)
        {
            mechanicController = Main.GetScript<MechanicController>();
            taxiController = Main.GetScript<TaxiController>();
        }
        [Command("aimech")]
        private void StartMechanic()
        {
            Ped playerPed = Game.PlayerPed;
            if (playerPed.CurrentVehicle != null)
            {
                if (!mechanicController.IsRuning())
                {
                    mechanicController.CreateCar(playerPed.CurrentVehicle.Position);
                }
                else
                {
                    mechanicController.LeaveTarget();
                }
                
            }
            
        }
        [Command("aitaxi")]
        private void StartTaxi()
        {
            Ped playerPed = Game.PlayerPed;
            if (!taxiController.IsRuning())
            {
                taxiController.CreateCar(playerPed.Position);
            }
            else
            {
                taxiController.Reset();
            }
        }
    }
}
