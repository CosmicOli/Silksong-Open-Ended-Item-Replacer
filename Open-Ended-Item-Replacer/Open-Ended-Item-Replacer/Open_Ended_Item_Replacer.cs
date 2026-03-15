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

using static Open_Ended_Item_Replacer.Patches.NailSlash_Patches.StartSlash;
using static Open_Ended_Item_Replacer.Patches.PlayMakerFSM_Patches.Awake;

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
            harmony.PatchAll(typeof(Awake));

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

        // Replaces physical Mask Shards and Spool Fragments
        // All physically placed mask shards (heart piece) and spool fragments (silk spool) have persistent bools attributed to them
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PersistentBoolItem), "Awake")]
        private static void PersistentBoolItem_Awake_Postfix(PersistentBoolItem __instance)
        {
            if (__instance.ItemData.ID.ToLowerInvariant().StartsWith("heart piece"))
            {
                //logSource.LogInfo("Heart Piece");
                Replace(__instance.gameObject, "Heart Piece", false, null);
            }

            if (__instance.ItemData.ID.ToLowerInvariant().StartsWith("silk spool"))
            {
                //logSource.LogInfo("Silk Spool");
                Replace(__instance.gameObject, "Silk Spool", false, null);
            }
        }

        

        

        

        

        

        

        

        

        

        // Handles FSM checks
        // All fleas have SavedItems that are gotten at the end of their fsms
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerFSM_Awake_Postfix(PlayMakerFSM __instance)
        {
            /*HandleFlea(__instance);

            HandleWeaverStatue(__instance);

            HandleCrest(__instance);
            HandleCrestDoor(__instance);

            HandleSilkNeedle(__instance);

            HandleSilkHeart(__instance);

            HandleNeedolinPreMemory(__instance);
            HandleNeedolinInMemory(__instance);

            HandleFirstSinnerPersistenceAndPickup(__instance);
            HandleFirstSinnerInMemory(__instance);

            HandlePhantom(__instance);

            HandleArchitectMelody(__instance);
            HandleConductorMelody(__instance);
            HandleLibrarianMelody(__instance);

            HandlePlinney(__instance);

            HandleSeamstress(__instance);
            HandleFourthChorus(__instance);

            HandlePinstress(__instance);

            HandleFaydownCloak(__instance);

            HandleEva(__instance);

            HandleNuu(__instance);

            HandleGrishkin(__instance);
            HandleFleaCharm(__instance);
            HandleSethMemento(__instance);

            HandleChef(__instance);
            HandleNectar(__instance);
            //HandleMossDruid(__instance);

            HandleSurfaceMemento(__instance);

            TEST(__instance);
            HandleBellHermit(__instance);*/
        }


        // The original code does not skip base level hunter, so this needs to be removed
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CountCrestUnlockPoints), "OnEnter")]
        private static void CountCrestUnlockPoints_OnEnter_Postfix(CountCrestUnlockPoints __instance)
        {
            ToolCrestList toolCrestList = __instance.CrestList.Value as ToolCrestList;

            int currentPointsTally = 0;
            int maxPointsTally = 0;
            ToolCrest hunter = toolCrestList.GetByName("Hunter");

            if (!hunter.IsUpgradedVersionUnlocked)
            {
                ToolCrest.SlotInfo[] slots = hunter.Slots;
                for (int i = 0; i < slots.Length; i++)
                {
                    _ = ref slots[i];
                    maxPointsTally++;
                }

                ToolCrest.SlotInfo[] slots2 = hunter.Slots;
                List<ToolCrestsData.SlotData> slots3 = hunter.SaveData.Slots;
                for (int j = 0; j < slots2.Length; j++)
                {
                    if (!slots2[j].IsLocked || (slots3 != null && j < slots3.Count && slots3[j].IsUnlocked))
                    {
                        currentPointsTally++;
                    }
                }
            }

            __instance.StoreCurrentPoints.Value -= currentPointsTally;
            __instance.StoreMaxPoints.Value -= maxPointsTally;
        }

        // Handles when FSMs run CollectableItemCollect
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CollectableItemCollect), "DoAction")]
        private static bool CollectableItemCollect_DoAction_Prefix(CollectableItemCollect __instance, CollectableItem item)
        {
            ReplaceFsmItemGet(__instance, item);

            return false;
        }

        // Handles when FSMs run SavedItemGet
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGet), "OnEnter")]
        private static bool SavedItemGet_OnEnter_Prefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SavedItemGetV2
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGetV2), "OnEnter")]
        private static bool SavedItemGetV2_OnEnter_Prefix(SavedItemGetV2 __instance)
        {
            /*if (__instance.Item.Value.name.Contains(genericFleaItemName) && __instance.Item.Name.Contains("Generic_Item-"))
            {
                return true;
            }*/

            if (__instance.Item.Value.name.Contains("Needle Upgrade"))
            {
                GenericSavedItem needleUpgradeItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                PersistentItemData<bool> needleUpgradePersistentBoolData;

                for (int i = 1; i <= 4; i++)
                {
                    needleUpgradeItem.name = "Needle Upgrade " + i.ToString();
                    needleUpgradePersistentBoolData = GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade " + i.ToString());

                    if (!GetPersistentBoolFromData(needleUpgradePersistentBoolData))
                    {
                        ReplaceFsmItemGet(__instance, needleUpgradeItem);
                        break;
                    }
                }

                return false;
            }

            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }
        
        // Handles when FSMs run SavedItemGetDelayed
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGetDelayed), "DoGet")]
        private static bool SavedItemGetDelayed_DoGet_Prefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SetToolUnlocked
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetToolUnlocked), "OnEnter")]
        private static bool SetToolUnlocked_OnEnter_Prefix(SetToolUnlocked __instance)
        {
            bool flag = false;

            ToolItem item = __instance.Tool.Value as ToolItem;

            if (item?.name == "Flea Brew" && item.SavedData.IsUnlocked) // If this item is flea brew and flea brew is owned
            {
                flag = true;

                // Commented out to handle flea caravan side
                /*if (!CheckAllCaravanScenesForFleaBrew()) // If the player hasn't gotten the flea brew check yet, give it now
                {
                    GameObject fleaBrewGameObject = new GameObject(fleaBrew);

                    GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                    genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(fleaBrewGameObject, fleaBrew, genericItem);
                }*/

                return flag;
            }

            ReplaceFsmToolGet(__instance);

            __instance.Finish();
            return flag;
        }

        // Handles when FSMs run SetToolLocked
        // Stops NPCs locking tools when not actually replacing them
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetToolLocked), "OnEnter")]
        private static bool SetToolLocked_OnEnter_Prefix(SetToolLocked __instance)
        {
            /*string name = ((ToolItem)__instance.Tool.Value).name;
            if (name == "Silk Snare" || name == "Extractor")
            {
                return true;
            }
            else
            {
                __instance.Finish();
                return false;
            }*/

            __instance.Finish();
            return false;
        }

        /*public static void Ignore(string name, ref FsmEvent IsCollected, FsmEvent NotCollected)
        {
            // Not sure if this is necessary, but may be useful in the future for other checks
            if (name.ToLowerInvariant().Contains("memento"))
            {
                IsCollected = NotCollected;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemGetData), "DoAction")]
        public static void CollectableItemGetData_DoAction_Postfix(CollectableItemGetData __instance)
        {
            Ignore(__instance.Item.Name, ref __instance.IsCollected, __instance.NotCollected);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemGetDataV2), "DoAction")]
        public static void CollectableItemGetDataV2_DoAction_Postfix(CollectableItemGetDataV2 __instance)
        {
            Ignore(__instance.Item.Name, ref __instance.IsCollected, __instance.NotCollected);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemGetDataV3), "DoAction")]
        public static void CollectableItemGetDataV3_DoAction_Postfix(CollectableItemGetDataV3 __instance)
        {
            Ignore(__instance.Item.Name, ref __instance.IsCollected, __instance.NotCollected);
        }*/

        private static void HandleUiMsgGetItem(PlayMakerFSM playMakerFsm)
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

        private static void HandleUiMsgGetItemMelody(PlayMakerFSM playMakerFsm, SpawnObjectFromGlobalPool __instance)
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreateUIMsgGetItem), "OnEnter")]
        private static void CreateUIMsgGetItem_OnEnter_Prefix(CreateUIMsgGetItem __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.storeObject.Value.transform.GetComponent<PlayMakerFSM>();

            HandleUiMsgGetItem(playMakerFsm);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnObjectFromGlobalPool), "OnEnter")]
        private static void SpawnObjectFromGlobalPool_OnEnter_Prefix(SpawnObjectFromGlobalPool __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.gameObject?.Value?.transform?.GetComponent<PlayMakerFSM>();
            if (playMakerFsm == null) { return; }

            HandleUiMsgGetItemMelody(playMakerFsm, __instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreateObject), "OnEnter")]
        private static bool CreateObject_OnEnter_Prefix(CreateObject __instance)
        {
            if (__instance.gameObject.Value == null) { return true; }

            string loweredName = __instance.gameObject.Value.name.ToLowerInvariant();

            if (loweredName.Contains("silk spool") || loweredName.Contains("heart piece"))
            {
                string itemName;

                if (loweredName.Contains("silk spool"))
                {
                    itemName = "Silk Spool";
                }
                else
                {
                    itemName = "Heart Piece";
                }

                GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

                genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(__instance.Fsm.GameObject, itemName, genericItem);

                // Handles persistence set by new item
                if (!GetPersistentBoolFromData(genericItem.persistentBoolItem.ItemData))
                {
                    genericItem.Get();
                }

                foreach (var transition in __instance.State.Transitions)
                {
                    if (transition.EventName.Contains("SILK SPOOL UI END") || transition.EventName.Contains("HEART PIECE UI END"))
                    {
                        __instance.Fsm.SetState(transition.ToState);
                    }
                }

                // I believe this should be intended
                HeroController.instance.RegainControl(true);
                HeroController.instance.StartAnimationControl();

                return false;
            }

            return true;

            /*if (loweredName.Contains("ui msg crest evolve"))
            {
                PlayMakerFSM playMakerFsm = __instance.gameObject?.Value?.transform?.GetComponent<PlayMakerFSM>();
                if (playMakerFsm == null) { return; }

                HandleUiMsgCrestEvolve(playMakerFsm);
            }*/
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CollectableItemPickup), "Awake")]
        private static void CollectableItemPickup_Awake_Prefix(CollectableItemPickup __instance)
        {
            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!spawningReplacementCollectableItemPickup)
            {
                // Fixes original persistence taking effect
                Traverse.Create(__instance).Field("persistent").SetValue(null);

                // For logging
                string playerDataBool = Traverse.Create(__instance).Field("playerDataBool").GetValue<string>();
                if (!playerDataBool.IsNullOrWhiteSpace())
                {
                    logSource.LogInfo("PlayerDataBool: " + playerDataBool);
                }

                // Stops player data bool based persistence checks
                Traverse.Create(__instance).Field("playerDataBool").SetValue(null);
            }
        }

        // Replaces CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        private static bool spawningReplacementCollectableItemPickup = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "Awake")]
        private static void CollectableItemPickup_Awake_Postfix(CollectableItemPickup __instance)
        {
            logSource.LogMessage("CollectableItemPickup Awake");

            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!spawningReplacementCollectableItemPickup)
            {
                /*if (__instance.Item.name.Contains("Common Spine")) // will generalise a check for active later
                {
                    return;
                }*/

                if (__instance.Item == null) { return; }
                if (__instance.gameObject == null) { return; }

                bool originalActive = __instance.gameObject.activeSelf;

                if (__instance.gameObject.name.ToLowerInvariant().Contains("tool metal"))
                {
                    GameObject dummyGameObject = new GameObject(__instance.gameObject.name + "-DummyParent");
                    testTransform = Replace(__instance.gameObject, dummyGameObject, __instance.Item.name, true, null);
                }
                else
                {
                    testTransform = Replace(__instance.gameObject, __instance.Item.name, true, null);
                }
            }
        }

        // As some items check persistence using whether an item can be gotten anymore, this needs to be intercepted
        // A transpiler could be used instead to change the one line that is modified, but the relative difficulty compared to this method means this is what I will be doing for now
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CollectableItemPickup), "CheckActivation")]
        private static bool CollectableItemPickup_CheckActivation_Prefix(CollectableItemPickup __instance, bool ___activatedRead, string ___playerDataBool, PersistentBoolItem ___persistent, SavedItem ___item)
        {
            if (___activatedRead) // This first part resets items that can be continuously gotten that have already been picked up; this doesn't need changing for now
            {
                if (string.IsNullOrEmpty(___playerDataBool) && ___persistent == null && (___item == null || (!___item.IsUnique && ___item.CanGetMore())))
                {
                    ___activatedRead = false;
                    return false; // This stops the original code running
                }
            }
            else // This second part usually looks like "activatedRead = (bool)item && !item.CanGetMore();", but has been changed to ignore CanGetMore()
            {
                bool flag = false;

                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    GameObject replacementObject = __instance.transform.GetChild(i).gameObject;
                    if (replacementObject == null) { continue; } // Not sure if this is necessary

                    if (replacementObject.name.Contains(__instance.gameObject.name) && replacementObject.GetComponent<CollectableItemPickup>() != null)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    ___activatedRead = false;
                }
                else // If the item was never replaced (from either config or other reasons) the original code should run
                {
                    ___activatedRead = (bool)___item && !___item.CanGetMore();
                }
            }

            // The rest of this is unchanged
            if (___activatedRead)
            {
                if (__instance.OnPickedUp != null)
                {
                    __instance.OnPickedUp.Invoke();
                }

                if (__instance.OnPreviouslyPickedUp != null)
                {
                    __instance.OnPreviouslyPickedUp.Invoke();
                }

                __instance.gameObject.SetActive(value: false);
            }

            // This stops the original code running
            return false;
        }

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
