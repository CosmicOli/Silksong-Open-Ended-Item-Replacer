using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.ReplaceUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.SavedItemGet_V1_2_Patches
{
    internal class OnEnter
    {
        // Handles when FSMs run SavedItemGet
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGet), "OnEnter")]
        public static bool SavedItemGet_OnEnter_Prefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SavedItemGetV2
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGetV2), "OnEnter")]
        public static bool SavedItemGetV2_OnEnter_Prefix(SavedItemGetV2 __instance)
        {
            /*if (__instance.Item.Value.name.Contains(genericFleaItemName) && __instance.Item.Name.Contains("Generic_Item-"))
            {
                return true;
            }*/

            if (__instance.Item.Value.name.Contains("Needle Upgrade"))
            {
                GenericSavedItem needleUpgradeItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                PersistentItemData<bool> needleUpgradePersistentBoolData;

                for (int i = 1; i <= 4; i++)
                {
                    needleUpgradeItem.name = "Needle Upgrade " + i.ToString();
                    needleUpgradePersistentBoolData = GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade " + i.ToString());

                    if (!GetPersistentBoolFromData(needleUpgradePersistentBoolData))
                    {
                        ReplaceFsmItemGet(__instance, needleUpgradeItem);
                        break;
                    }
                }

                return false;
            }

            logSource.LogWarning("TEST");

            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }
    }
}
