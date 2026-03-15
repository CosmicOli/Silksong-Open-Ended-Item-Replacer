using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Open_Ended_Item_Replacer.FsmStateActions;

using Open_Ended_Item_Replacer.Components;

using Open_Ended_Item_Replacer.Patches.GameManager_Patches;
using Open_Ended_Item_Replacer.Patches.SceneAdditiveLoadConditional_Patches;
using Open_Ended_Item_Replacer.Patches.NailSlash_Patches;
using Open_Ended_Item_Replacer.Patches.PlayMakerFSM_Patches;
using Open_Ended_Item_Replacer.Patches.PersistentBoolItem_Patches;
using Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches;
using Open_Ended_Item_Replacer.Patches.CountCrestUnlockPoints_Patches;
using Open_Ended_Item_Replacer.Patches.CollectableItemCollect_Patches;
using Open_Ended_Item_Replacer.Patches.SavedItemGet_V1_2_Patches;
using Open_Ended_Item_Replacer.Patches.SavedItemGetDelayed_Patches;
using Open_Ended_Item_Replacer.Patches.CreateUIMsgGetItem_Patches;
using Open_Ended_Item_Replacer.Patches.SpawnObjectFromGlobalPool_Patches;
using Open_Ended_Item_Replacer.Patches.CreateObject_Patches;
using Open_Ended_Item_Replacer.Patches.SetToolUnlocked_Patches;
using Open_Ended_Item_Replacer.Patches.SetToolLocked_Patches;

using static Open_Ended_Item_Replacer.Patches.NailSlash_Patches.StartSlash;
using static Open_Ended_Item_Replacer.Patches.PlayMakerFSM_Patches.Awake;
using static Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches.Awake;

using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.WeaverStatueHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CrestHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkNeedleHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkHeartHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.NeedolinHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FirstSinnerHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PhantomHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.ThreefoldSongHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PlinneyHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.BrollyAndAssociatedHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PinstressHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FaydownHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.EvaHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.NuuHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FleaCaravanHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.LugoliHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CreigeHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.MossDruidHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SurfaceMementoHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkAndSoulHandler;


namespace Open_Ended_Item_Replacer
{
    // I'm sure I'll update the version number at some point, right?
    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        public static ManualLogSource logSource = new ManualLogSource("logSource");
        public static Vector3 defaultReplacedParentLocation = new Vector3(-250, -250);

        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(logSource);

            Harmony harmony = new Harmony("OEIR");

            harmony.PatchAll(typeof(Open_Ended_Item_Replacer));

            // GameManager Patches
            harmony.PatchAll(typeof(LevelActivated));
            harmony.PatchAll(typeof(TimePasses));
            harmony.PatchAll(typeof(TimePassesElsewhere));

            // SceneAdditiveLoadConditional Patches
            harmony.PatchAll(typeof(TryTestLoad));

            // NailSlash Patches
            harmony.PatchAll(typeof(StartSlash));

            // PlayMakerFsm Patches
            harmony.PatchAll(typeof(Patches.PlayMakerFSM_Patches.Awake));

            // PersistentBoolItem Patches
            harmony.PatchAll(typeof(Patches.PersistentBoolItem_Patches.Awake));

            // CollectableItemPickup Patches
            harmony.PatchAll(typeof(Patches.CollectableItemPickup_Patches.Awake));
            harmony.PatchAll(typeof(CheckActivation));

            // CountCrestUnlockPoints Patches
            harmony.PatchAll(typeof(Patches.CountCrestUnlockPoints_Patches.OnEnter));

            // CollectableItemCollect Patches
            harmony.PatchAll(typeof(DoAction));

            // SavedItemGet V1-2 Patches
            harmony.PatchAll(typeof(Patches.SavedItemGet_V1_2_Patches.OnEnter));

            // SavedItemGetDelayed Patches
            harmony.PatchAll(typeof(DoGet));

            // CreateUIMsgGetItem Patches
            harmony.PatchAll(typeof(Patches.CreateUIMsgGetItem_Patches.OnEnter));

            // SpawnObjectFromGlobalPool Patches
            harmony.PatchAll(typeof(Patches.SpawnObjectFromGlobalPool_Patches.OnEnter));

            // CreateObject Patches
            harmony.PatchAll(typeof(Patches.CreateObject_Patches.OnEnter));

            // SetToolUnlocked Patches
            harmony.PatchAll(typeof(Patches.SetToolUnlocked_Patches.OnEnter));

            // SetToolLocked Patches
            harmony.PatchAll(typeof(Patches.SetToolLocked_Patches.OnEnter));


            // If this were c# 9.0, I would shove this responsibility onto the handler classes themselves with a ModuleInitializer, but alas
            // NOTE: Some of these exist within the same handler but are still seperated in case another mod wished to override a distinct function
            AwakePatchEvent += HandleFlea;

            AwakePatchEvent += HandleWeaverStatue;

            AwakePatchEvent += HandleCrest;
            AwakePatchEvent += HandleCrestDoor;

            AwakePatchEvent += HandleSilkNeedle;

            AwakePatchEvent += HandleSilkHeart;

            AwakePatchEvent += HandleNeedolinPreMemory;
            AwakePatchEvent += HandleNeedolinInMemory;

            AwakePatchEvent += HandleFirstSinnerPersistenceAndPickup;
            AwakePatchEvent += HandleFirstSinnerInMemory;

            AwakePatchEvent += HandlePhantom;

            AwakePatchEvent += HandleArchitectMelody;
            AwakePatchEvent += HandleConductorMelody;
            AwakePatchEvent += HandleLibrarianMelody;

            AwakePatchEvent += HandlePlinney;

            AwakePatchEvent += HandleSeamstress;
            AwakePatchEvent += HandleFourthChorus;

            AwakePatchEvent += HandlePinstress;

            AwakePatchEvent += HandleFaydownCloak;

            AwakePatchEvent += HandleEva;

            AwakePatchEvent += HandleNuu;

            AwakePatchEvent += HandleGrishkin;
            AwakePatchEvent += HandleFleaCharm;
            AwakePatchEvent += HandleSethMemento;

            AwakePatchEvent += HandleChef;

            AwakePatchEvent += HandleNectar;

            AwakePatchEvent += HandleMossDruid;

            AwakePatchEvent += HandleSurfaceMemento;

            AwakePatchEvent += TEST;
            AwakePatchEvent += HandleBellHermit;


            associatedChapelSceneName.Add("Spinner", "Tut_05");
            associatedChapelSceneName.Add("Wanderer", "Chapel_Wanderer");
            associatedChapelSceneName.Add("Warrior", "Ant_19");
            associatedChapelSceneName.Add("Reaper", "Greymoor_20c");
            associatedChapelSceneName.Add("Toolmaster", "Under_20");

            Logger.LogInfo("Plugin loaded and initualised.");
            //MethodInfo DoMsgOriginal = typeof(UIMsgBase<>).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name == "DoMsg");
            //harmony.Patch(DoMsgOriginal, prefix: new HarmonyMethod(typeof(Open_Ended_Item_Replacer).GetMethod("UIMsgBase_DoMsgPrefix", BindingFlags.Static | BindingFlags.NonPublic)));*/
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "BeginSceneTransition")]
        public static void GameManager_BeginSceneTransition_Postfix(SceneLoadInfo info)
        {
            logSource.LogFatal(info.EntryGateName);
            logSource.LogFatal(info.HeroLeaveDirection);
        }*/

        public static string replacementFlag = "-(Replacement)";

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab, Vector3 offset = new Vector3())
        {
            return Replace(replacedObject, replacedObject, replacedItemName, interactable, replacementPrefab, offset);
        }

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, GameObject activeParent, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab, Vector3 offset = new Vector3())
        {
            // Sets up the replacement object to not be replaced itself
            spawningReplacementCollectableItemPickup = true;

            try
            {
                // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
                logSource.LogInfo("Pickup: " + replacedObject.name);

                // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
                logSource.LogInfo("Pickup At: " + replacedObject.transform.position);

                UniqueID uniqueID = new UniqueID(replacedObject, replacedItemName);

                Transform output;

                // Attempts to spawn the replacement object
                logSource.LogInfo("Pickup Drop Attempt Start");
                if (interactable)
                {
                    output = SpawnGenericInteractablePickup(uniqueID, replacementPrefab, replacedObject.transform, offset);
                }
                else
                {
                    output = SpawnGenericCollisionPickup(uniqueID, replacementPrefab, replacedObject.transform, offset);
                }
                logSource.LogInfo("Pickup Drop Attempt End");

                // Removes the original object, along with removing its gravity and collision
                // Note that scenes in this game only extend in postive x and y, so -250 -250 should be plenty out of the way
                replacedObject.transform.position = defaultReplacedParentLocation;
                activeParent.transform.position = defaultReplacedParentLocation;

                Rigidbody2D replacementRigidBody2D = replacedObject.GetComponent<Rigidbody2D>();
                if (replacementRigidBody2D != null)
                {
                    Rigidbody2D outputRigidBody2D = output.GetComponent<Rigidbody2D>();
                    if (outputRigidBody2D != null)
                    {
                        outputRigidBody2D.gravityScale = replacementRigidBody2D.gravityScale;
                        outputRigidBody2D.constraints = replacementRigidBody2D.constraints;
                    }

                    replacementRigidBody2D.gravityScale = 0;
                    replacementRigidBody2D.constraints = RigidbodyConstraints2D.FreezeAll;
                }

                Collider2D replacementCollider2D = replacedObject.GetComponent<Collider2D>();
                if (replacementCollider2D != null)
                {
                    replacementCollider2D.enabled = false;
                }

                output.parent = activeParent.transform;

                spawningReplacementCollectableItemPickup = false;

                return output;
            }
            catch (Exception e)
            {
                logSource.LogInfo("Failed to replace");
                spawningReplacementCollectableItemPickup = false;
            }

            return null;
        }

        public static void ReplaceGiantFleaPickup(Transform giantFlea, PlayMakerFSM giantFleaFSM, PlayMakerFSM __instance, GameObject fleaObject)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(fleaObject, GenericFleaItemName, genericItem);

            // Handle actions on "Stun" state
            FsmStateAction[] stunActions = giantFleaFSM.Fsm.GetState("Stun").Actions;

            FsmBool giantFleaBool = new FsmBool();
            giantFleaBool.Value = false;

            // Stops the giant flea being marked as saved in the player bools
            HutongGames.PlayMaker.Actions.SetPlayerDataBool setGiantFleaSaved = stunActions.OfType<HutongGames.PlayMaker.Actions.SetPlayerDataBool>().ToList()[0];
            Traverse.Create(setGiantFleaSaved).Field("value").SetValue(giantFleaBool);


            // Handle actions on "Deactivate" state
            FsmStateAction[] deactivateActions = giantFleaFSM.Fsm.GetState("Deactivate").Actions;

            SavedItemGetV2 getFleaItem = deactivateActions.OfType<SavedItemGetV2>().ToList()[0];
            getFleaItem.Item = genericItem;
            getFleaItem.ShowPopup = true;
            getFleaItem.Amount = 1;


            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.persistentBoolItem.ItemData))
            {
                giantFlea.gameObject.SetActive(false);
                __instance.gameObject.SetActive(false);
            }
        }

        public static void ReplaceFsmItemGet(FsmStateAction __instance, SavedItem item)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(gameObject, item.name, genericItem);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.persistentBoolItem.ItemData))
            {
                logSource.LogInfo("Replacement set inactive: " + genericItem.persistentBoolItem.ItemData.SceneName + "   " + genericItem.persistentBoolItem.ItemData.ID);
            }
            else
            {
                genericItem.Get();
            }
        }

        public static void ReplaceFsmToolGet(SetToolUnlocked __instance)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(gameObject, (__instance.Tool.Value as ToolItem).name, genericItem);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.persistentBoolItem.ItemData))
            {
                logSource.LogInfo("Replacement set inactive");
            }
            else
            {
                genericItem.Get();
            }
        }


        


        public static void HandleUiMsgGetItem(PlayMakerFSM playMakerFsm)
        {
            // As these fsms are spawned from a template, I am unsure whether the names will exactly match
            if (playMakerFsm.Fsm.Name.Contains("Msg Control") && playMakerFsm.gameObject.name.Contains("UI Msg Get Item"))
            {
                if (playMakerFsm.gameObject.name.Contains("Melody")) { return; }

                logSource.LogMessage("Ui Msg Get Item found");

                FsmState init = playMakerFsm.Fsm.GetState("Init");
                FsmState done = playMakerFsm.Fsm.GetState("Done");

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(playMakerFsm.Fsm, init, done);

                init.Actions = ReturnCombinedActions(newActions, init.Actions);
            }
        }

        public static void HandleUiMsgGetItemMelody(PlayMakerFSM playMakerFsm, SpawnObjectFromGlobalPool __instance)
        {
            // As these fsms are spawned from a template, I am unsure whether the names will exactly match
            if (playMakerFsm.Fsm.Name.Contains("Msg Control") && playMakerFsm.gameObject.name.Contains("UI Msg Get Item Melody"))
            {
                logSource.LogMessage("Ui Msg Get Item Melody found");

                FsmState init = playMakerFsm.Fsm.GetState("Init");
                FsmState stopPlaying = playMakerFsm.Fsm.GetState("Stop Playing");
                FsmState wait = playMakerFsm.Fsm.GetState("Wait");
                FsmState stopUp = playMakerFsm.Fsm.GetState("Stop Up");
                FsmState done = playMakerFsm.Fsm.GetState("Done");

                FsmStateAction[] newActionsForInit = new FsmStateAction[1];
                newActionsForInit[0] = new SetFsmActiveState(playMakerFsm.Fsm, init, stopPlaying);
                //Array.Copy(init.Actions, 0, newActionsForInit, numberOfNewActionsForInit, init.Actions.Length);
                //init.Actions = newActionsForInit;
                init.Actions = ReturnCombinedActions(newActionsForInit, init.Actions);

                (wait.Actions[0] as Wait).time = 0;

                FsmStateAction[] newActionsForStopUp = new FsmStateAction[1];
                newActionsForStopUp[newActionsForStopUp.Length - 1] = new SetFsmActiveState(playMakerFsm.Fsm, stopUp, done);
                //Array.Copy(init.Actions, 0, newActionsForStopUp, 0, init.Actions.Length);
                //stopUp.Actions = newActionsForStopUp;
                stopUp.Actions = ReturnCombinedActions(newActionsForStopUp, stopUp.Actions);

                foreach (FsmStateAction action in __instance.State.Actions)
                {
                    SendEventByName sendEventByName = action as SendEventByName;

                    if (sendEventByName != null)
                    {
                        sendEventByName.Enabled = false;
                    }
                }

                playMakerFsm.Fsm.Start(); // This would usually start in some other way that also shows the message, so this needs to be started independantly
            }
        }

        /*private static void HandleUiMsgCrestEvolve(PlayMakerFSM playMakerFsm)
        {
            // As these fsms are spawned from a template, I am unsure whether the names will exactly match
            if (playMakerFsm.Fsm.Name.Contains("Msg Control") && playMakerFsm.gameObject.name.Contains("UI Msg Crest Evolve"))
            {
                logSource.LogMessage("UI Msg Crest Evolve found");

                FsmState init = playMakerFsm.Fsm.GetState("Init");
                FsmState done = playMakerFsm.Fsm.GetState("Done");

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                init.Actions[8] = new SetFsmActiveState(playMakerFsm.Fsm, init, done);
            }
        }*/

        



        

        

        

        

        // If a CollectableItemPickup changes active, the replaced object should instead
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(GameObject), "SetActive")]
        private static bool GameObject_SetActive_Prefix(GameObject __instance, bool value)
        {
            if (__instance.GetComponent<CollectableItemPickup>() != null && !spawningReplacementCollectableItemPickup)
            {
                logSource.LogWarning("Changing active");

                GameObject replacementObject = GameObject.Find(__instance.name + replacementFlag);

                replacementObject.SetActive(value);

                return false;
            }

            return true;
        }*/

        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnSetSaveState")]
        private static void testtest(bool value)
        {
            logSource.LogMessage("VALUE: " + value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PersistentItem<bool>), "Start")]
        private static bool PersistentItem_Bool_Start_Prefix(PersistentItem<bool> __instance)
        {
            if (__instance.ItemData.ID.Contains("Weaver Totem"))
            {
                logSource.LogError("Flag 1");
                logSource.LogWarning(__instance.ItemData.Value);

                logSource.LogError("Flag end 1");
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PersistentItem<bool>), "Start")]
        private static void PersistentItem_Bool_Start_Postfix(PersistentItem<bool> __instance)
        {
            if (__instance.ItemData.ID.Contains("Weaver Totem"))
            {
                logSource.LogError("Flag 2");
                logSource.LogWarning(__instance.ItemData.Value);
                logSource.LogWarning(__instance.LoadedValue);
                logSource.LogError("Flag end 2");
            }
        }*/

        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(PersistentItemDataCollection<bool, SerializableBoolData>), "SetValue")]
        private static bool PersistentItemDataCollection_Bool_SetValue_Prefix(PersistentItemDataCollection<bool, SerializableBoolData> __instance, PersistentItemData<bool> itemData)
        {
            //throw new NotImplementedException();

            if (itemData.ID.Contains("Weaver Totem"))
            {
                logSource.LogError("FLAG 2");
                logSource.LogInfo(itemData.ID);
                logSource.LogInfo(itemData.Value);
                logSource.LogError("FLAG END 2");
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PersistentItem<bool>), "SaveStateNoCondition")]
        private static bool PersistentItem_Bool_SaveStateNoCondition_Prefix(PersistentItem<bool> __instance)
        {
            //throw new NotImplementedException();

            if (__instance.ItemData.ID.Contains("Weaver Totem"))
            {
                logSource.LogError("FLAG");
                var fieldInfo = typeof(PersistentItem<bool>).GetField(
                    "OnSetSaveState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    var eventDelegate = fieldInfo.GetValue(__instance) as MulticastDelegate;
                    if (eventDelegate != null) // will be null if no subscribed event consumers
                    {
                        var delegates = eventDelegate.GetInvocationList();

                        foreach (var deleg in delegates)
                        {
                            if (deleg != null)
                            {
                                logSource.LogWarning(deleg.Method.DeclaringType);
                            }
                        }
                    }
                }

                logSource.LogInfo(__instance.ItemData.ID);
                logSource.LogInfo(__instance.ItemData.Value);

                var fieldInfo2 = typeof(PersistentItem<bool>).GetField(
                    "OnGetSaveState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo2 != null)
                {
                    var eventDelegate2 = fieldInfo2.GetValue(__instance) as MulticastDelegate;
                    if (eventDelegate2 != null) // will be null if no subscribed event consumers
                    {
                        var delegates = eventDelegate2.GetInvocationList();

                        foreach (var deleg in delegates)
                        {
                            if (deleg != null)
                            {
                                logSource.LogWarning(deleg.Method.DeclaringType);
                            }
                        }
                    }
                }

                logSource.LogError("FLAG END");
            }
            return true;
        }*/

        // Spawns a replacement pickup, defining the item with uniqueID
        private static Transform SpawnGenericInteractablePickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
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
        private static Transform SpawnGenericCollisionPickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
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

        private static void SetGenericPickupInfo(UniqueID uniqueID, CollectableItemPickup collectableItemPickup)
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
