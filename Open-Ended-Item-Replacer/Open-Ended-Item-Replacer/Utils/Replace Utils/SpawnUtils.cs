using BepInEx;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Components;
using System;
using System.Collections;
using System.Reflection;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.InfoUtils;
using static UnityEngine.Object;

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

        // Spawns a replacement pickup, defining the item with uniqueID
        public static bool bought;
        public static bool choosing;
        public static Transform SpawnGenericCostedPickup(UniqueID uniqueID, Transform spawnPoint, Vector3 offset)
        {
            // GenericPickup is another good option here, but it does not have persistence built in so I am going to stick with CollectableItemPickups
            CollectableItemPickup prefab = Gameplay.CollectableItemPickupPrefab;

            // Defines the spawn location of the replacement pickup
            Vector3 vector = spawnPoint.position + offset;
            Vector3 position = vector;

            CollectableItemPickup collectableItemPickup;
            //GameObject dummyParent = new GameObject();
            //CostedCollectableItemPickupContainer costedCollectableItemPickupContainer = dummyParent.AddComponent<CostedCollectableItemPickupContainer>();

            // Creates the new pickup and sets its position
            collectableItemPickup = Instantiate(prefab);
            collectableItemPickup.transform.position = position;
            collectableItemPickup.gameObject.name = uniqueID.PickupName;

            //costedCollectableItemPickupContainer = new CostedCollectableItemPickupContainer(collectableItemPickup);
            //collectableItemPickup.transform.parent = dummyParent.transform;

            InteractEvents interactEvents = Traverse.Create(collectableItemPickup).Field("interactEvents").GetValue<InteractEvents>();

            SetGenericPickupInfo(uniqueID, collectableItemPickup);
            SavedItem item = collectableItemPickup.Item;
            SavedItem dummyItem = ScriptableObject.CreateInstance<FakeCollectable>();
            collectableItemPickup.SetItem(dummyItem, true);

            // When the new prefab is instantiated, awake is called, but awake is called *after* this as it runs on the next update, so we don't need to fix the order of subscribed methods
            // In other words, the following commented out code isn't needed:
            //MethodInfo DoPickup = Traverse.Create(collectableItemPickup).Method("DoPickup").GetValue<MethodInfo>();
            //interactEvents.Interacted -= (System.Action)DoPickup.CreateDelegate(typeof(System.Action), interactEvents);

            // It looks like the original intention of interactEvents is that it is supposed to invoke Interacted when 'yes' is inputted from a yes no box, so I am using OnPickup
            /*collectableItemPickup.OnPickup.AddListener(delegate {
                CollectableItemPickup.IsPickupPaused = true;
                collectableItemPickup.SetItem(null, true);
                logSource.LogWarning("Item set to null to force DoPickupAction to return false");

                blockNextFsmEventTransmition = true;
                choosing = true;
            });

            collectableItemPickup.OnPickup.AddListener(delegate {
                DialogueYesNoBox.Open(delegate {
                    logSource.LogInfo("yes");
                    choosing = false;
                    bought = true;
                    collectableItemPickup.SetItem(dummyItem, true);
                    logSource.LogWarning("Item reset to dummyItem");
                }, delegate {
                    logSource.LogInfo("no");
                    choosing = false;
                    bought = false;
                }, true, "text", CurrencyType.Money, 10, displayHudPopup: true, true, item);
            });*/

            interactEvents.Interacted += delegate
            {
                CollectableItemPickup.IsPickupPaused = true;
                collectableItemPickup.SetItem(null, true);
                logSource.LogWarning("Item set to null to force DoPickupAction to return false");

                blockNextFsmEventTransmition = true;
                choosing = true;
                CollectableItemPickup.IsPickupPaused = true;
            };

            interactEvents.Interacted += delegate
            {
                DialogueYesNoBox.Open(delegate {
                    //logSource.LogInfo("yes");
                    bought = true;
                    collectableItemPickup.SetItem(dummyItem, true);
                    CollectableItemPickup.IsPickupPaused = false;
                    logSource.LogWarning("Item reset to dummyItem");
                    collectableItemPickup.SetActivation(true);
                    Purchase(collectableItemPickup);
                }, delegate {
                    //logSource.LogInfo("no");
                    bought = false;
                    CollectableItemPickup.IsPickupPaused = false;
                    collectableItemPickup.SetActivation(false);
                }, true, "text", CurrencyType.Money, 10, displayHudPopup: true, true, item);
            };

            return collectableItemPickup.transform; //dummyParent.transform;
        }

        private static void Purchase(CollectableItemPickup collectableItemPickup)
        {
            if (choosing)
            {
                choosing = false;

                if (bought)
                {
                    bought = false;

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
                }
            }
        }
    }
}
