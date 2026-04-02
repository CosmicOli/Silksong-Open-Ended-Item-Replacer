using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Open_Ended_Item_Replacer.Patches.CollectableItemCollect_Patches;
using Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches;
using Open_Ended_Item_Replacer.Patches.FSMUtility_Patches;
using Open_Ended_Item_Replacer.Patches.FullQuestBase_Patches;
using Open_Ended_Item_Replacer.Patches.GameManager_Patches;
using Open_Ended_Item_Replacer.Patches.NailSlash_Patches;
using Open_Ended_Item_Replacer.Patches.PlayerData_Patches;
using Open_Ended_Item_Replacer.Patches.QuestBoardInteractable_Patches;
using Open_Ended_Item_Replacer.Patches.SavedItemGetDelayed_Patches;
using Open_Ended_Item_Replacer.Patches.SceneAdditiveLoadConditional_Patches;
using UnityEngine;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.BellwayHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.BrollyAndAssociatedHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CraftPickupHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CreigeHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CrestHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CrullAndBenjinHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.CurseHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.EvaHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FaydownHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FirstSinnerHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FleaCaravanHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.HeartsHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.KeyOfHereticHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.LugoliHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.MossDruidHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.NeedolinHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.NuuHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PhantomHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PinstressHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.PlinneyHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkAndSoulHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkHeartHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SilkNeedleHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.SurfaceMementoHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.ThreefoldSongHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.VentricaHandler;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.WeaverStatueHandler;
using static Open_Ended_Item_Replacer.Patches.PlayMakerFSM_Patches.Awake;


namespace Open_Ended_Item_Replacer
{
    // I'm sure I'll update the version number at some point, right?
    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        private static bool spawningReplacement = false;
        public static bool SpawningReplacement
        {
            get { return spawningReplacement; }
            set { spawningReplacement = value; }
        }

        // Currently only implemented to block FSMUtility.SendEventUpwards();
        public static bool blockNextFsmEventTransmition;
        public static bool BlockNextFsmEventTransmition
        {
            get { return blockNextFsmEventTransmition; }
            set { blockNextFsmEventTransmition = value; }
        }

        public readonly static ManualLogSource logSource = new ManualLogSource("logSource");
        public readonly static Vector3 defaultReplacedParentLocation = new Vector3(-250, -250);

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
            harmony.PatchAll(typeof(DoPickupAction));
            harmony.PatchAll(typeof(EndInteraction));

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

            // PlayerData Patches
            harmony.PatchAll(typeof(BellCentipedeWaiting_Get));
            harmony.PatchAll(typeof(BellCentipedeLocked_Get));

            // FsmUtility Patches
            harmony.PatchAll(typeof(SendEventUpwards));

            // FullQuestBase Patches
            harmony.PatchAll(typeof(RewardIcon_Get));

            // QuestBoardInteractable Patches
            harmony.PatchAll(typeof(ProcessQueuedCompletions));


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
            AwakePatchEvent += HandleBeastlingCall;
            AwakePatchEvent += HandleElegyOfTheDeep;

            AwakePatchEvent += HandleFirstSinnerPersistenceAndPickup;
            AwakePatchEvent += HandleFirstSinnerInMemory;

            AwakePatchEvent += HandlePhantom;

            AwakePatchEvent += HandleArchitectMelody;
            AwakePatchEvent += HandleConductorMelody;
            AwakePatchEvent += HandleLibrarianMelody;
            AwakePatchEvent += HandleThreefoldSongLift;

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

            AwakePatchEvent += HandleBellHermit;
            AwakePatchEvent += HandleChurchkeeper;

            AwakePatchEvent += HandleCoralHeart;

            AwakePatchEvent += HandleKeyOfHeretic;

            AwakePatchEvent += HandleWoodWitch;
            AwakePatchEvent += HandleDoctorFly;
            AwakePatchEvent += HandleSteelSpines;

            AwakePatchEvent += HandleBellway;
            AwakePatchEvent += HandleVentrica;

            AwakePatchEvent += HandleCraftPickup;


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
    }
}
