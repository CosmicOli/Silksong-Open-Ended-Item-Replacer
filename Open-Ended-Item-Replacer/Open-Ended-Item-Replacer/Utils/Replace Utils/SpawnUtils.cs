using GlobalSettings;
using Open_Ended_Item_Replacer.Components;
using UnityEngine;
using static UnityEngine.Object;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.InfoUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Utils.Replace_Utils
{
    internal class SpawnUtils
    {
        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericInteractablePickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
        {
            // If no prefab is provided, a generic pickup prefab is used
            if (!prefab)
            {
                logSource.LogInfo("No prefab provided, using CollectableItemPickupPrefab");
                prefab = Gameplay.CollectableItemPickupPrefab;
            }

            // Defines the spawn location of the replacement pickup
            Vector3 vector = spawnPoint.position + offset;
            Vector3 position = vector;

            CollectableItemPickup collectableItemPickup;

            // Creates the new pickup and sets its position
            collectableItemPickup = Instantiate(prefab);
            collectableItemPickup.transform.position = position;
            collectableItemPickup.gameObject.name = uniqueID.PickupName;

            SetGenericPickupInfo(uniqueID, collectableItemPickup);

            return collectableItemPickup.transform;
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericCollisionPickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
        {
            // If no prefab is provided, a generic pickup prefab is used
            if (!prefab)
            {
                logSource.LogInfo("No prefab provided, using CollectableItemPickupInstantPrefab");
                prefab = Gameplay.CollectableItemPickupInstantPrefab;
            }

            // Defines the spawn location of the replacement pickup
            Vector3 vector = spawnPoint.position + offset;
            Vector3 position = vector;

            CollectableItemPickup collectableItemPickup;

            // Creates the new pickup and sets its position
            collectableItemPickup = Instantiate(prefab);
            collectableItemPickup.transform.position = position;
            collectableItemPickup.gameObject.name = uniqueID.PickupName;
            collectableItemPickup.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;

            SetGenericPickupInfo(uniqueID, collectableItemPickup);

            return collectableItemPickup.transform;
        }
    }
}
