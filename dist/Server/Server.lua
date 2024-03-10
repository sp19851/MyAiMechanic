local QBCore

local path = GetResourcePath(GetCurrentResourceName())
local file = io.open(path.."//config.json", "r") -- Открываем файл в режиме чтения
a = file:read("*a") -- Читаем файл, там у нас таблица
file:close() -- Закрываем
t = json.decode(a)
--Проверяем какой фреймворк указан в файле
if (t.Framework == "QBCore") then
    QBCore = exports['qb-core']:GetCoreObject()
end

RegisterNetEvent('MyAiMech:server:IsCanPay', function(price)
    local src = source
    local Player = QBCore.Functions.GetPlayer(src)
    local CanPay = false
    if (Player.PlayerData.money.bank>=price or Player.PlayerData.money.chash>=price) then
        CanPay = true

    end

    TriggerClientEvent("MyAiMech:client:IsCanPay", source, CanPay, Player.PlayerData.money.bank, Player.PlayerData.money.cash)

end)
RegisterNetEvent('MyAiMech:server:RemoveBank', function(price)
    local src = source
    local Player = QBCore.Functions.GetPlayer(src)
    if (Player.PlayerData.money.bank>=price or Player.PlayerData.money.chash>=price) then
        Player.Functions.RemoveMoney('bank', Config.StandDeposit, 'aimech')
    end
end)
RegisterNetEvent('MyAiMech:server:RemoveCash', function(price)
    local src = source
    local Player = QBCore.Functions.GetPlayer(src)
    if (Player.PlayerData.money.bank>=price or Player.PlayerData.money.chash>=price) then
        Player.Functions.RemoveMoney('cash', Config.StandDeposit, 'aimech')
    end
end)

