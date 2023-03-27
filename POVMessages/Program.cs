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

            //foreach (var eachGameSetting in state.LoadOrder.PriorityOrder.GameSetting().WinningOverrides())
            //{
            //    switch (eachGameSetting)
            //    {
            //        case IGameSettingString stringGetter:
            //            if (stringGetter is null || stringGetter.Data is null) break;
            //            Console.WriteLine(stringGetter.Data);
            //            break;
            //    }
            //    //var gameSetting = state.PatchMod.GameSettings.GetOrAddAsOverride(eachGameSetting);
            //}

            int i = 0;
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
