using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static UnityEngine.Object;

namespace Open_Ended_Item_Replacer.Core.Utils.Replace_Utils
{
    internal class SpawnUtils
    {
        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericPickup<Container>(Container prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool SpawningReplacement = true)
            where Container : MonoBehaviour, IContainer
        {
            if (!LoadGameRunPatched)
            {
                logSource.LogInfo("Attempted to spawn while !LoadGameRunPatched, returning null");
                return null;
            }

            Open_Ended_Item_Replacer.SpawningReplacement = SpawningReplacement;

            try
            {
                // Defines the spawn location of the replacement pickup
                Vector3 vector = spawnPoint.position + offset;
                Vector3 position = vector;

                Container container;

                // Creates the new pickup and sets its position
                container = Instantiate(prefab);
                container.transform.position = position;
                container.gameObject.name = uniqueID.PickupName;

                container.Setup(uniqueID);

                // This logs where the pickup has been placed
                logSource.LogInfo("New Pickup Placed At: " + container.transform.position);

                if (SpawningReplacement)
                {
                    Open_Ended_Item_Replacer.SpawningReplacement = false;
                }

                return container.transform;
            }
            catch (Exception e)
            {
                if (SpawningReplacement)
                {
                    Open_Ended_Item_Replacer.SpawningReplacement = false;
                }
                logSource.LogError(e);
                return null;
            }
        }
    }
}
