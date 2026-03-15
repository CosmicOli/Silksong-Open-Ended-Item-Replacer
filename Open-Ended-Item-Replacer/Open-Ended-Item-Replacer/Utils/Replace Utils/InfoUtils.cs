using HarmonyLib;
using Open_Ended_Item_Replacer.Components;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Utils.Replace_Utils
{
    internal class InfoUtils
    {
        public static void SetGenericPickupInfo(UniqueID uniqueID, CollectableItemPickup collectableItemPickup)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            // Ignores areas that disallow replacement pickups such that it can exist anyways
            Traverse.Create(collectableItemPickup).Field("ignoreCanExist").SetValue(true);

            // This logs where the pickup has been placed
            logSource.LogInfo("New Pickup Placed At: " + collectableItemPickup.transform.position);

            PersistentBoolItem persistent = Traverse.Create(collectableItemPickup).Field("persistent").GetValue<PersistentBoolItem>();

            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;

            // Sets the item granted upon pickup
            collectableItemPickup.SetItem(genericItem, true);
            logSource.LogInfo("Pickup Item Set: " + genericItem.name);

            // Ensures new replacements made post-pickup don't reset the persistent data
            Traverse.Create(collectableItemPickup).Field("activatedSave").SetValue(true);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(persistent.ItemData))
            {
                collectableItemPickup.gameObject.SetActive(false); // Kinda not needed as disablePrefabIfActivated exists
                logSource.LogInfo("Replacement set inactive");
            }
        }
    }
}
