using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.InfoUtils;
using static UnityEngine.Object;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using Open_Ended_Item_Replacer.Core.Containers;

namespace Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils
{
    internal class SpawnUtils
    {
        public static Transform SpawnGenericCostedPickup<CostedContainer>(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IPersistent, ICosted
        {
            return SpawnGenericCostedPickup<CostedContainer>(uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, null, null, true, true, null, SpawningReplacement: SpawningReplacement);
        }

        public static Transform SpawnGenericCostedPickup<CostedContainer>(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IPersistent, ICosted
        {
            return SpawnGenericCostedPickup<CostedContainer>(uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, requiredItems, itemAmounts, true, true, null, SpawningReplacement: SpawningReplacement);
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        public static bool choosing;
        public static bool purchased;
        public static Transform SpawnGenericCostedPickup<CostedContainer>(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool returnHud, string text, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool displayHudPopup, bool consumeCurrency, SavedItem willGetItem, TakeItemTypes takeItemType = TakeItemTypes.Silent, YesNoAction.DisplayType displayType = YesNoAction.DisplayType.RequiredItems, bool SpawningReplacement = true)
            where CostedContainer : MonoBehaviour, IContainer, IPersistent, ICosted
        {
            Open_Ended_Item_Replacer.SpawningReplacement = SpawningReplacement;

            try
            {
                CostedContainer prefab = DefaultCostedContainer as CostedContainer;

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

                SetGenericPickupInfo(uniqueID, container as PersistentContainer);

                string itemName = text;
                if (itemName == "")
                {
                    itemName = (container.Item as SavedItem).GetPopupName();
                }

                container.SpawnSetup();

                InteractEvents interactEvents = container.InteractEvents;
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
