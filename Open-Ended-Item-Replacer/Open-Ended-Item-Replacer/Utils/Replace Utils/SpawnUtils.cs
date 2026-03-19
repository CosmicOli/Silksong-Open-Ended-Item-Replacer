using BepInEx;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TeamCherry.Localization;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.InfoUtils;
using static UnityEngine.Object;
using static Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches.Awake;

namespace Open_Ended_Item_Replacer.Utils.Replace_Utils
{
    internal class SpawnUtils
    {
        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericInteractablePickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset, bool SpawningReplacementCollectableItemPickup = true)
        {
            spawningReplacementCollectableItemPickup = SpawningReplacementCollectableItemPickup;

            try
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

                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }

                return collectableItemPickup.transform;
            }
            catch (Exception e)
            {
                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }
                logSource.LogError(e);
                return null;
            }
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        public static Transform SpawnGenericCollisionPickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset, bool SpawningReplacementCollectableItemPickup = true)
        {
            spawningReplacementCollectableItemPickup = SpawningReplacementCollectableItemPickup;

            try
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

                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }

                return collectableItemPickup.transform;
            }
            catch (Exception e)
            {
                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }
                logSource.LogError(e);
                return null;
            }
        }

        public static Transform SpawnGenericCostedPickup(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, bool SpawningReplacementCollectableItemPickup = true) 
        {
            return SpawnGenericCostedPickup(uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, null, null, true, true, null, SpawningReplacementCollectableItemPickup: SpawningReplacementCollectableItemPickup);
        }

        public static Transform SpawnGenericCostedPickup(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool SpawningReplacementCollectableItemPickup = true)
        {
            return SpawnGenericCostedPickup(uniqueID, spawnPoint, offset, true, "", currencyType, currencyAmount, requiredItems, itemAmounts, true, true, null, SpawningReplacementCollectableItemPickup: SpawningReplacementCollectableItemPickup);
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        public static bool choosing;
        public static bool purchased;
        public static Transform SpawnGenericCostedPickup(UniqueID uniqueID, Transform spawnPoint, Vector3 offset, bool returnHud, string text, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, bool displayHudPopup, bool consumeCurrency, SavedItem willGetItem, TakeItemTypes takeItemType = TakeItemTypes.Silent, YesNoAction.DisplayType displayType = YesNoAction.DisplayType.RequiredItems, bool SpawningReplacementCollectableItemPickup = true)
        {
            spawningReplacementCollectableItemPickup = SpawningReplacementCollectableItemPickup;

            try
            {
                // GenericPickup is another good option here, but it does not have persistence built in so I am going to stick with CollectableItemPickups
                CollectableItemPickup prefab = Gameplay.CollectableItemPickupPrefab;

                // Defines the spawn location of the replacement pickup
                Vector3 vector = spawnPoint.position + offset;
                Vector3 position = vector;

                CollectableItemPickup collectableItemPickup;

                // Creates the new pickup and sets its position
                collectableItemPickup = Instantiate(prefab);
                collectableItemPickup.transform.position = position;
                collectableItemPickup.gameObject.name = uniqueID.PickupName;

                InteractEvents interactEvents = Traverse.Create(collectableItemPickup).Field("interactEvents").GetValue<InteractEvents>();

                SetGenericPickupInfo(uniqueID, collectableItemPickup);
                SavedItem item = collectableItemPickup.Item;
                SavedItem dummyItem = ScriptableObject.CreateInstance<FakeCollectable>();
                collectableItemPickup.SetItem(dummyItem, true);

                string itemName = text;
                if (itemName == "")
                {
                    itemName = item.GetPopupName();
                }

                HeroController HCinstance = HeroController.instance;
                void OnCancel()
                {
                    CollectableItemPickup.IsPickupPaused = false;
                    blockNextFsmEventTransmition = false;
                    purchased = false;
                    choosing = false;

                    HCinstance.OnTakenDamage -= OnCancel;
                }

                interactEvents.Interacted += delegate
                {
                    collectableItemPickup.SetItem(null, true);
                    logSource.LogWarning("Item set to null to force DoPickupAction to return false");

                    CollectableItemPickup.IsPickupPaused = true;
                    blockNextFsmEventTransmition = true;
                    choosing = true;
                    purchased = false;

                    HCinstance.OnTakenDamage += OnCancel;
                };

                interactEvents.Interacted += delegate
                {
                    try
                    {
                        // Commented out as for some reason force closing the box means the pickup becomes givable on contact instead of on purchase
                        //HeroController instance = HeroController.instance;
                        //instance.OnTakenDamage += delegate { DialogueYesNoBox.ForceClose(); };

                        DialogueYesNoBox.Open(delegate
                        {
                            //logSource.LogInfo("yes");
                            collectableItemPickup.SetItem(dummyItem, true);
                            CollectableItemPickup.IsPickupPaused = false;
                            logSource.LogWarning("Item reset to dummyItem");
                            collectableItemPickup.SetActivation(true);
                            Purchase(collectableItemPickup, item);
                            HCinstance.OnTakenDamage -= OnCancel;
                            purchased = true;
                        }, delegate
                        {
                            //logSource.LogInfo("no");
                            CollectableItemPickup.IsPickupPaused = false;
                            collectableItemPickup.SetActivation(false);
                            HCinstance.OnTakenDamage -= OnCancel;
                            purchased = false;
                        }, returnHud, itemName, currencyType, currencyAmount, requiredItems, itemAmounts, displayHudPopup, consumeCurrency, willGetItem, takeItemType, displayType);
                    }
                    catch (Exception e)
                    {
                        OnCancel();
                        logSource.LogError(e);
                    }
                };

                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }

                return collectableItemPickup.transform;
            }
            catch (Exception e)
            {
                if (SpawningReplacementCollectableItemPickup)
                {
                    spawningReplacementCollectableItemPickup = false;
                }
                logSource.LogError(e);
                return null;
            }
        }

        private static void Purchase(CollectableItemPickup collectableItemPickup, SavedItem item)
        {
            if (collectableItemPickup.OnPickup != null)
            {
                collectableItemPickup.OnPickup.Invoke();
            }

            if (collectableItemPickup.OnPickedUp != null)
            {
                collectableItemPickup.OnPickedUp.Invoke();
            }

            SpriteRenderer spriteRenderer = Traverse.Create(collectableItemPickup).Field("spriteRenderer").GetValue<SpriteRenderer>();
            GameObject extraEffects = Traverse.Create(collectableItemPickup).Field("extraEffects").GetValue<GameObject>();
            GameObject pickupEffect = Traverse.Create(collectableItemPickup).Field("pickupEffect").GetValue<GameObject>();
            bool smallGetEffect = Traverse.Create(collectableItemPickup).Field("smallGetEffect").GetValue<bool>();
            bool showPopup = Traverse.Create(collectableItemPickup).Field("showPopup").GetValue<bool>();

            if ((bool)spriteRenderer)
            {
                NestedFadeGroup component = spriteRenderer.GetComponent<NestedFadeGroup>();
                if ((bool)component)
                {
                    component.AlphaSelf = 0f;
                }
                else
                {
                    spriteRenderer.enabled = false;
                }
            }

            if ((bool)extraEffects)
            {
                extraEffects.SetActive(value: false);
            }

            if ((bool)pickupEffect)
            {
                pickupEffect.SetActive(value: true);
            }

            if (!showPopup)
            {
                CollectableItemHeroReaction.DoReaction(new Vector2(0f, -0.76f), smallGetEffect);
            }

            MethodInfo DoPickup = typeof(CollectableItemPickup).GetMethod("StopRBMovement", BindingFlags.Instance | BindingFlags.NonPublic);
            DoPickup.Invoke(collectableItemPickup, new object[] { });

            item.Get();
        }
    }
}
