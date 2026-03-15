using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;

namespace Open_Ended_Item_Replacer.Patches.CountCrestUnlockPoints_Patches
{
    [HarmonyPatch(typeof(CountCrestUnlockPoints), "OnEnter")]
    internal class OnEnter
    {
        // The original code does not skip base level hunter, so this needs to be removed
        private static void Postfix(CountCrestUnlockPoints __instance)
        {
            ToolCrestList toolCrestList = __instance.CrestList.Value as ToolCrestList;

            int currentPointsTally = 0;
            int maxPointsTally = 0;
            ToolCrest hunter = toolCrestList.GetByName("Hunter");

            if (!hunter.IsUpgradedVersionUnlocked)
            {
                ToolCrest.SlotInfo[] slots = hunter.Slots;
                for (int i = 0; i < slots.Length; i++)
                {
                    _ = ref slots[i];
                    maxPointsTally++;
                }

                ToolCrest.SlotInfo[] slots2 = hunter.Slots;
                List<ToolCrestsData.SlotData> slots3 = hunter.SaveData.Slots;
                for (int j = 0; j < slots2.Length; j++)
                {
                    if (!slots2[j].IsLocked || (slots3 != null && j < slots3.Count && slots3[j].IsUnlocked))
                    {
                        currentPointsTally++;
                    }
                }
            }

            __instance.StoreCurrentPoints.Value -= currentPointsTally;
            __instance.StoreMaxPoints.Value -= maxPointsTally;
        }
    }
}
