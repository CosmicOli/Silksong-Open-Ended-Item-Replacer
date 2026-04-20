using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers;
using Open_Ended_Item_Replacer.Silksong.Containers.Flea_Containers;
using Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemCollect_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.CustomSceneManager_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.FSMUtility_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.FullQuestBase_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.GradeMarker_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.NailSlash_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.PlayerData_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.QuestBoardInteractable_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.SavedItemGetDelayed_Patches;
using Open_Ended_Item_Replacer.Silksong.Patches.SceneAdditiveLoadConditional_Patches;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.BellwayHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.BrollyAndAssociatedHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.CraftPickupHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.CreigeHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.CrestHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.CrullAndBenjinHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.CurseHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.EvaHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FaydownHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FirstSinnerHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FleaCaravanHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.HeartsHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.KeyOfHereticHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.LugoliHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.MossDruidHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.NeedolinHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.NuuHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.PhantomHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.PinstressHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.PlinneyHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.SilkAndSoulHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.SilkHeartHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.SilkNeedleHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.SurfaceMementoHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.ThreefoldSongHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.VentricaHandler;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.WeaverStatueHandler;
using static Open_Ended_Item_Replacer.Silksong.Patches.PlayMakerFSM_Patches.Awake;


namespace Open_Ended_Item_Replacer
{
    // I'm sure I'll update the version number at some point, right?
    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        public static GameObject HeartPieceInstant;
        public static GameObject Flea_Barrel;

        public static CollectableItemPickup_Container DefaultInteractableContainer
        {
            get
            {
                return CollectableItemPickup_Container.Prefab;
            }
        }

        public static CollectableItemPickupInstant_Container DefaultCollisionContainer
        {
            get
            {
                return CollectableItemPickupInstant_Container.Prefab;
            }
        }
        public static Costed_CollectableItemPickup_Container DefaultCostedContainer
        {
            get
            {
                return Costed_CollectableItemPickup_Container.Prefab;
            }
        }

        public static Flea_Barrel_Container BarrelFleaContainer
        {
            get
            {
                return Flea_Barrel_Container.Prefab;
            }
        }

        private static bool spawningReplacement = false;
        public static bool SpawningReplacement
        {
            get { return spawningReplacement; }
            set { spawningReplacement = value; }
        }

        public static bool loadGameRunPatched = true;
        public static bool LoadGameRunPatched
        {
            get { return loadGameRunPatched; }
            set { loadGameRunPatched = value; }
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
            harmony.PatchAll(typeof(StartNewGame));
            harmony.PatchAll(typeof(ContinueGame));

            // SceneAdditiveLoadConditional Patches
            harmony.PatchAll(typeof(TryTestLoad));

            // NailSlash Patches
            harmony.PatchAll(typeof(StartSlash));

            // PlayMakerFsm Patches
            harmony.PatchAll(typeof(Silksong.Patches.PlayMakerFSM_Patches.Awake));

            // PersistentBoolItem Patches
            harmony.PatchAll(typeof(Silksong.Patches.PersistentBoolItem_Patches.Awake));

            // CollectableItemPickup Patches
            harmony.PatchAll(typeof(Silksong.Patches.CollectableItemPickup_Patches.Awake));
            harmony.PatchAll(typeof(CheckActivation));
            harmony.PatchAll(typeof(DoPickupAction));
            harmony.PatchAll(typeof(EndInteraction));

            // CountCrestUnlockPoints Patches
            harmony.PatchAll(typeof(Silksong.Patches.CountCrestUnlockPoints_Patches.OnEnter));

            // CollectableItemCollect Patches
            harmony.PatchAll(typeof(DoAction));

            // SavedItemGet V1-2 Patches
            harmony.PatchAll(typeof(Silksong.Patches.SavedItemGet_V1_2_Patches.OnEnter));

            // SavedItemGetDelayed Patches
            harmony.PatchAll(typeof(DoGet));

            // CreateUIMsgGetItem Patches
            harmony.PatchAll(typeof(Silksong.Patches.CreateUIMsgGetItem_Patches.OnEnter));

            // SpawnObjectFromGlobalPool Patches
            harmony.PatchAll(typeof(Silksong.Patches.SpawnObjectFromGlobalPool_Patches.OnEnter));

            // CreateObject Patches
            harmony.PatchAll(typeof(Silksong.Patches.CreateObject_Patches.OnEnter));

            // SetToolUnlocked Patches
            harmony.PatchAll(typeof(Silksong.Patches.SetToolUnlocked_Patches.OnEnter));

            // SetToolLocked Patches
            harmony.PatchAll(typeof(Silksong.Patches.SetToolLocked_Patches.OnEnter));

            // PlayerData Patches
            harmony.PatchAll(typeof(BellCentipedeWaiting_Get));
            harmony.PatchAll(typeof(BellCentipedeLocked_Get));

            // FsmUtility Patches
            harmony.PatchAll(typeof(SendEventUpwards));

            // FullQuestBase Patches
            harmony.PatchAll(typeof(RewardIcon_Get));

            // QuestBoardInteractable Patches
            harmony.PatchAll(typeof(ProcessQueuedCompletions));

            // CustomSceneManager Patches
            harmony.PatchAll(typeof(Silksong.Patches.CustomSceneManager_Patches.Start));
            harmony.PatchAll(typeof(Silksong.Patches.CustomSceneManager_Patches.Update));

            // GradeMarker Patches
            harmony.PatchAll(typeof(Silksong.Patches.GradeMarker_Patches.Start));
            harmony.PatchAll(typeof(Silksong.Patches.GradeMarker_Patches.Update));


            // If this were c# 9.0, I would shove this responsibility onto the handler classes themselves with a ModuleInitializer, but alas
            // NOTE: Some of these exist within the same handler but are still seperated in case another mod wished to override a distinct function
            AwakePatchEvent += Handle_Flea;

            AwakePatchEvent += Handle_WeaverStatue;

            AwakePatchEvent += Handle_Crest;
            AwakePatchEvent += Handle_CrestDoor;

            AwakePatchEvent += Handle_SilkNeedle;

            AwakePatchEvent += Handle_SilkHeart;

            AwakePatchEvent += Handle_NeedolinPreMemory;
            AwakePatchEvent += Handle_NeedolinInMemory;
            AwakePatchEvent += Handle_BeastlingCall;
            AwakePatchEvent += Handle_ElegyOfTheDeep;

            AwakePatchEvent += Handle_FirstSinnerPersistenceAndPickup;
            AwakePatchEvent += Handle_FirstSinnerInMemory;

            AwakePatchEvent += Handle_Phantom;

            AwakePatchEvent += Handle_ArchitectMelody;
            AwakePatchEvent += Handle_ConductorMelody;
            AwakePatchEvent += Handle_LibrarianMelody;
            AwakePatchEvent += Handle_ThreefoldSongLift;

            AwakePatchEvent += Handle_Plinney;

            AwakePatchEvent += Handle_Seamstress;
            AwakePatchEvent += Handle_FourthChorus;

            AwakePatchEvent += Handle_Pinstress;

            AwakePatchEvent += Handle_FaydownCloak;

            AwakePatchEvent += Handle_Eva;

            AwakePatchEvent += Handle_Nuu;

            AwakePatchEvent += Handle_Grishkin;
            AwakePatchEvent += Handle_FleaCharm;
            AwakePatchEvent += Handle_SethMemento;

            AwakePatchEvent += Handle_Chef;

            AwakePatchEvent += Handle_Nectar;

            AwakePatchEvent += Handle_MossDruid;

            AwakePatchEvent += Handle_SurfaceMemento;

            //AwakePatchEvent += Handle_BellHermit; // Replaced with being handled in LevelActivated
            AwakePatchEvent += Handle_Churchkeeper;

            AwakePatchEvent += Handle_CoralHeart;

            AwakePatchEvent += Handle_KeyOfHeretic;

            AwakePatchEvent += Handle_WoodWitch;
            AwakePatchEvent += Handle_DoctorFly;
            AwakePatchEvent += Handle_SteelSpines;

            AwakePatchEvent += Handle_Bellway;
            AwakePatchEvent += Handle_Ventrica;

            AwakePatchEvent += Handle_CraftPickup;


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
