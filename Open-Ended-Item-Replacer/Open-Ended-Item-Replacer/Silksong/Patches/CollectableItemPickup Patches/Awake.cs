using BepInEx;
using HarmonyLib;
using UnityEngine;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.ReplaceUtils;
using static Open_Ended_Item_Replacer.Silksong.Patches.NailSlash_Patches.StartSlash;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using System.Collections.Generic;
using System.Linq;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches
{

    [HarmonyPatch(typeof(CollectableItemPickup), "Awake")]
    internal class Awake
    {
        private static void Prefix(CollectableItemPickup __instance)
        {
            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!SpawningReplacement)
            {
                // Fixes original persistence taking effect
                Traverse.Create(__instance).Field("persistent").SetValue(null);

                // For logging
                string playerDataBool = Traverse.Create(__instance).Field("playerDataBool").GetValue<string>();
                if (!playerDataBool.IsNullOrWhiteSpace())
                {
                    logSource.LogInfo("PlayerDataBool: " + playerDataBool);
                }

                // Stops player data bool based persistence checks
                Traverse.Create(__instance).Field("playerDataBool").SetValue(null);
            }
        }

        // Replaces CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        private static void Postfix(CollectableItemPickup __instance)
        {
            logSource.LogMessage("CollectableItemPickup Awake");

            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!SpawningReplacement)
            {
                if (__instance.Item == null) { return; }
                if (__instance.gameObject == null) { return; }

                bool originalActive = __instance.gameObject.activeSelf;

                if (__instance.gameObject.name.ToLowerInvariant().Contains("tool metal")) // Craftmetal has problems replacing the original pickup, so a dummy is used
                {
                    GameObject dummyGameObject = new GameObject(__instance.gameObject.name + "-DummyParent");
                    testTransform = Replace(__instance.gameObject, dummyGameObject, __instance.Item.name); // INTERACTABLE true
                }
                /*else if (__instance.Item.name.Contains(""))
                {
                    testTransform = ReplaceWithCostedPickup(__instance.gameObject, __instance.Item.name, CurrencyType.Money, 0, new List<SavedItem>() { CollectableItemManager.GetItemByName("Tool Metal") }, new List<int>() { 1 });
                }*/
                else
                {
                    testTransform = Replace(__instance.gameObject, __instance.Item.name); // INTERACTABLE true
                }
            }
        }
    }
}
