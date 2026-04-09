using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;

namespace Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils
{
    internal class InfoUtils
    {
        public static void SetGenericPickupInfo(UniqueID uniqueID, PersistentContainer container)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = container.ContainerPersistentBoolItem;
            logSource.LogInfo(persistent);

            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.PersistentBoolItem = persistent;

            // Sets the item granted upon pickup
            container.Item = genericItem;
            logSource.LogInfo("Pickup Item Set: " + genericItem.name);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(persistent.ItemData))
            {
                container.gameObject.SetActive(false); // Kinda not needed as disablePrefabIfActivated exists
                logSource.LogInfo("Replacement set inactive");
            }
        }
    }
}
