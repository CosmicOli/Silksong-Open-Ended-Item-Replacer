using BepInEx;
using BepInEx.Logging;
using GenericVariableExtension;
using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using InControl;
using QuestPlaymakerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamCherry.Localization;
using TeamCherry.SharedUtils;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using static AchievementPopup;
using static CollectableItem;
using static FullQuestBase;
using static GameManager;
using static GamepadVibrationMixer.GamepadVibrationEmission;
using static HutongGames.EasingFunction;
using static HutongGames.PlayMaker.FsmEventTarget;
using static PlayerDataTest;
using static tk2dSpriteCollectionDefinition;
using static UnityEngine.UI.Image;
using static UnityEngine.UI.Selectable;

namespace Open_Ended_Item_Replacer
{
    public class UniqueID
    {
        public string PickupName
        {
            get;
            private set;
        }

        public string SceneName
        {
            get;
            private set;
        }

        [Obsolete]
        public UniqueID(string PickupName, string SceneName)
        {
            this.PickupName = PickupName;
            this.SceneName = SceneName;
        }

        public UniqueID(GameObject replacedObject, string replacedItemName)
        {
            // Defining the unique id for the new pickup
            this.PickupName = replacedObject.name + "-" + replacedItemName;
            this.SceneName = GameManager.GetBaseSceneName(replacedObject.scene.name);
        }
    }

    public class TestAction : FsmStateAction
    {
        public override void OnEnter()
        {
            Open_Ended_Item_Replacer.logSource.LogWarning("action ran");
            Open_Ended_Item_Replacer.logSource.LogInfo(this.State.Name);

            Active = false;
            Finished = true;
            Finish();
        }
    }

    // Progressive start and end are inclusive
    public class GetReplacedProgressiveLevel : FsmStateAction
    {
        int progressiveStart;
        int progressiveEnd;
        string[] gameObjectNames;
        string progressiveItemName;
        FsmInt storeResult;

        public GetReplacedProgressiveLevel(int progressiveStart, int progressiveEnd, string gameObjectName, string progressiveItemName, FsmInt storeResult)
        {
            gameObjectNames = new string[progressiveEnd - progressiveStart + 1];
            for (int i = 0; i < gameObjectNames.Length; i++) { gameObjectNames[i]  = gameObjectName; }

            this.progressiveStart = progressiveStart;
            this.progressiveEnd = progressiveEnd;
            this.progressiveItemName = progressiveItemName;
            this.storeResult = storeResult;
        }

        public GetReplacedProgressiveLevel(int progressiveStart, int progressiveEnd, string[] gameObjectNames, string progressiveItemName, FsmInt storeResult)
        {
            this.progressiveStart = progressiveStart;
            this.progressiveEnd = progressiveEnd;
            this.gameObjectNames = gameObjectNames;
            this.progressiveItemName = progressiveItemName;
            this.storeResult = storeResult;
        }

        public override void OnEnter()
        {
            GenericSavedItem needleUpgradeItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            PersistentItemData<bool> needleUpgradePersistentBoolData;

            for (int i = progressiveStart; i <= progressiveEnd; i++)
            {
                needleUpgradeItem.name = progressiveItemName + " " + i.ToString();
                needleUpgradePersistentBoolData = Open_Ended_Item_Replacer.GeneratePersistentBoolData_SameScene(gameObjectNames[i - progressiveStart], needleUpgradeItem.name);

                if (!Open_Ended_Item_Replacer.GetPersistentBoolFromData(needleUpgradePersistentBoolData))
                {
                    storeResult.Value = i - 1; // Minus 1 as the previous i will be the last "true" bool
                    break;
                }
            }
        }
    }

    public class GetPersistentBoolUsingPersistentItemBool : FsmStateAction
    {
        PersistentItemData<bool> persistence;
        FsmBool storeResult;

        public GetPersistentBoolUsingPersistentItemBool(PersistentItemData<bool> persistence, FsmBool storeResult)
        {
            this.persistence = persistence;
            this.storeResult = storeResult;
        }

        public override void OnEnter()
        {
            storeResult = Open_Ended_Item_Replacer.GetPersistentBoolFromData(persistence);
        }
    }

    public class SetFsmActiveState : FsmStateAction
    {
        bool[] cachedEnabled;
        bool revert = false;

        Fsm fsm;
        FsmState oldState;
        FsmState newState;
        Func<bool> comparisonFirstHalf;
        Func<bool> comparisonSecondHalf;
        bool blockRemainingActions = false;

        public SetFsmActiveState(Fsm fsm, FsmState oldState, FsmState newState, bool blockRemainingActions = true)
        {
            bool getTrue() { return true; }

            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.comparisonFirstHalf = getTrue;
            this.comparisonSecondHalf = getTrue;
            this.blockRemainingActions = blockRemainingActions;
        }

        public SetFsmActiveState(Fsm fsm, FsmState newState)
        {
            bool getTrue() { return true; }

            this.fsm = fsm;
            this.newState = newState;
            this.comparisonFirstHalf = getTrue;
            this.comparisonSecondHalf = getTrue;
        }

        public SetFsmActiveState(Fsm fsm, FsmState newState, Func<bool> comparisonFirstHalf, Func<bool> comparisonSecondHalf)
        {
            this.fsm = fsm;
            this.newState = newState;
            this.comparisonFirstHalf = comparisonFirstHalf;
            this.comparisonSecondHalf = comparisonSecondHalf;
        }

        public SetFsmActiveState(Fsm fsm, FsmState oldState, FsmState newState, Func<bool> comparisonFirstHalf, Func<bool> comparisonSecondHalf, bool blockRemainingActions = true)
        {
            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.comparisonFirstHalf = comparisonFirstHalf;
            this.comparisonSecondHalf = comparisonSecondHalf;
            this.blockRemainingActions = blockRemainingActions;
        }

        private void HandleBlockRemainingActions()
        {
            if (revert)
            {
                FsmStateAction[] dummyArray = new FsmStateAction[cachedEnabled.Length];
                Array.Copy(oldState.Actions, dummyArray, dummyArray.Length);
                oldState.Actions = dummyArray;

                for (int i = 0; i < cachedEnabled.Length; i++)
                {
                    oldState.Actions[i].Enabled = cachedEnabled[i];
                }

                revert = false;

                Active = false;
                Finished = true;
                Finish();
            }
            else
            {
                if (comparisonFirstHalf.Invoke() == comparisonSecondHalf.Invoke())
                {
                    int length = oldState.Actions.Length;
                    cachedEnabled = new bool[length];
                    for (int i = 0; i < length; i++)
                    {
                        cachedEnabled[i] = oldState.Actions[i].Enabled;
                    }

                    FsmStateAction[] cachedActions = new FsmStateAction[length];
                    Array.Copy(oldState.Actions, cachedActions, length);
                    oldState.Actions = new FsmStateAction[length + 1];
                    Array.Copy(cachedActions, oldState.Actions, length);
                    revert = true;

                    for (int i = 0; i < length; i++)
                    {
                        oldState.Actions[i].Enabled = false;
                    }

                    Enabled = true;
                    oldState.Actions[length] = this;

                    Traverse.Create(fsm).Field("activeState").SetValue(newState);
                    Traverse.Create(fsm).Field("activeStateName").SetValue(newState.Name);
                    fsm.Start();
                }
                Finish();
            }
        }

        public override void OnEnter()
        {
            if (blockRemainingActions)
            {
                HandleBlockRemainingActions();
                return;
            }

            if (comparisonFirstHalf.Invoke() == comparisonSecondHalf.Invoke())
            {
                fsm.SwitchState(newState);

                //Traverse.Create(fsm).Field("activeState").SetValue(newState);
                //Traverse.Create(fsm).Field("activeStateName").SetValue(newState.Name);
                //fsm.Start();
            }

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class RemoveExtraSilkHeart : FsmStateAction
    {
        public override void OnEnter()
        {
            HeroController.instance.AddToMaxSilkRegen(-1);

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class GetCheck : FsmStateAction
    {
        GenericSavedItem genericItem;

        public GetCheck(GameObject gameObject, string itemName)
        {
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            genericItem.persistentBoolItem = Open_Ended_Item_Replacer.GeneratePersistentBoolSetToItem(gameObject, itemName, genericItem);
        }

        public override void OnEnter()
        {
            // Handles persistence set by new item
            if (!Open_Ended_Item_Replacer.GetPersistentBoolFromData(genericItem.persistentBoolItem.ItemData))
            {
                genericItem.Get();
            }

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class ReplacePickup : FsmStateAction
    {
        GameObject gameObject;
        string itemName;

        public ReplacePickup(GameObject gameObject, string itemName)
        {
            this.gameObject = gameObject;
        }

        public override void OnEnter()
        {
            Open_Ended_Item_Replacer.Replace(gameObject, itemName, true, null);

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class AllowPickup : FsmStateAction
    {
        CollectableItemPickup pickup;

        public AllowPickup(CollectableItemPickup pickup)
        {
            this.pickup = pickup;
        }

        public override void OnEnter()
        {
            pickup.transform.GetComponent<Collider2D>().enabled = true;
            Traverse.Create(pickup).Field("canPickupTime").SetValue((double) 0);
            Traverse.Create(pickup).Field("canPickupDelay").SetValue(Traverse.Create(Gameplay.CollectableItemPickupPrefab).Field("canPickupDelay").GetValue<float>());

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class SetContainerPersistence : FsmStateAction
    {
        PersistentItemData<bool> persistentBoolData;

        public SetContainerPersistence(PersistentItemData<bool> persistentBoolData)
        {
            this.persistentBoolData = persistentBoolData;
        }

        public override void OnEnter()
        {
            persistentBoolData.Value = true;
            SceneData.instance.PersistentBools.SetValue(persistentBoolData);

            Active = false;
            Finished = true;
            Finish();
        }
    }

    // This object defines the item that replaces intended items
    // TODO: 
    // -> Show popup
    // -> Make functions make open ended requests
    public class GenericSavedItem : SavedItem
    {
        private UniqueID uniqueID;
        public UniqueID UniqueID
        {
            get 
            { 
                return uniqueID; 
            }

            set
            {
                uniqueID = value;
                name = "Generic_Item-" + uniqueID.PickupName + "-" + uniqueID.SceneName;
            }
        }

        public PersistentBoolItem persistentBoolItem;

        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

            // Show popup (if showPopup)
            // Send get request
            persistentBoolItem.ItemData.Value = true;
            SceneData.instance.PersistentBools.SetValue(persistentBoolItem.ItemData);
            logSource.LogInfo("Item get:  " + persistentBoolItem.ItemData.ID + "  In Scene: " + persistentBoolItem.ItemData.SceneName);
        }

        public override bool CanGetMore()
        {
            return true;
        }

        public override Sprite GetPopupIcon()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.LogException(new NotImplementedException());
            }

            return null;
        }

        public override string GetPopupName()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.LogException(new NotImplementedException());
            }

            return null;
        }
    }

    // I'm sure I'll update the version number at some point, right?
    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        public static ManualLogSource logSource = new ManualLogSource("logSource");

        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(logSource);
            Logger.LogInfo("Plugin loaded and initualised.");

            Harmony harmony = Harmony.CreateAndPatchAll(typeof(Open_Ended_Item_Replacer), null);

            //MethodInfo DoMsgOriginal = typeof(UIMsgBase<>).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name == "DoMsg");
            //harmony.Patch(DoMsgOriginal, prefix: new HarmonyMethod(typeof(Open_Ended_Item_Replacer).GetMethod("UIMsgBase_DoMsgPrefix", BindingFlags.Static | BindingFlags.NonPublic)));*/
        }

        static Func<bool> GetTrueFunc() 
        {
            bool GetTrue() { return true; }
            return GetTrue; 
        }

        static Func<bool> GetFalseFunc()
        {
            bool GetFalse() { return false; }
            return GetFalse; 
        }

        public static bool GetPersistentBoolFromData(PersistentItemData<bool> persistentBoolData)
        {
            return SceneData.instance.PersistentBools.GetValueOrDefault(persistentBoolData.SceneName, persistentBoolData.ID);
        }

        public static bool GetPersistentBoolFromData(string sceneName, string persistentID)
        {
            return SceneData.instance.PersistentBools.GetValueOrDefault(sceneName, persistentID);
        }

        static Func<bool> GetPersistentBoolFromDataFunc(PersistentItemData<bool> persistentData)
        {
            bool GetBool() { return GetPersistentBoolFromData(persistentData); }

            return GetBool;
        }

        static Func<bool> GetPlayerDataBoolFunc(string playerDataBool)
        {
            bool GetBool() 
            {
                if (!VariableExtensions.VariableExists<bool, PlayerData>(playerDataBool))
                {
                    return false;
                }

                return GameManager.instance.GetPlayerDataBool(playerDataBool); 
            }

            return GetBool;
        }

        public static PersistentBoolItem GeneratePersistentBoolSetToItem_SameScene(string gameObjectName, string originalItemName, GenericSavedItem replacementItem)
        {
            GameObject gameObject = new GameObject(gameObjectName);
            return GeneratePersistentBoolSetToItem(gameObject, originalItemName, replacementItem);
        }

        public static PersistentBoolItem GeneratePersistentBoolSetToItem(GameObject gameObject, string originalItemName, GenericSavedItem replacementItem)
        {
            UniqueID uniqueID = new UniqueID(gameObject, originalItemName);
            replacementItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
            SetGenericPersistentInfo(uniqueID, persistent);

            return persistent;
        }

        public static PersistentItemData<bool> GeneratePersistentBoolData_SameScene(string gameObjectName, string originalItemName)
        {
            GameObject gameObject = new GameObject(gameObjectName);
            return GeneratePersistentBoolData(gameObject, originalItemName);
        }

        public static PersistentItemData<bool> GeneratePersistentBoolData(GameObject gameObject, string originalItemName)
        {
            UniqueID uniqueID = new UniqueID(gameObject, originalItemName);
            PersistentItemData<bool> persistentBoolData = new PersistentItemData<bool>();
            SetGenericPersistentBoolDataInfo(uniqueID, persistentBoolData);

            return persistentBoolData;
        }

        private static FsmStateAction[] ReturnCombinedActions(FsmStateAction[] preActions, FsmStateAction[] postActions)
        {
            FsmStateAction[] replacementActions = new FsmStateAction[preActions.Length + postActions.Length];

            Array.Copy(preActions, 0, replacementActions, 0, preActions.Length);
            Array.Copy(postActions, 0, replacementActions, preActions.Length, postActions.Length);

            return replacementActions;
        }

        // This is used for getting familiar with what objects are in rooms with checks; will be removed later
        // It is a simple depth first search on all root game objects, putting all that are found in a list I output the contents of easily
        private List<GameObject> startingGameObjects = new List<GameObject>();
        public static List<GameObject> FindAllObjectsInCurrentScene()
        {
            List<GameObject> FindChildren(Transform parentTransform)
            {
                int childCount = parentTransform.childCount;

                List<GameObject> decendantsList = new List<GameObject>();
                for (int i = 0; i < childCount; i++)
                {
                    Transform currentChild = parentTransform.GetChild(i);

                    decendantsList.Add(currentChild.gameObject);
                    decendantsList.Concat(FindChildren(currentChild));
                }

                return decendantsList;
            }

            Scene CurrentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = CurrentScene.GetRootGameObjects();

            List<GameObject> gameObjects = new List<GameObject>();

            foreach (GameObject rootObject in rootObjects)
            {
                gameObjects.Add(rootObject);
                gameObjects.Concat(FindChildren(rootObject.transform));
            }

            return gameObjects;
        }

        private static void LevelActivatedDebugging()
        {
            GameManager GameManager = GameManager.instance;
            HeroController heroControl = GameManager.hero_ctrl;

            Scene CurrentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = CurrentScene.GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                logSource.LogInfo(rootObject.name);
            }

            List<GameObject> gameObjectsInCurrentScene = FindAllObjectsInCurrentScene();

            foreach (GameObject gameObject in gameObjectsInCurrentScene)
            {
                //logSource.LogInfo(gameObject.transform.name);

                if (false)//gameObject.transform.name == "Flea Rescue Branches" || gameObject.transform.name == "Flea Rescue Cage" || gameObject.transform.name == "Flea Rescue Barrel")
                {
                    logSource.LogWarning(gameObject.transform.name);

                    /*UnityEngine.Component[] components = gameObject.GetComponents<UnityEngine.Component>();
                    foreach (UnityEngine.Component component in components)
                    {
                        logSource.LogInfo(component);
                    }*/

                    PlayMakerFSM fsm = gameObject.GetComponent<PlayMakerFSM>();

                    //logSource.LogWarning(fsm.FsmName);
                    FsmState[] states = fsm.FsmStates;

                    List<FsmEvent> events = FsmEvent.EventList;

                    FsmVariables variables = fsm.FsmVariables;

                    NamedVariable[] named = variables.GetAllNamedVariables();

                    foreach (NamedVariable namedVar in named)
                    {
                        //logSource.LogInfo(namedVar.Name);
                    }

                    //FsmGameObject fleaFsmGameObject = variables.GetFsmGameObject("Flea");
                    //GameObject flea = fleaFsmGameObject.Value;

                    NamedVariable QuestTracker = variables.GetVariable("Quest Tracker");
                    //logSource.LogInfo(QuestTracker.ObjectType);

                    /*UnityEngine.Component[] components = (variables.GetFsmObject("Quest Tracker").Value as QuestTargetPlayerDataBools).Get;
                    foreach (UnityEngine.Component component in components)
                    {
                        logSource.LogInfo(component);
                    }*/



                    /*foreach (FsmEvent Event in events)
                    {
                        logSource.LogWarning(Event.Name);
                        logSource.LogInfo(Event.Path);
                        logSource.LogInfo(Event.ToString());
                    }*/

                    /*foreach (FsmState state in states)
                    {
                        logSource.LogInfo(state.Name);
                        if (state.Name == "Activate Flea")
                        {
                            FsmStateAction[] test = state.Actions;

                            foreach (FsmStateAction action in test)
                            {
                                logSource.LogInfo(action.DisplayName);
                                action.Event()
                            }
                        }
                    }*/
                }

                /*if (gameObject.transform.name == "Heart Piece")
                {
                    logSource.LogWarning("Heart Piece");

                    UnityEngine.Component[] components = gameObject.GetComponents<UnityEngine.Component>();
                    foreach (UnityEngine.Component component in components)
                    {
                        logSource.LogInfo(component);
                    }
                }

                if (gameObject.transform.name == "Silk Spool")
                {
                    logSource.LogWarning("Silk Spool");

                    UnityEngine.Component[] components = gameObject.GetComponents<UnityEngine.Component>();
                    foreach (UnityEngine.Component component in components)
                    {
                        logSource.LogInfo(component);
                    }
                }*/
            }
        }

        private static bool hasHarpoonDash;
        private static bool hasNeedolin;
        private static bool hasDash;
        private static bool hasBrolly;
        private static bool hasWalljump;
        private static bool hasDoubleJump;
        private static bool hasSuperJump;

        static bool hasGranted;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "LevelActivated")]
        public static void GameManager_LevelActivated_Postfix(GameManager __instance)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            logSource.LogMessage("Level Activated: " + sceneName);

            bool debugging = false;
            if (debugging)
            {
                LevelActivatedDebugging();
            }

            // Stops softlocking in memories
            PlayerData playerData = PlayerData.instance;
            if (!hasGranted)
            {
                if (sceneName.ToLower().Contains("memory") && (sceneName.ToLower().Contains("silk_heart") || sceneName.ToLower().Contains("needolin") || sceneName.ToLower().Contains("first_sinner")))
                {
                    hasGranted = true;

                    hasHarpoonDash = playerData.hasHarpoonDash;
                    hasNeedolin = playerData.hasNeedolin;
                    hasDash = playerData.hasDash;
                    hasBrolly = playerData.hasBrolly;
                    hasWalljump = playerData.hasWalljump;
                    hasDoubleJump = playerData.hasDoubleJump;
                    hasSuperJump = playerData.hasSuperJump;

                    playerData.hasHarpoonDash = true;
                    playerData.hasNeedolin = true;
                    playerData.hasDash = true;
                    playerData.hasBrolly = true;
                    playerData.hasWalljump = true;
                    playerData.hasDoubleJump = true;
                    playerData.hasSuperJump = true;
                }
            }
            else
            {
                if (!sceneName.ToLower().Contains("memory"))
                {
                    hasGranted = false;

                    playerData.hasHarpoonDash = hasHarpoonDash;
                    playerData.hasNeedolin = hasNeedolin;
                    playerData.hasDash = hasDash;
                    playerData.hasBrolly = hasBrolly;
                    playerData.hasWalljump = hasWalljump;
                    playerData.hasDoubleJump = hasDoubleJump;
                    playerData.hasSuperJump = hasSuperJump;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "TimePasses")]
        public static void GameManager_TimePasses_Postfix(GameManager __instance)
        {
            PlayerData playerData = __instance.playerData;
            string sceneNameString = __instance.GetSceneNameString();

            if (sceneNameString != "Room_Pinstress")
            {
                if (playerData.blackThreadWorld)
                {
                    logSource.LogInfo("time passed");

                    if (GetPersistentBoolFromData("Room_Pinstress", GeneratePersistentBoolData_SameScene("Needle Strike", "Needle Strike").ID))
                    {
                        playerData.pinstressQuestReady = true;
                    }
                    else
                    {
                        playerData.pinstressQuestReady = false;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "TryTestLoad")]
        private static bool SceneAdditiveLoadConditional_TryTestLoad_Prefix(SceneAdditiveLoadConditional __instance, PlayerDataTest ___tests, QuestTest[] ___questTests, ref bool __result)
        {
            if (!Traverse.Create(__instance).Field("sceneNameToLoad").GetValue<string>().ToLower().Contains("bone_east_08")) { return true; }

            foreach (TestGroup testGroup in ___tests.TestGroups)
            {
                for (int i = 0; i < testGroup.Tests.Length; i++)
                {
                    logSource.LogInfo(testGroup.Tests[i].FieldName);

                    if (testGroup.Tests[i].FieldName == "hasBrolly")
                    {

                        if (QuestManager.GetQuest("Brolly Get").IsCompleted)
                        {
                            if (PlayerData.instance.defeatedSongGolem)
                            {
                                __result = false;
                            }
                            else
                            {
                                __result = true;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private static Transform testTransform;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NailSlash), "StartSlash")]
        private static void NailSlash_StartSlash_Postfix(NailSlash __instance)
        {
            //logSource.LogMessage(PlayerData.instance.HasMelodyArchitect);
            //logSource.LogMessage(PlayerData.instance.HasMelodyConductor);
            //logSource.LogMessage(PlayerData.instance.HasMelodyLibrarian);

            //logSource.LogMessage(testTransform.position);
            //logSource.LogMessage(testTransform.gameObject.activeSelf);

            /*var quests = QuestManager.GetAllQuests();

            foreach (var quest in quests)
            {
                logSource.LogMessage(quest.DisplayName);
            }*/

            //logSource.LogMessage("Slash Postfix");

            //HeroController heroControl = GameManager.instance.hero_ctrl;

            //logSource.LogInfo(test.GetComponent<Rigidbody2D>().gravityScale);
            //logSource.LogInfo(testAction.gravityScale);

            /*UniqueID uniqueID = new UniqueID("pickupName", "sceneName");
            spawningReplacementCollectableItemPickup = true;
            SpawnGenericInteractablePickup(uniqueID, null, heroControl.transform, new Vector3(0, 0, 0));
            spawningReplacementCollectableItemPickup = false;*/
        }

        public static string replacementFlag = "_(Replacement)";
        
        // Deactivates and replaces a given object
        public static Transform Replace(GameObject replacedObject, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab)
        {
            // Sets up the replacement object to not be replaced itself
            spawningReplacementCollectableItemPickup = true;

            // Removes the original object
            replacedObject.SetActive(false);

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
                output = SpawnGenericInteractablePickup(uniqueID, replacementPrefab, replacedObject.transform, new Vector3(0, 0, 0));
            }
            else
            {
                output = SpawnGenericCollisionPickup(uniqueID, replacementPrefab, replacedObject.transform, new Vector3(0, 0, 0));
            }
            logSource.LogInfo("Pickup Drop Attempt End");

            spawningReplacementCollectableItemPickup = false;

            return output;
        }

        public static void ReplaceGiantFleaPickup(Transform giantFlea, PlayMakerFSM giantFleaFSM, PlayMakerFSM __instance, GameObject fleaObject)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(fleaObject, genericFleaItemName, genericItem);

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
            if (__instance.ItemData.ID.ToLower().StartsWith("heart piece"))
            {
                //logSource.LogInfo("Heart Piece");
                Replace(__instance.gameObject, "Heart Piece", false, null);
            }

            if (__instance.ItemData.ID.ToLower().StartsWith("silk spool"))
            {
                //logSource.LogInfo("Silk Spool");
                Replace(__instance.gameObject, "Silk Spool", false, null);
            }
        }

        private static CheckQuestPdSceneBool SearchForCheckQuestPdSceneBool(FsmState state, string questTargetName)
        {
            List<CheckQuestPdSceneBool> actions = state?.Actions?.OfType<CheckQuestPdSceneBool>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                QuestTargetPlayerDataBools questTarget = (action.QuestTarget?.RawValue as QuestTargetPlayerDataBools);
                if (questTarget == null) { continue; }

                if (questTarget.name.Contains(genericFleaItemName))
                {
                    return action;
                }
            }

            return null;
        }

        private static PlayerDataBoolTest SearchForPlayerDataBoolTest(FsmState state, string boolName)
        {
            List<PlayerDataBoolTest> actions = state?.Actions?.OfType<PlayerDataBoolTest>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                FsmString fsmBoolName = action?.boolName;
                if (fsmBoolName == null) { continue; }

                if (fsmBoolName.Value == boolName)
                {
                    return action;
                }
            }

            return null;
        }

        private static PlayerDataVariableTest SearchForPlayerDataVariableTest(FsmState state, string variableName)
        {
            List<PlayerDataVariableTest> actions = state?.Actions?.OfType<PlayerDataVariableTest>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                FsmString fsmVariableName = action?.VariableName;
                if (fsmVariableName == null) { continue; }

                if (fsmVariableName.Value == variableName)
                {
                    return action;
                }
            }

            return null;
        }

        private static void HandleCheckQuestPdSceneBoolFlea(CheckQuestPdSceneBool genericPersistenceChecker, PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (genericPersistenceChecker == null) { return; }

            string stateName = genericPersistenceChecker.State.Name;

            logSource.LogInfo("Generic flea flagged " + stateName);
            genericPersistenceChecker.trueEvent = new FsmEvent("");

            GameObject gameObject = __instance.gameObject;

            switch (stateName)
            {
                case "Init":
                case "Check State":
                case "Idle":
                    NamedVariable[] fsmGameObjects = __instance.FsmVariables.GetNamedVariables(VariableType.GameObject);

                    bool containsFleaSprite = false;
                    // Replaces containers that look like fleas
                    if (gameObject.GetComponent<tk2dSpriteAnimator>())
                    {
                        foreach (NamedVariable variable in fsmGameObjects)
                        {
                            if (variable.Name.ToLower().Contains("flea") && variable.Name.ToLower().Contains("sprite"))
                            {
                                containsFleaSprite = true;
                            }

                            if (!containsFleaSprite)
                            {
                                fleaObject.transform.position = __instance.transform.position;
                                Replace(fleaObject, genericFleaItemName, false, null);
                                return;
                            }
                        }
                    }

                    PersistentItemData<bool> persistentBoolData = GeneratePersistentBoolData(gameObject, genericFleaItemName);

                    Transform replacmentTransform;

                    // Replace any contained fleas
                    FsmGameObject fleaFsmGameObject = __instance.FsmVariables.GetFsmGameObject("Flea");
                    if (fleaFsmGameObject.Value == null)
                    {
                        fleaObject.transform.position = __instance.transform.position;
                        __instance.gameObject.SetActive(false);
                        replacmentTransform = Replace(fleaObject, genericFleaItemName, true, null);
                    }
                    else
                    {
                        fleaObject.transform.position = fleaFsmGameObject.Value.transform.position;
                        fleaFsmGameObject.Value.SetActive(false);
                        replacmentTransform = Replace(fleaObject, genericFleaItemName, true, null);
                    }

                    // Checks if anything enables the flea we want disabled, and then removes the ability to enable it
                    // Also checks for any BREAK transitions
                    PlayMakerFSM[] parentFSMs = gameObject.GetComponents<PlayMakerFSM>();
                    foreach (PlayMakerFSM parentFSM in parentFSMs)
                    {
                        FsmState[] parentFsmStates = parentFSM.FsmStates;
                        foreach (FsmState parentFsmState in parentFsmStates)
                        {
                            FsmStateAction[] parentFsmStateActions = parentFsmState.Actions;
                            foreach (FsmStateAction parentFsmStateAction in parentFsmStateActions)
                            {
                                ActivateGameObject activateGameObject = parentFsmStateAction as ActivateGameObject;
                                if (activateGameObject != null)
                                {
                                    string associatedGameObjectName = activateGameObject.gameObject?.GameObject?.Name;
                                    if (associatedGameObjectName.ToLower().Contains("flea"))
                                    {
                                        parentFsmStateAction.Enabled = false;
                                    }
                                }
                            }

                            // Any state with transition named break should add an action to the next state at the beginning that reenables gravity
                            int numberOfNewActions = 3;
                            FsmTransition[] transitions = parentFsmState.Transitions;
                            foreach (FsmTransition transition in transitions)
                            {
                                if (transition.EventName == "BREAK")
                                {
                                    FsmState nextState = parentFSM.Fsm.GetState(transition.ToState);

                                    FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                                    SetGravity2dScaleV2 setGravity2dScaleV2 = new SetGravity2dScaleV2();
                                    setGravity2dScaleV2.gravityScale = replacmentTransform.GetComponent<Rigidbody2D>().gravityScale;

                                    setGravity2dScaleV2.everyFrame = false;

                                    setGravity2dScaleV2.gameObject = new FsmOwnerDefault();
                                    setGravity2dScaleV2.gameObject.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                                    setGravity2dScaleV2.gameObject.GameObject = replacmentTransform.gameObject;

                                    newActions[0] = new SetContainerPersistence(persistentBoolData);
                                    newActions[1] = setGravity2dScaleV2;
                                    newActions[2] = new AllowPickup(replacmentTransform.GetComponent<CollectableItemPickup>());

                                    //Array.Copy(nextState.Actions, 0, newActions, numberOfNewActions, nextState.Actions.Length);
                                    nextState.Actions = ReturnCombinedActions(newActions, nextState.Actions);

                                    //nextState.Actions = newActions;
                                }
                            }
                        }
                    }

                    // Handles persistence for the container
                    if (GetPersistentBoolFromData(persistentBoolData))
                    {
                        gameObject.SetActive(false);
                        logSource.LogInfo("Container set inactive");
                    }
                    else
                    {
                        // Should only drop and be interactable when container broken
                        replacmentTransform.GetComponent<Collider2D>().enabled = false;
                        replacmentTransform.GetComponent<Rigidbody2D>().gravityScale = 0;
                        Traverse.Create(replacmentTransform.GetComponent<CollectableItemPickup>()).Field("canPickupTime").SetValue(double.PositiveInfinity);
                        Traverse.Create(replacmentTransform.GetComponent<CollectableItemPickup>()).Field("canPickupDelay").SetValue(float.PositiveInfinity);
                    }

                    break;

                case "Sleeping":
                    // Sleeping fleas have to be on a floor, so they will be interactable
                    fleaObject.transform.position = __instance.transform.position;
                    __instance.gameObject.SetActive(false);
                    Replace(fleaObject, genericFleaItemName, true, null);
                    break;

                default:
                    logSource.LogError("State handled with incorrect name");
                    break;
            }
        }

        private static void HandleKrattFlea(PlayerDataBoolTest krattPersistenceChecker, PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (krattPersistenceChecker == null) { return; }

            logSource.LogInfo("Kratt flagged");
            krattPersistenceChecker.isTrue = new FsmEvent("");

            fleaObject.transform.position = __instance.transform.position;
            __instance.gameObject.SetActive(false);
            Replace(fleaObject, genericFleaItemName, true, null);
        }

        private static void HandleGiantFlea(PlayerDataBoolTest giantFleaPersistenceChecker, PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (giantFleaPersistenceChecker == null) { return; }

            logSource.LogInfo("Giant flea flagged");
            giantFleaPersistenceChecker.isTrue = new FsmEvent("");

            Transform giantFlea = (__instance.FsmVariables.GetVariable("Parent").RawValue as GameObject).transform.Find("Giant Flea");
            fleaObject.transform.position = giantFlea.position;

            PlayMakerFSM[] giantFleaFSMs = giantFlea.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM giantFleaFSM in giantFleaFSMs)
            {
                if (giantFleaFSM.FsmName == "Control")
                {
                    ReplaceGiantFleaPickup(giantFlea, giantFleaFSM, __instance, fleaObject);
                }
            }
        }

        private static void HandleVogFlea(PlayerDataVariableTest vogPersistenceChecker, PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (vogPersistenceChecker == null) { return; }

            logSource.LogInfo("Vog flagged");
            vogPersistenceChecker.IsExpectedEvent = new FsmEvent("TRUE");

            fleaObject.transform.position = __instance.transform.position;
            __instance.gameObject.SetActive(false);
            Replace(fleaObject, genericFleaItemName, true, null);
        }

        private static void HandleFrozenFlea(PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.gameObject.name.Contains("Snowflake Chunk - Flea") && __instance.name.Contains("Control"))
            {
                logSource.LogInfo("Frozen flea flagged");

                fleaObject.transform.position = __instance.transform.position;
                __instance.gameObject.SetActive(false);
                Replace(fleaObject, genericFleaItemName, false, null);
            }
        }

        private static void HandleAspidFlea(PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.gameObject.name.Contains("Aspid Collector"))
            {
                bool hasBerry = false;
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    if (__instance.transform.GetChild(i).name.Contains("Mossberry Pickup"))
                    {
                        hasBerry = true;
                    }
                }

                if (__instance.FsmVariables.GetFsmBool("Flea Carrier").Value && !hasBerry)
                {
                    logSource.LogInfo("Aspid flea flagged");

                    fleaObject.transform.position = __instance.transform.position;
                    __instance.gameObject.SetActive(false);
                    Replace(fleaObject, genericFleaItemName, false, null);
                }
            }
        }

        // Handles anything that is or contains a flea
        private static string genericFleaItemName = "FleasCollected Target";
        private static void HandleFlea(PlayMakerFSM __instance)
        {
            FsmState initState = __instance.Fsm.GetState("Init");
            FsmState checkState = __instance.Fsm.GetState("Check State");
            FsmState sleepState = __instance.Fsm.GetState("Sleeping");
            FsmState idleState = __instance.Fsm.GetState("Idle");

            // Fleas are unique to scenes even if they sometimes can be in different locations in a scene, so it's worth only distinguishing fleas by scene name
            // By unique to scenes, I really mean it; as far as I remember seeing, the fleas set the playerdata bool associated with them using the scene name
            GameObject fleaObject = new GameObject("Flea Object");

            // Should flag every flea except
            // -> Giant flea
            // -> Vog
            // -> Kratt
            // -> That one aspid flea
            // -> That one frozen flea
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(initState, genericFleaItemName), __instance, fleaObject); // Misc fleas
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(checkState, genericFleaItemName), __instance, fleaObject); // Like bellhart and karak flea
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(sleepState, genericFleaItemName), __instance, fleaObject); // Seepy fleas -> gameObject can be replaced with no restrictions
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(idleState, genericFleaItemName), __instance, fleaObject); // Fancy citadel cage fleas and slab cell flea

            // Specifically for Kratt
            HandleKrattFlea(SearchForPlayerDataBoolTest(initState, "CaravanLechSaved"), __instance, fleaObject);

            // Specifically for giant flea
            HandleGiantFlea(SearchForPlayerDataBoolTest(idleState, "tamedGiantFlea"), __instance, fleaObject);
            
            // Specifically for Vog
            FsmState stillHereState = __instance.Fsm.GetState("Still Here?");
            HandleVogFlea(SearchForPlayerDataVariableTest(stillHereState, "MetTroupeHunterWild"), __instance, fleaObject);

            // Specifically for frozen flea
            HandleFrozenFlea(__instance, fleaObject);

            // Specifically for aspid flea
            HandleAspidFlea(__instance, fleaObject);
        }

        private static void HandleWeaverStatue(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Inspection" && __instance.gameObject?.name == "Shrine Weaver Ability")
            {
                Fsm fsm = __instance.Fsm;

                // Removes original persistence checking
                FsmState collectedCheckState = fsm.GetState("Collected Check");
                (collectedCheckState.Actions[0] as PlayerDataBoolTest).isTrue = new FsmEvent("");

                Replace(__instance.gameObject, fsm.Variables.GetFsmEnum("Ability").Value.ToString(), true, null);
            }
        }

        private static void HandleCrest(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Crest Get Shrine")
            {
                Fsm fsm = __instance.Fsm;

                // Removes original persistence checking
                FsmState checkUnlockedState = fsm.GetState("Check Unlocked");
                (checkUnlockedState.Actions[0] as GetIsCrestUnlocked).trueEvent = new FsmEvent("");

                Replace(__instance.gameObject, fsm.Variables.GetFsmEnum("Crest Type").Value.ToString(), true, null);
            }
        }

        private static void HandleCrestDoor(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "chapel_door_control" && __instance.gameObject?.name == "Chapel Door Control")
            {
                Fsm fsm = __instance.Fsm;

                // Removes original persistence checking
                FsmState stateCheckState = fsm.GetState("State Check");

                // Any state with transition named break should add an action to the next state at the beginning that reenables gravity
                int numberOfNewActions = 1;
                FsmTransition[] transitions = stateCheckState.Transitions;
                foreach (FsmTransition transition in transitions)
                {
                    if (transition.EventName == "CLOSED" || transition.EventName == "DO CLOSE")
                    {
                        FsmState nextState = stateCheckState.Fsm.GetState(transition.ToState);

                        FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                        newActions[0] = new ReplacePickup(__instance.gameObject, fsm.Variables.GetFsmEnum("Crest Type").Value.ToString());

                        //Array.Copy(nextState.Actions, 0, newActions, numberOfNewActions, nextState.Actions.Length);
                        nextState.Actions = ReturnCombinedActions(newActions, nextState.Actions);

                        //nextState.Actions = newActions;
                    }
                }
            }
        }

        private static void HandleSilkNeedle(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Silk Needle Spell Get")
            {
                // Removes original persistence checking
                FsmState checkUnlockedState = __instance.Fsm.GetState("Check Unlocked");
                (checkUnlockedState.Actions[0] as GetIsCrestUnlocked).trueEvent = new FsmEvent("");

                Replace(__instance.gameObject, __instance.Fsm.Variables.GetFsmEnum("Silk Needle").Value.ToString(), true, null);
            }
        }

        private static void HandleSilkHeart(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Silk Heart Memory Return" && __instance.gameObject?.name == "Silk Heart Memory Return")
            {
                FsmState save = __instance.Fsm.GetState("Save");

                int numberOfNewActions = 2;

                FsmStateAction[] newActions = new FsmStateAction[save.Actions.Length + numberOfNewActions];

                GameObject dummyGameObject = new GameObject("Silk Heart");
                newActions[0] = new GetCheck(dummyGameObject, "Silk Heart"); // Replace
                newActions[1] = new RemoveExtraSilkHeart();

                //Array.Copy(save.Actions, 0, newActions, numberOfNewActions, save.Actions.Length);
                save.Actions = ReturnCombinedActions(newActions, save.Actions);

                //save.Actions = newActions;
            }
        }

        private static void HandleNeedolinPreMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Spinner Boss")
            {
                FsmState finalBindBurst = __instance.Fsm.GetState("Final Bind Burst");
                FsmState getNeedolin = __instance.Fsm.GetState("Get Needolin");
                if (finalBindBurst == null || getNeedolin == null) { return; }

                finalBindBurst.Actions[3].Enabled = false; // disables giving needolin
                getNeedolin.Actions[1].Enabled = false; // disables needolin message
            }
        }

        private static void HandleNeedolinInMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Control" && __instance.gameObject?.name == "Memory Control")
            {
                if (__instance.Fsm.GetState("Get Rune Bomb") != null) { return; } // Don't want to trigger on first sinner

                FsmState needolinPrompt = __instance.Fsm.GetState("Needolin Prompt");
                FsmState endScene = __instance.Fsm.GetState("End Scene");
                if (needolinPrompt == null || endScene == null) { return; }

                needolinPrompt.Actions[1].Enabled = false; // disables giving needolin
                endScene.Actions[0].Enabled = false; // disables giving needolin

                int numberOfNewActions = 1;

                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                GameObject dummyGameObject = new GameObject("Needolin");
                newActions[0] = new GetCheck(dummyGameObject, "Needolin"); // Replace

                //Array.Copy(endScene.Actions, 0, newActions, numberOfNewActions, endScene.Actions.Length);
                endScene.Actions = ReturnCombinedActions(newActions, endScene.Actions);

                //endScene.Actions = newActions;
            }
        }

        private static void HandleFirstSinnerPersistenceAndPickup(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Inspection" && __instance.gameObject?.name == "Shrine First Weaver")
            {
                FsmState init = __instance.Fsm.GetState("Init");
                if (init == null) { return; }

                (init.Actions[0] as PlayerDataBoolTest).isTrue = new FsmEvent(""); // disables checking for rune bomb

                // Handles persistence set by new item
                GameObject dummyGameObject = new GameObject("Rune Bomb");
                UniqueID uniqueID = new UniqueID(dummyGameObject, "Rune Bomb");
                if (GetPersistentBoolFromData("Memory_First_Sinner", uniqueID.PickupName + replacementFlag))
                {
                    __instance.gameObject.SetActive(false);
                }
            }
        }

        private static void HandleFirstSinnerInMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Control" && __instance.gameObject?.name == "Memory Control")
            {
                FsmState getRuneBomb = __instance.Fsm.GetState("Get Rune Bomb");
                if (getRuneBomb == null) { return; }

                FsmState needolinPrompt = __instance.Fsm.GetState("Needolin Prompt");
                needolinPrompt.Actions[1].Enabled = false; // disables giving needolin
                needolinPrompt.Actions[2].Enabled = false; // disables setting having beaten widow to true

                getRuneBomb.Actions[0].Enabled = false; // disables owning rune bomb
                getRuneBomb.Actions[1].Enabled = false; // disables auto equipping rune bomb
                getRuneBomb.Actions[2].Enabled = false; // disables displaying rune bomb

                int numberOfNewActions = 1;

                FsmStateAction[] newActions = new FsmStateAction[getRuneBomb.Actions.Length + numberOfNewActions];

                GameObject dummyGameObject = new GameObject("Rune Rage");
                newActions[0] = new GetCheck(dummyGameObject, "Rune Rage"); // Replace

                //Array.Copy(getRuneBomb.Actions, 0, newActions, numberOfNewActions, getRuneBomb.Actions.Length);
                getRuneBomb.Actions = ReturnCombinedActions(newActions, getRuneBomb.Actions);

                //getRuneBomb.Actions = newActions;
            }
        }

        private static void HandlePhantom(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Phantom")
            {
                FsmState UIMsg = __instance.Fsm.GetState("UI Msg");
                FsmState setData = __instance.Fsm.GetState("Set Data");
                if (UIMsg == null || setData == null) { return; }

                UIMsg.Actions[1].Enabled = false; // disables giving parry
                UIMsg.Actions[5].Enabled = false; // disables auto equipping parry
                UIMsg.Actions[6].Enabled = false; // disables displaying parry

                setData.Actions[0].Enabled = false; // disables giving parry

                FsmStateAction[] newActionsPre = new FsmStateAction[1];

                GameObject dummyGameObject = new GameObject("Parry");
                newActionsPre[0] = new GetCheck(dummyGameObject, "Parry"); // Replace

                /*FsmOwnerDefault ownerDefault = new FsmOwnerDefault();
                ownerDefault.GameObject = __instance.gameObject;
                ownerDefault.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                FsmEventTarget eventTarget = new FsmEventTarget();
                eventTarget.target = EventTarget.BroadcastAll;
                eventTarget.excludeSelf = false;
                eventTarget.gameObject = ownerDefault;
                eventTarget.fsmName = __instance.Fsm.Name;
                eventTarget.sendToChildren = true;
                eventTarget.fsmComponent = __instance;

                SendEventByName msgFadeOut = new SendEventByName();
                msgFadeOut.eventTarget = eventTarget;
                msgFadeOut.sendEvent = "SKILL GET MSG FADED OUT";
                msgFadeOut.delay = 0;

                SendEventByName msgEnd = new SendEventByName();
                msgEnd.eventTarget = eventTarget;
                msgEnd.sendEvent = "SKILL GET MSG ENDED";
                msgFadeOut.delay = 0;*/

                //Array.Copy(UIMsg.Actions, 0, newActions, numberOfNewActions - 1, UIMsg.Actions.Length);

                UIMsg.Actions = ReturnCombinedActions(newActionsPre, UIMsg.Actions);

                FsmStateAction[] newActionsPost = new FsmStateAction[1];
                //newActionsPost[0] = new SetFsmActiveState(__instance.Fsm, UIMsg, __instance.Fsm.GetState("End Pause"), false);
                newActionsPost[0] = new SetFsmActiveState(__instance.Fsm, __instance.Fsm.GetState("End Pause")); // Replaces original persistence check with custom

                UIMsg.Actions = ReturnCombinedActions(UIMsg.Actions, newActionsPost);

                //UIMsg.Actions = newActions;
            }
        }

        private static void HandleArchitectMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Cylinder States" && __instance.gameObject?.name == "puzzle cylinders")
            {
                FsmState waitForNotify = __instance.Fsm.GetState("Wait For Notify");
                FsmState startLock = __instance.Fsm.GetState("Start Lock");
                FsmState hasMelody = __instance.Fsm.GetState("Has Melody");
                if (startLock == null || waitForNotify == null || hasMelody == null) { return; }

                waitForNotify.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasMelody, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("puzzle cylinders", "Citadel Ascent Melody Architect")), GetTrueFunc()); // Replaces original persistence check with custom

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, startLock, waitForNotify, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc()); // Disables allowing getting the song part without needolin

                //Array.Copy(startLock.Actions, 0, newActions, numberOfNewActions, startLock.Actions.Length);
                startLock.Actions = ReturnCombinedActions(newActions, startLock.Actions);

                //startLock.Actions = newActions;
            }
        }

        private static void HandleConductorMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Last Conductor NPC")
            {
                FsmState hasItem = __instance.Fsm.GetState("Has Item?");
                FsmState repeatDlg = __instance.Fsm.GetState("Repeat Dlg");
                FsmState questActive = __instance.Fsm.GetState("Quest Active?");
                FsmState melodyNoQuest = __instance.Fsm.GetState("Melody NoQuest");

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, questActive, melodyNoQuest, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc()); // Disables allowing getting the song part without needolin
                questActive.Actions = ReturnCombinedActions(newActions, questActive.Actions);

                hasItem.Actions[8] = new SetFsmActiveState(__instance.Fsm, hasItem, repeatDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Last Conductor NPC", "Citadel Ascent Melody Conductor")), GetTrueFunc());
            }
        }

        private static void HandleLibrarianMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Librarian")
            {
                FsmState openRelicBoard = __instance.Fsm.GetState("Open Relic Board");
                if (openRelicBoard == null) { return; }

                FsmState needolinPreWait = __instance.Fsm.GetState("Needolin Pre Wait");
                FsmState dlgEnd = __instance.Fsm.GetState("Dlg End");
                FsmStateAction[] newActions = new FsmStateAction[2];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, needolinPreWait, dlgEnd, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc());

                newActions[1] = new SetFsmActiveState(__instance.Fsm, needolinPreWait, dlgEnd, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Librarian", "Citadel Ascent Melody Librarian Return")), GetTrueFunc());

                needolinPreWait.Actions = ReturnCombinedActions(newActions, needolinPreWait.Actions);

                if (PlayerData.instance.HasMelodyLibrarian)
                {
                    (openRelicBoard.Actions[2] as ShowRelicBoard).ClosedEvent = FsmEvent.GetFsmEvent("MELODY CYLINDER PLAYED");
                }
            }
        }

        private static void HandlePlinney(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Plinney Inside")
            {
                FsmState upgradeState = __instance.Fsm.GetState("Upgrade State");
                FsmState setUpgrade1 = __instance.Fsm.GetState("Set Upgrade 1"); 
                FsmState setUpgrade2 = __instance.Fsm.GetState("Set Upgrade 2");
                FsmState furtherUpgrades = __instance.Fsm.GetState("Further Upgrades");
                FsmState completeRepeat = __instance.Fsm.GetState(" Complete Repeat"); // I kid you not, this state has a space at the beginning of its name, and yes I did have to spend time debugging to discover this lmao
                if (upgradeState == null || setUpgrade1 == null || setUpgrade2 == null || furtherUpgrades == null || completeRepeat == null) { return; }

                FsmInt storeValue = (upgradeState.Actions[0] as GetPlayerDataInt).storeValue; // Gets the variable responsible for tracking current needle upgrade

                upgradeState.Actions = new FsmStateAction[5];
                upgradeState.Actions[0] = new GetReplacedProgressiveLevel(1, 4, __instance.Fsm.Owner.name, "Needle Upgrade", storeValue); // Sets the variable responsible for tracking current needle upgrade by checking the progressive persistent bools; necessary for price and dialogue functionality
                upgradeState.Actions[1] = new SetFsmActiveState(__instance.Fsm, upgradeState, setUpgrade1, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 1")), GetFalseFunc()); // Upgrade 1
                upgradeState.Actions[2] = new SetFsmActiveState(__instance.Fsm, upgradeState, setUpgrade2, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 2")), GetFalseFunc()); // Upgrade 2
                upgradeState.Actions[3] = new SetFsmActiveState(__instance.Fsm, upgradeState, furtherUpgrades, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 4")), GetFalseFunc()); // Upgrade 3 and 4
                upgradeState.Actions[4] = new SetFsmActiveState(__instance.Fsm, upgradeState, completeRepeat, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 4")), GetTrueFunc()); // All upgrades
            }
        }

        private static void HandleSeamstress(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Seamstress")
            {
                FsmState msg = __instance.Fsm.GetState("Msg");
                if (msg == null) { return; }

                GameObject dummyGameObject = new GameObject("Drifter's Cloak");
                msg.Actions[3] = new GetCheck(dummyGameObject, "Drifter's Cloak");
            }
        }

        private static void HandleFourthChorus(PlayMakerFSM __instance)
        {
            // Fourth Chorus; "Control" "Boss Scene" "Init 18 and 19" -> Will need a more identifying part of this fsm to avoid triggering for other bosses

            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Boss Scene")
            {
                FsmState activateReturnBombRock = __instance.Fsm.GetState("Activate Return Bomb Rock"); // Hopefully a unique enough state name to uniquely identify fourth chorus
                FsmState init = __instance.Fsm.GetState("Init");
                if (activateReturnBombRock == null || init == null) { return; }

                init.Actions[18] = init.Actions[19]; // Moves checking whether encountered Fourth Chorus prior to checking the quest

                CheckQuestState checkBrollyGetQuest = new CheckQuestState();
                checkBrollyGetQuest.Quest = QuestManager.GetQuest("Brolly Get");
                checkBrollyGetQuest.NotTrackedEvent = FsmEvent.GetFsmEvent("");
                checkBrollyGetQuest.TrackedEvent = FsmEvent.GetFsmEvent("");
                checkBrollyGetQuest.CompletedEvent = FsmEvent.GetFsmEvent("MEET READY");

                init.Actions[19] = checkBrollyGetQuest;
            }
        }

        private static void HandlePinstress(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "States" && __instance.gameObject?.name == "Pinstress States")
            {
                FsmState check = __instance.Fsm.GetState("Check");
                FsmState ground = __instance.Fsm.GetState("Ground");
                if (check == null || ground == null) { return; }

                check.Actions[0] = new SetFsmActiveState(__instance.Fsm, check, ground, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Needle Strike", "Needle Strike")), GetFalseFunc());
            }

            if (__instance.Fsm.Name == "Behaviour" && __instance.gameObject?.name == "Pinstress Interior Ground Sit")
            {
                FsmState save = __instance.Fsm.GetState("Save");
                FsmState met = __instance.Fsm.GetState("Met?");
                FsmState reofferDlg = __instance.Fsm.GetState("Reoffer Dlg");
                if (save == null || met == null) { return; }

                met.Actions[4] = new SetFsmActiveState(__instance.Fsm, met, reofferDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Needle Strike", "Needle Strike")), GetFalseFunc());

                GameObject dummyGameObject = new GameObject("Needle Strike");
                save.Actions[2] = new GetCheck(dummyGameObject, "Needle Strike");
            }
        }

        private static void HandleFaydownCloak(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "DJ Get Sequence" && __instance.gameObject?.name == "DJ Get Sequence")
            {
                FsmState hasDJ = __instance.Fsm.GetState("Has DJ?");
                FsmState startBlizzardAudio = __instance.Fsm.GetState("Start Blizzard Audio");
                FsmState completed = __instance.Fsm.GetState("Completed");
                FsmState breakTuningFork = __instance.Fsm.GetState("Break Tuning Fork");

                if (hasDJ == null || startBlizzardAudio == null || completed == null || breakTuningFork == null) { return; }

                hasDJ.Actions = new FsmStateAction[2];
                hasDJ.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasDJ, startBlizzardAudio, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Faydown Cloak", "Faydown Cloak")), GetFalseFunc());
                hasDJ.Actions[1] = new SetFsmActiveState(__instance.Fsm, hasDJ, completed, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Faydown Cloak", "Faydown Cloak")), GetTrueFunc());

                GameObject dummyGameObject = new GameObject("Faydown Cloak");
                breakTuningFork.Actions[3] = new GetCheck(dummyGameObject, "Faydown Cloak");
            }
        }

        private static void HandleEva(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Crest Upgrade Shrine")
            {
                FsmState checkCombo1 = __instance.Fsm.GetState("Check Combo 1");
                FsmState checkSlot1 = __instance.Fsm.GetState("Check Slot1");
                FsmState checkSlot2 = __instance.Fsm.GetState("Check Slot2");
                FsmState checkHunterv3 = __instance.Fsm.GetState("Check Hunter v3");
                FsmState checkFinalUpgrade = __instance.Fsm.GetState("Check Final Upgrade");
                FsmState showedPrompt = __instance.Fsm.GetState("Showed Prompt?");

                FsmState unlockCrestUpg1 = __instance.Fsm.GetState("Unlock Crest Upg 1");
                FsmState unlockFirstSlot = __instance.Fsm.GetState("Unlock First Slot");
                FsmState unlockOtherSlot = __instance.Fsm.GetState("Unlock Other Slot");
                FsmState unlockCrestUpg2 = __instance.Fsm.GetState("Unlock Crest Upg 2");
                FsmState setBound = __instance.Fsm.GetState("Set Bound");

                FsmState crestChangeAntic = __instance.Fsm.GetState("Crest Change Antic");
                FsmState crestChange = __instance.Fsm.GetState("Crest Change");
                FsmState crestChangeEnd = __instance.Fsm.GetState("Crest Change End");
                FsmState firstUpgDlg = __instance.Fsm.GetState("First Upg Dlg");
                FsmState upgradeSequence5 = __instance.Fsm.GetState("Upgrade Sequence 5");

                FsmState wasUpgraded = __instance.Fsm.GetState("Was Upgraded?");
                FsmState offerDlg = __instance.Fsm.GetState("Offer Dlg");

                FsmState init = __instance.Fsm.GetState("Init");
                FsmState endDialogue = __instance.Fsm.GetState("End Dialogue");
                FsmState breakState = __instance.Fsm.GetState("Break");
                FsmState broken = __instance.Fsm.GetState("Broken");

                if (checkCombo1 == null || checkSlot1 == null || checkSlot2 == null || checkHunterv3 == null || checkFinalUpgrade == null || showedPrompt == null) { return; }

                PersistentItemData<bool> persistentBoolDataHunter_v2 = GeneratePersistentBoolData_SameScene("Hunter_v2", "Hunter_v2");
                PersistentItemData<bool> persistentBoolDataHunter_v3 = GeneratePersistentBoolData_SameScene("Hunter_v3", "Hunter_v3");
                PersistentItemData<bool> persistentBoolDataYellowSlot = GeneratePersistentBoolData_SameScene("Yellow Slot", "Yellow Slot");
                PersistentItemData<bool> persistentBoolDataBlueSlot = GeneratePersistentBoolData_SameScene("Blue Slot", "Blue Slot");
                PersistentItemData<bool> persistentBoolDataSylphsong = GeneratePersistentBoolData_SameScene("Sylphsong", "Sylphsong");

                // The following handles replacing the majority of persistence checks

                // my one thought is that checkCombo1 is being run and not properly reset, causing it to hang on second entry from denying and reentering the dialogue
                checkCombo1.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkCombo1, checkSlot1, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v2), GetTrueFunc()); // hunter 2
                checkCombo1.Actions[1].Enabled = false;

                checkSlot1.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkSlot1, checkSlot2, GetPersistentBoolFromDataFunc(persistentBoolDataYellowSlot), GetTrueFunc()); // yellow slot
                checkSlot1.Actions[1].Enabled = false;
                checkSlot1.Actions[2].Enabled = false;

                checkSlot2.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkSlot2, checkHunterv3, GetPersistentBoolFromDataFunc(persistentBoolDataBlueSlot), GetTrueFunc()); // blue slot
                checkSlot2.Actions[1].Enabled = false;
                checkSlot2.Actions[2].Enabled = false;
             
                checkHunterv3.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkHunterv3, checkFinalUpgrade, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v3), GetTrueFunc()); // hunter 3

                checkFinalUpgrade.Actions[0].Enabled = false;
                checkFinalUpgrade.Actions[1] = new SetFsmActiveState(__instance.Fsm, checkFinalUpgrade, showedPrompt, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc()); // bound eva

                // The following handle replacing getting the upgrades

                bool CheckCurrentCrestPoints()
                {
                    if (__instance.Fsm.GetFsmInt("Current Crest Points").Value >= 12)
                    {
                        __instance.Fsm.GetFsmBool("Stay In Sequence").Value = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                GameObject hunter_v2GameObject = new GameObject("Hunter_v2");
                unlockCrestUpg1.Actions[2].Enabled = false;
                unlockCrestUpg1.Actions[3].Enabled = false;
                firstUpgDlg.Actions = ReturnCombinedActions(new FsmStateAction[] { new GetCheck(hunter_v2GameObject, "Hunter_v2"), new SetFsmActiveState(__instance.Fsm, firstUpgDlg, checkSlot1, CheckCurrentCrestPoints, GetTrueFunc()) }, firstUpgDlg.Actions);

                GameObject yellowSlot_GameObject = new GameObject("Yellow Slot");
                unlockFirstSlot.Actions[5] = new GetCheck(yellowSlot_GameObject, "Yellow Slot");

                GameObject blueSlot_GameObject = new GameObject("Blue Slot");
                unlockOtherSlot.Actions[2] = new SetIntValue();
                (unlockOtherSlot.Actions[2] as SetIntValue).intVariable = __instance.Fsm.GetFsmInt("Slot Index");
                (unlockOtherSlot.Actions[2] as SetIntValue).intValue = 1;
                unlockOtherSlot.Actions[4] = new GetCheck(blueSlot_GameObject, "Blue Slot");

                GameObject hunter_v3GameObject = new GameObject("Hunter_v3");
                unlockCrestUpg2.Actions[2] = new GetCheck(hunter_v3GameObject, "Hunter_v3");

                GameObject sylphsong_GameObject = new GameObject("Sylphsong");
                setBound.Actions[2] = new GetCheck(sylphsong_GameObject, "Sylphsong");

                // The following handles removing the incorrect animations for hunter crest upgrades

                unlockCrestUpg1.Actions = ReturnCombinedActions(unlockCrestUpg1.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, crestChangeAntic) });
                unlockCrestUpg2.Actions = ReturnCombinedActions(unlockCrestUpg2.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, crestChangeAntic) });

                crestChangeAntic.Actions[1].Enabled = false; // Play animation
                crestChangeAntic.Actions[7].Enabled = false; // Wait
                crestChangeAntic.Actions[9].Enabled = false; // Wait
                crestChangeAntic.Actions[10].Enabled = false; // Send event
                crestChangeAntic.Actions[11].Enabled = false; // Wait

                crestChange.Actions[1].Enabled = false; // Auto equip crest
                crestChange.Actions[2].Enabled = false; // Send event
                crestChange.Actions[3].Enabled = false; // Wait

                crestChangeEnd.Actions[1].Enabled = false; // Play animation
                crestChangeEnd.Actions[3] = new SetFsmActiveState(__instance.Fsm, firstUpgDlg, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v2), GetFalseFunc());
                //crestChangeEnd.Actions = ReturnCombinedActions(crestChangeEnd.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkFinalUpgrade, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v3), GetTrueFunc()) });

                // The following handles removing the incorrect animations for slot upgrades

                unlockFirstSlot.Actions[6].Enabled = false;
                unlockFirstSlot.Actions[7].Enabled = false;
                unlockFirstSlot.Actions[8].Enabled = false;
                unlockFirstSlot.Actions = ReturnCombinedActions(unlockFirstSlot.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkSlot2) }); // Added to the end instead of replacing to make it clear 6-8 are disabled

                unlockOtherSlot.Actions[5].Enabled = false;
                unlockOtherSlot.Actions[6].Enabled = false;
                unlockOtherSlot.Actions[7].Enabled = false;
                unlockOtherSlot.Actions = ReturnCombinedActions(unlockOtherSlot.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkHunterv3) }); // Added to the end instead of replacing to make it clear 5-7 are disabled

                // The following handles removing the incorrect animations and persistence for Sylphsong

                init.Actions[0].Enabled = false;
                init.Actions = ReturnCombinedActions(init.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, broken, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc()) });
                setBound.Actions[0].Enabled = false;
                endDialogue.Actions[2] = endDialogue.Actions[3];
                endDialogue.Actions[3] = new SetFsmActiveState(__instance.Fsm, broken, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc());

                // The following handles incorrect dialogue and other misc fixes
                
                (offerDlg.Actions[2] as RunDialogue).Key = "CREST_UPGRADE_ALL"; // Makes sure the correct dialogue plays
                (wasUpgraded.Actions[2] as GetIsCrestUnlocked).falseEvent = FsmEvent.GetFsmEvent("CONVO_END"); // Ensures skipping dialogue always

                // As a note here, by adding dummy code to a test action for testing, I don't think that its some pc specific async issue based on processing time or some bs like that
                showedPrompt.Actions[7].Enabled = false; // For some reason, the Wait here completely stops working and the state moves on before anything past it happens, so I disable it
                showedPrompt.Actions[8].Enabled = false; // The animation also causes issues, which I suspect relates to the wait?
            }
        }

        // Handles FSM checks
        // All fleas have SavedItems that are gotten at the end of their fsms
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerFSM_Awake_Postfix(PlayMakerFSM __instance)
        {
            HandleFlea(__instance);

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

        /*HarmonyPostfix]
        [HarmonyPatch(typeof(Fsm), "Start")]
        private static void PlayMakerFSM_AwakePostfix(Fsm __instance)
        {
            logSource.LogInfo(__instance.Name);
        }*/

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
            if (__instance.Item.Value.name.Contains(genericFleaItemName) && __instance.Item.Name.Contains("Generic_Item-"))
            {
                return true;
            }

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
            ReplaceFsmToolGet(__instance);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SetToolLocked
        // Stops NPCs locking tools when not actually replacing them
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetToolLocked), "OnEnter")]
        private static bool SetToolLocked_OnEnter_Prefix(SetToolLocked __instance)
        {
            __instance.Finish();
            return false;
        }

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
        private static void CreateObject_OnEnter_Prefix(CreateObject __instance)
        {
            if (__instance.gameObject.Value == null) { return; }

            string loweredName = __instance.gameObject.Value.name.ToLower();

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
            }

            /*if (loweredName.Contains("ui msg crest evolve"))
            {
                PlayMakerFSM playMakerFsm = __instance.gameObject?.Value?.transform?.GetComponent<PlayMakerFSM>();
                if (playMakerFsm == null) { return; }

                HandleUiMsgCrestEvolve(playMakerFsm);
            }*/
        }

        // Replaces CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        // I have somewhat arbitrarily picked OnEnable over awake here as I am hoping that if there are pickups that start disabled they aren't replaced until they are enabled
        private static bool spawningReplacementCollectableItemPickup = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void CollectableItemPickup_OnEnable_Postfix(CollectableItemPickup __instance) //, PersistentBoolItem ___persistent)
        {
            logSource.LogMessage("CollectableItemPickup Enabled");

            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!spawningReplacementCollectableItemPickup)
            {
                // Using Harmony's traverse tool, the private field "persistent" can be copied
                // Persistance tracks data about pickups independantly to the item they contain, so this needs to be preserved to allow tracking of what pickups have been interacted with
                //PersistentBoolItem replacedPersistent = ___persistent;
                
                // Traverse.Create(__instance).Field("persistent").GetValue<PersistentBoolItem>();

                /*if (__instance.Item.name.Contains("Common Spine")) // will generalise a check for active later
                {
                    return;
                }*/

                if (__instance.Item == null) { return; }
                if (__instance.gameObject == null) { return; }

                Replace(__instance.gameObject, __instance.Item.name, true, null);
            }
        }

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
            Vector3 vector = spawnPoint.TransformPoint(offset);
            Vector3 position = vector;

            CollectableItemPickup collectableItemPickup;

            // Creates the new pickup and sets its position
            collectableItemPickup = Instantiate(prefab);
            collectableItemPickup.transform.position = position;
            collectableItemPickup.gameObject.name = uniqueID.PickupName + replacementFlag;

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
            Vector3 vector = spawnPoint.TransformPoint(offset);
            Vector3 position = vector;

            CollectableItemPickup collectableItemPickup;

            // Creates the new pickup and sets its position
            collectableItemPickup = Instantiate(prefab);
            collectableItemPickup.transform.position = position;
            collectableItemPickup.gameObject.name = uniqueID.PickupName + replacementFlag;
            collectableItemPickup.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;

            SetGenericPickupInfo(uniqueID, collectableItemPickup);

            return collectableItemPickup.transform;
        }

        private static void SetGenericPickupInfo(UniqueID uniqueID, CollectableItemPickup collectableItemPickup)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            // This logs where the pickup has been placed
            logSource.LogInfo("New Pickup Placed At: " + collectableItemPickup.transform.position);

            PersistentBoolItem persistent = Traverse.Create(collectableItemPickup).Field("persistent").GetValue<PersistentBoolItem>();

            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;

            // Sets the item granted upon pickup
            collectableItemPickup.SetItem(genericItem, true);
            logSource.LogInfo("Pickup Item Set: " + genericItem.name);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(persistent.ItemData))
            {
                collectableItemPickup.gameObject.SetActive(false); // Kinda not needed as disablePrefabIfActivated exists
                logSource.LogInfo("Replacement set inactive");
            }
        }

        public static void SetGenericPersistentInfo(UniqueID uniqueID, PersistentBoolItem persistent)
        {
            // Makes sure that persistent has loaded and that hasSetup = true
            persistent.LoadIfNeverStarted();
            persistent.ItemData.ToString();

            // Sets persistent data
            SetGenericPersistentBoolDataInfo(uniqueID, persistent.ItemData);
        }

        public static void SetGenericPersistentBoolDataInfo(UniqueID uniqueID, PersistentItemData<bool> persistentBoolData)
        {
            // Sets persistent data
            persistentBoolData.ID = uniqueID.PickupName + replacementFlag;
            persistentBoolData.SceneName = uniqueID.SceneName;
            persistentBoolData.IsSemiPersistent = false;
            persistentBoolData.Value = false;
            persistentBoolData.Mutator = SceneData.PersistentMutatorTypes.None;
        }
    }
}
