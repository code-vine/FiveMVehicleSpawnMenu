﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using VehicleMenu.Client.DataModels;
using static CitizenFX.Core.Native.API;

namespace VehicleMenu.Client
{
    public class VehicleMenuClient : BaseScript
    {
        private Dictionary<string, List<VehicleData>> _categorizedVehicles = new Dictionary<string, List<VehicleData>>();
        
        public VehicleMenuClient()
        {
            LoadVehicleData();

            // Register the spawn_vehicle callback
            RegisterNuiCallbackType("spawn_vehicle");
            EventHandlers["__cfx_nui:spawn_vehicle"] += new Action<IDictionary<string, object>, CallbackDelegate>(SpawnVehicle);


            RegisterNuiCallbackType("close_menu");
            EventHandlers["__cfx_nui:close_menu"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                SetNuiFocus(false, false);
                cb(new { status = "closed" });
            });


            // Command to open the menu manually
            RegisterCommand("openvehmenu", new Action<int, List<object>, string>((src, args, raw) =>
            {
                OpenMenu();

            }), false);



            RegisterSpawnCarCommand();
        }

        private void LoadVehicleData()
        {

            Task.Factory.StartNew(() =>
            {
                try
                {
                    _categorizedVehicles = GetCategorizedVehicles();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load vehicles: {ex.Message}");
                }
            });
        }

        private void SpawnVehicle(IDictionary<string, object> data, CallbackDelegate cb)
        {
            string result = "success";
            try
            {
                if (data != null && data.TryGetValue("model", out var modelObj))
                {
                    string model = modelObj?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(model))
                    {
                        ExecuteCommand($"spawncar {model}");
                    }
                    else
                    {
                        result = "invalid_model";
                    }
                }
                else
                {
                    result = "missing_model";
                }
            }
            catch (Exception ex)
            {
                result = $"error: {ex.Message}";
            }
            cb(new { status = result });
        }

        private void RegisterSpawnCarCommand()
        {
            RegisterCommand("spawncar", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // account for the argument not being passed
                var model = "adder";
                if (args.Count > 0)
                {
                    model = args[0].ToString();
                }

                // check if the model actually exists
                var hash = (uint)GetHashKey(model);
                if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                {
                    TriggerEvent("chat:addMessage", new
                    {
                        color = new[] { 255, 0, 0 },
                        args = new[] { "[CarSpawner]", $"Cannot spawn a {model}!" }
                    });
                    return;
                }

                // create the vehicle
                var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);

                // set the player ped into the vehicle and driver seat
                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // tell the player
                TriggerEvent("chat:addMessage", new
                {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "[CarSpawner]", $"Enjoy your new ^*{model}!" }
                });
            }), false);
        }

        private Dictionary<string, List<VehicleData>> GetCategorizedVehicles()
        {

            foreach (var vehicleObj in GetAllVehicleModels())
            {
                try
                {
                    string modelName = vehicleObj.ToString();

                    uint hash = (uint)GetHashKey(modelName); //Convert string to hash

                    string label = GetLabelText(GetDisplayNameFromVehicleModel(hash));
                    string category = GetVehicleClassFromModel(hash);

                    if (string.IsNullOrEmpty(label)) label = modelName;

                    var vehicleData = new VehicleData
                    {
                        Name = modelName.ToLower(),
                        DisplayName = label,
                        Category = category,
                        Image = $"{modelName.ToLower()}.webp",
                    };

                    if (!_categorizedVehicles.ContainsKey(category))
                        _categorizedVehicles[category] = new List<VehicleData>();

                    _categorizedVehicles[category].Add(vehicleData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to categorize vehicle: {vehicleObj} → {ex.Message}");
                }
            }

            return _categorizedVehicles;
        }

        private string GetVehicleClassFromModel(uint modelHash)
        {
            int classId = GetVehicleClassFromName(modelHash);

            switch (classId)
            {
                case 0: return "Compacts";
                case 1: return "Sedans";
                case 2: return "SUVs";
                case 3: return "Coupes";
                case 4: return "Muscle";
                case 5: return "Sports Classics";
                case 6: return "Sports";
                case 7: return "Super";
                case 8: return "Motorcycles";
                case 9: return "Off-Road";
                case 10: return "Industrial";
                case 11: return "Utility";
                case 12: return "Vans";
                case 13: return "Cycles";
                case 14: return "Boats";
                case 15: return "Helicopters";
                case 16: return "Planes";
                case 17: return "Service";
                case 18: return "Emergency";
                case 19: return "Military";
                case 20: return "Commercial";
                case 21: return "Trains";
                default: return "Misc";
            }
        }


        [Tick]
        public async Task HandleOpenMenuTick()
        {
            if (IsControlJustPressed(0, 170)) // F3 key
            {
                OpenMenu();
            }

            await Task.FromResult(0);
        }

        private void OpenMenu()
        {

            SetNuiFocus(true, true);
            var jsonPayload = JsonConvert.SerializeObject(
            new { type = "show", vehicles = _categorizedVehicles },
                new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                }
            );

            Debug.WriteLine("Sending JSON to NUI: " + jsonPayload);
            SendNuiMessage(jsonPayload);
        }
    }
}
