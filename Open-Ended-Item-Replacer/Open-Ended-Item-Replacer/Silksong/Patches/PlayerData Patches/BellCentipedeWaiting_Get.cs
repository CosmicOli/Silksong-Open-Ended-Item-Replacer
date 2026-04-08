using HarmonyLib;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.NeedolinHandler;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.PlayerData_Patches
{
    [HarmonyPatch(typeof(PlayerData), "BellCentipedeWaiting", MethodType.Getter)]
    internal class BellCentipedeWaiting_Get
    {
        // I want to make this require needolin, but entering act 3 inherently requires needolin so I will refrain for now
        private static void Postfix(PlayerData __instance, ref bool __result)
        {
            if (__instance.blackThreadWorld)
            {
                PersistentItemData<bool> persistent = GeneratePersistentBoolData_SameScene(BeastlingCallGameObjectName, BeastlingCall);
                persistent.SceneName = "Bellway_Centipede_Arena";

                __result = !GetPersistentBoolFromData(persistent);
            }
        }
    }
}
