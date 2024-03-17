using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public class EconomyController:Script
    {
        private decimal playerMoney;
        private decimal playerBank;
        private bool playerEconomyDataReady = false;
        private bool QbCoreCanPay = false;

        public EconomyController(Main main):base(main) 
        {
            
        }
        internal async Task<bool> CanPay(decimal price)
        {
            
            if (Constant.Framework == "Core")
            {
                Logger.Warn($"Economy.GetEconomyData");
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
            else if (Constant.Framework == "QBCore")
            {
                playerEconomyDataReady = false;
                TriggerServerEvent("MyAiMech:server:IsCanPay", price);
                while (!playerEconomyDataReady) await Delay(500);
                return QbCoreCanPay;


            }
            return false;
        }

        internal async void Pay(decimal price)
        {
            if (Constant.Framework == "Core")
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
            }
            else if (Constant.Framework == "QBCore")
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


        #region Events
        #region Core
        [EventHandler("Economy.OnGetEconomyData")]
        public void OnGetEconomyData(decimal money, decimal bank)
        {
            Logger.Warn($"OnGetEconomyData money {money} bank {bank}");
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
        #endregion
    }
}
