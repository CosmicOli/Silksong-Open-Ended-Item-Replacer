using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Core.Containers;

namespace Open_Ended_Item_Replacer.Core.Utils.Replace_Utils
{
    internal class InfoUtils
    {
        public static void SetGenericPickupInfo<T>(UniqueID uniqueID, T container)
            where T : MonoBehaviour, IContainer
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            container.Setup();
            PersistentBoolItem persistent = container.ContainerPersistentBoolItem;

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
