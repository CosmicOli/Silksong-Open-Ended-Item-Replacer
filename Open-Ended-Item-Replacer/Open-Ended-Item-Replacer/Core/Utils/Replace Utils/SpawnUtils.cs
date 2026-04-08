using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.InfoUtils;
using static UnityEngine.Object;

namespace Open_Ended_Item_Replacer.Core.Utils.Replace_Utils
{
    internal class SpawnUtils
    {
        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericInteractablePickup<InteractableContainer>(InteractableContainer prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool SpawningReplacement = true)
            where InteractableContainer : MonoBehaviour, IContainer, IInteractable
        {
            Open_Ended_Item_Replacer.SpawningReplacement = SpawningReplacement;

            try
            {
                // If no prefab is provided, a generic pickup prefab is used
                if (!prefab)
                {
                    logSource.LogInfo("No prefab provided, using CollectableItemPickupPrefab");
                    prefab = DefaultInteractableContainer;
                }

                // Defines the spawn location of the replacement pickup
                Vector3 vector = spawnPoint.position + offset;
                Vector3 position = vector;

                InteractableContainer container;

                // Creates the new pickup and sets its position
                container = Instantiate(prefab);
                container.transform.position = position;
                container.gameObject.name = uniqueID.PickupName;

                // This logs where the pickup has been placed
                logSource.LogInfo("New Pickup Placed At: " + container.transform.position);

                SetGenericPickupInfo(uniqueID, container);

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

        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericCollisionPickup<CollisionableContainer>(CollisionableContainer prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool SpawningReplacement = true)
            where CollisionableContainer : MonoBehaviour, IContainer, ICollisionable
        {
            Open_Ended_Item_Replacer.SpawningReplacement = SpawningReplacement;

            try
            {
                // If no prefab is provided, a generic pickup prefab is used
                if (!prefab)
                {
                    logSource.LogInfo("No prefab provided, using CollectableItemPickupInstantPrefab");
                    prefab = DefaultCollisionContainer;
                }

                // Defines the spawn location of the replacement pickup
                Vector3 vector = spawnPoint.position + offset;
                Vector3 position = vector;

                CollisionableContainer container;

                // Creates the new pickup and sets its position
                container = Instantiate(prefab);
                container.transform.position = position;
                container.gameObject.name = uniqueID.PickupName;
                container.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;

                // This logs where the pickup has been placed
                logSource.LogInfo("New Pickup Placed At: " + container.transform.position);

                SetGenericPickupInfo(uniqueID, container);

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

        public static Transform SpawnGenericCostedPickup<CostedContainer>(CostedContainer prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IInteractable, ICosted
        {
            return SpawnGenericCostedPickup(prefab, uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, null, null, true, true, null, SpawningReplacement: SpawningReplacement);
        }

        public static Transform SpawnGenericCostedPickup<CostedContainer>(CostedContainer prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IInteractable, ICosted
        {
            return SpawnGenericCostedPickup(prefab, uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, requiredItems, itemAmounts, true, true, null, SpawningReplacement: SpawningReplacement);
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        public static bool choosing;
        public static bool purchased;
        public static Transform SpawnGenericCostedPickup<CostedContainer>(CostedContainer prefab, UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool returnHud, string text, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool displayHudPopup, bool consumeCurrency, SavedItem willGetItem, TakeItemTypes takeItemType = TakeItemTypes.Silent, YesNoAction.DisplayType displayType = YesNoAction.DisplayType.RequiredItems, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IInteractable, ICosted
        {
            Open_Ended_Item_Replacer.SpawningReplacement = SpawningReplacement;

            try
            {
                // GenericPickup is another good option here, but it does not have persistence built in so I am going to stick with CollectableItemPickups

                // Defines the spawn location of the replacement pickup
                Vector3 vector = spawnPoint.position + offset;
                Vector3 position = vector;

                CostedContainer container;

                // Creates the new pickup and sets its position
                container = Instantiate(prefab);
                container.transform.position = position;
                container.gameObject.name = uniqueID.PickupName;

                // This logs where the pickup has been placed
                logSource.LogInfo("New Pickup Placed At: " + container.transform.position);

                InteractEvents interactEvents = container.interactEvents;

                SetGenericPickupInfo(uniqueID, container);

                string itemName = text;
                if (itemName == "")
                {
                    itemName = container.Item.GetPopupName();
                }

                container.SpawnSetup();

                HeroController HCinstance = HeroController.instance;

                interactEvents.Interacted += delegate
                {
                    container.InteractSetup();

                    choosing = true;
                    purchased = false;
                    HCinstance.OnTakenDamage += container.OnCancel;
                };

                interactEvents.Interacted += delegate
                {
                    try
                    {
                        // Commented out as for some reason force closing the box means the pickup becomes givable on contact instead of on purchase
                        // instance.OnTakenDamage += delegate { DialogueYesNoBox.ForceClose(); };

                        DialogueYesNoBox.Open(container.Yes, container.No, returnHud, itemName, currencyType, currencyAmount, requiredItems, itemAmounts, displayHudPopup, consumeCurrency, willGetItem, takeItemType, displayType);
                    }
                    catch (Exception e)
                    {
                        container.OnCancel();
                        logSource.LogError(e);
                    }
                };

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

        public static void OnCancel(ICosted costed)
        {
            purchased = false;
            choosing = false;
            HeroController.instance.OnTakenDamage -= costed.OnCancel;
        }
    }
}
