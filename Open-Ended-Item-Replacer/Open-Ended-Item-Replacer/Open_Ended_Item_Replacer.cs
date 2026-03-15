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

        

        
    }
}
