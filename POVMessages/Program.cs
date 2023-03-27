using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Fallout4;
using System.Text.Json;
using System;

namespace POVMessages
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "POVPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state)
        {
            Console.WriteLine("POV Messages Start");

            var pathToInternalFile = state.RetrieveInternalFile("POVMap.json");
            string jsonModString = File.ReadAllText(pathToInternalFile);
            var POVMap = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonModString);
            Console.WriteLine($"Using POVMap.json");

            int i = 0;
            foreach (var eachGameSetting in state.LoadOrder.PriorityOrder.GameSetting().WinningOverrides())
            {
                if (eachGameSetting is null || eachGameSetting.Type is null || eachGameSetting.Type.ToString() != "Mutagen.Bethesda.Fallout4.IGameSettingString") continue;
                var gameSettingString = (IGameSettingStringGetter)eachGameSetting;
                if (gameSettingString is null || gameSettingString.Data is null || gameSettingString.Data.ToString() == "" || gameSettingString.Data.ToString() == " ") continue;
                string? originalString = gameSettingString.Data.ToString();
                if (originalString is null || POVMap is null) continue;
                string newString = originalString;
                foreach (var key in POVMap.Keys)
                {
                    newString = newString.Replace(key, POVMap[key]);
                }
                if (newString == originalString) continue;
                var gameSetting = state.PatchMod.GameSettings.GetOrAddAsOverride(eachGameSetting);
                var newGameSettingString = (IGameSettingString)gameSetting;
                newGameSettingString.Data = newString;
                Console.WriteLine(eachGameSetting.EditorID);
                Console.WriteLine($"Original string: {originalString}");
                Console.WriteLine($"New string: {newString}");
                i++;
            }
            Console.WriteLine($"Patched {i} game settings.");

            i = 0;
            foreach (var eachMessage in state.LoadOrder.PriorityOrder.Message().WinningOverrides())
            {
                string? description = eachMessage.Description.ToString();
                if (description is null || POVMap is null) continue;

                string originalDescription = description;
                foreach (var key in POVMap.Keys)
                {
                    description = description.Replace(key, POVMap[key]);
                }
                if (description == originalDescription) continue;

                var newMessage = state.PatchMod.Messages.GetOrAddAsOverride(eachMessage); 
                newMessage.Description = description;
                Console.WriteLine($"Old Description: {originalDescription}");
                Console.WriteLine($"New Description: {description}");
                i++;
            }
            Console.WriteLine($"Patched {i} messages.");
        }
    }
}
