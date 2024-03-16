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
        public Commands(Main main):base(main)
        {
            mechanicController = Main.GetScript<MechanicController>();
        }
        [Command("aimech")]
        private async void Start()
        {
            Ped playerPed = Game.PlayerPed;
            if (playerPed.CurrentVehicle != null)
            {
                if (!mechanicController.IsMechanicISRuning())
                {
                    mechanicController.CreateCar(playerPed.CurrentVehicle.Position);
                }
                else
                {
                    mechanicController.LeaveTarget();
                }
                
            }
            
        }
    }
}
