using BepInEx;
using BepInEx.Logging;
using GenericVariableExtension;
using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamCherry.Localization;
using TeamCherry.SharedUtils;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AchievementPopup;
using static CollectableItem;
using static FullQuestBase;
using static GamepadVibrationMixer.GamepadVibrationEmission;
using static HutongGames.EasingFunction;
using static HutongGames.PlayMaker.FsmEventTarget;
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

            Active = false;
            Finished = true;
            Finish();
        }
    }

    public class SetFsmActiveState : FsmStateAction
    {
        bool[] cachedEnabled;
        bool revert = false;

        Fsm fsm; 
        FsmState oldState;
        FsmState newState;

        public SetFsmActiveState(Fsm fsm, FsmState oldState, FsmState newState)
        {
            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
        }

        public override void OnEnter()
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
        }
    }

    public class SetFsmStateOnPlayerDataBool : FsmStateAction
    {
        bool[] cachedEnabled;
        bool revert = false;

        Fsm fsm;
        FsmState oldState;
        FsmState newState;
        string playerDataBool;
        bool expected;

        public SetFsmStateOnPlayerDataBool(Fsm fsm, FsmState oldState, FsmState newState, string playerDataBool, bool expected)
        {
            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.playerDataBool = playerDataBool;
            this.expected = expected;
        }

        public override void OnEnter()
        {
            //Open_Ended_Item_Replacer.logSource.LogWarning("Action entered: setFsmStateOnPlayerDataBool");

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
                if (VariableExtensions.VariableExists<bool, PlayerData>(playerDataBool))
                {
                    if (GameManager.instance.GetPlayerDataBool(playerDataBool) == expected)
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
                }
            }
        }
    }

    public class SetFsmStateOnPersistentBool : FsmStateAction
    {
        bool[] cachedEnabled;
        bool revert = false;

        Fsm fsm;
        FsmState oldState;
        FsmState newState;
        PersistentBoolItem persistent;
        bool expected;

        public SetFsmStateOnPersistentBool(Fsm fsm, FsmState oldState, FsmState newState, PersistentBoolItem persistent, bool expected)
        {
            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.persistent = persistent;
            this.expected = expected;
        }

        public override void OnEnter()
        {
            //Open_Ended_Item_Replacer.logSource.LogWarning("Action entered: setFsmStateOnPersistentBool");

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
                // Handles persistence set by new item
                if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID) == expected)
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
            }
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
            UniqueID uniqueID = new UniqueID(gameObject, itemName);

            // Generates a generic item using the uniqueID
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
            Open_Ended_Item_Replacer.SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;
        }

        public override void OnEnter()
        {
            // Handles persistence set by new item
            if (!SceneData.instance.PersistentBools.GetValueOrDefault(genericItem.persistentBoolItem.ItemData.SceneName, genericItem.persistentBoolItem.ItemData.ID))
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
        PersistentBoolItem persistentBoolItem;

        public SetContainerPersistence(PersistentBoolItem persistentBoolItem)
        {
            this.persistentBoolItem = persistentBoolItem;
        }

        public override void OnEnter()
        {
            persistentBoolItem.ItemData.Value = true;
            SceneData.instance.PersistentBools.SetValue(persistentBoolItem.ItemData);

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
            logSource.LogInfo("Item get: " + persistentBoolItem.ItemData.ID);
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

        private static FsmStateAction[] AddActionsPre(FsmStateAction[] newActions, FsmStateAction[] actions)
        {
            FsmStateAction[] replacementActions = new FsmStateAction[newActions.Length + actions.Length];

            Array.Copy(newActions, 0, replacementActions, 0, newActions.Length);
            Array.Copy(actions, 0, replacementActions, newActions.Length, actions.Length);

            return replacementActions;
        }

        private static FsmStateAction[] AddActionsPost(FsmStateAction[] newActions, FsmStateAction[] actions)
        {
            FsmStateAction[] replacementActions = new FsmStateAction[newActions.Length + actions.Length];

            Array.Copy(newActions, 0, replacementActions, actions.Length, newActions.Length);
            Array.Copy(actions, 0, replacementActions, 0, actions.Length);

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

        // Similarly used for debugging, will be removed later
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "LevelActivated")]
        public static void GameManager_LevelActivatedPostfix(GameManager __instance)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            logSource.LogMessage("Level Activated: " + sceneName);

            bool debugging = false;
            if (debugging)
            {
                LevelActivatedDebugging();
            }

            // DISABLED, DOESN'T TRACK PROPERLY
            // If in a memory, immediately leave
            /*if (sceneName.ToLower().Contains("memory") && (sceneName.ToLower().Contains("silk_heart") || sceneName.ToLower().Contains("needolin") || sceneName.ToLower().Contains("first_sinner")))
            {
                string nextSceneName;
                if (sceneName.ToLower().Contains("lacetower"))
                {
                    nextSceneName = "";
                }
                else if (sceneName.ToLower().Contains("wardboss"))
                {
                    nextSceneName = "Ward_02_Boss";
                }
                else // bell beast
                {
                    nextSceneName = "Bone_05";
                }

                SceneManager.LoadScene(nextSceneName);
            }*/

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

        private static Transform testTransform;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NailSlash), "StartSlash")]
        private static void StartSlashPostfix(NailSlash __instance)
        {
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

        /*private static void ReplaceSilkHeart(GameObject gameObject)
        {
            gameObject.SetActive(false);

            UniqueID uniqueID = new UniqueID(gameObject, "Silk Heart");

            Transform transform = SpawnGenericCollisionPickup(uniqueID, null, gameObject.transform, Vector3.zero);
        }*/

        // Should just replace kratt physically
        // Keeping code around for now as reference for changing dialogue
        /*private static void ReplaceKrattPickup(PlayMakerFSM __instance)
        {
            // Removing flags for processing
            string currentInstanceName = __instance.gameObject.name;
            if (currentInstanceName.EndsWith(replacedFlag))
            {
                currentInstanceName = currentInstanceName.Substring(0, currentInstanceName.Length - replacedFlag.Length);
            }
            else
            {
                __instance.gameObject.name += replacedFlag;
            }

            string currentItemName = "FleasCollected Target";

            // Defining the unique id for the new pickup
            string pickupName = currentInstanceName + "-" + currentItemName;
            string sceneName = GameManager.GetBaseSceneName(__instance.gameObject.scene.name);

            UniqueID uniqueID = new UniqueID(pickupName, sceneName);

            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = __instance.gameObject.AddComponent<PersistentBoolItem>();
            SetGenericPersistentInfo(uniqueID, persistent);
            genericItem.persistentItemBool = persistent;


            // Handle actions on "Break" state
            FsmStateAction[] breakActions = __instance.Fsm.GetState("Break").Actions;

            FsmBool krattBool = new FsmBool();
            krattBool.Value = false;

            // Stops kratt being marked as saved in the player bools
            HutongGames.PlayMaker.Actions.SetPlayerDataBool setKrattSaved = breakActions.OfType<HutongGames.PlayMaker.Actions.SetPlayerDataBool>().ToList()[0];
            Traverse.Create(setKrattSaved).Field("value").SetValue(krattBool);

            SavedItemGetV2 getItemBreak = breakActions.OfType<SavedItemGetV2>().ToList()[0];
            getItemBreak.Item = genericItem;
            getItemBreak.ShowPopup = true;
            getItemBreak.Amount = 1;


            // Handle actions on "NPC Ready" state
            FsmStateAction[] NPCReadyActions = __instance.Fsm.GetState("NPC Ready").Actions;

            SavedItemGetV2 getItemNPCReady = NPCReadyActions.OfType<SavedItemGetV2>().ToList()[0];
            getItemNPCReady.Enabled = false;
        }*/

        private static void ReplaceGiantFleaPickup(Transform giantFlea, PlayMakerFSM giantFleaFSM, PlayMakerFSM __instance)
        {
            UniqueID uniqueID = new UniqueID(giantFlea.gameObject, genericFleaItemName);

            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = __instance.gameObject.AddComponent<PersistentBoolItem>();
            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;


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
            if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID))
            {
                giantFlea.gameObject.SetActive(false);
                __instance.gameObject.SetActive(false);
            }
        }

        private static void ReplaceFsmItemGet(FsmStateAction __instance, SavedItem item)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            UniqueID uniqueID = new UniqueID(gameObject, item.name);

            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();

            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;

            // Handles persistence set by new item
            if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID))
            {
                logSource.LogInfo("Replacement set inactive");
            }
            else
            {
                genericItem.Get();
            }
        }

        private static void ReplaceFsmToolGet(SetToolUnlocked __instance)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            UniqueID uniqueID = new UniqueID(gameObject, (__instance.Tool.Value as ToolItem).name);

            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();

            SetGenericPersistentInfo(uniqueID, persistent);

            genericItem.persistentBoolItem = persistent;

            // Handles persistence set by new item
            if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID))
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
        private static void PersistentBoolItem_AwakePostfix(PersistentBoolItem __instance)
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

        private static void HandleCheckQuestPdSceneBoolFlea(CheckQuestPdSceneBool genericPersistenceChecker, PlayMakerFSM __instance)
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
                                Replace(gameObject, genericFleaItemName, false, null);
                                return;
                            }
                        }
                    }

                    PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
                    UniqueID uniqueID = new UniqueID(gameObject, genericFleaItemName);
                    SetGenericPersistentInfo(uniqueID, persistent);

                    Transform replacmentTransform;

                    // Replace any contained fleas
                    FsmGameObject fleaFsmGameObject = __instance.FsmVariables.GetFsmGameObject("Flea");
                    if (fleaFsmGameObject.Value == null)
                    {
                        GameObject dummyFlea = new GameObject();
                        dummyFlea.transform.position = __instance.transform.position;

                        replacmentTransform = Replace(dummyFlea, genericFleaItemName, true, null);
                    }
                    else
                    {
                        replacmentTransform = Replace(fleaFsmGameObject.Value, genericFleaItemName, true, null);
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

                                    newActions[0] = new SetContainerPersistence(persistent);
                                    newActions[1] = setGravity2dScaleV2;
                                    newActions[2] = new AllowPickup(replacmentTransform.GetComponent<CollectableItemPickup>());

                                    //Array.Copy(nextState.Actions, 0, newActions, numberOfNewActions, nextState.Actions.Length);
                                    nextState.Actions = AddActionsPre(newActions, nextState.Actions);

                                    //nextState.Actions = newActions;
                                }
                            }
                        }
                    }

                    // Handles persistence for the container
                    if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID))
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
                    Replace(gameObject, genericFleaItemName, true, null);
                    break;

                default:
                    logSource.LogError("State handled with incorrect name");
                    break;
            }
        }

        private static void HandleKrattFlea(PlayerDataBoolTest krattPersistenceChecker, PlayMakerFSM __instance)
        {
            if (krattPersistenceChecker == null) { return; }

            logSource.LogInfo("Kratt flagged");
            krattPersistenceChecker.isTrue = new FsmEvent("");

            Replace(__instance.gameObject, genericFleaItemName, true, null);
        }

        private static void HandleGiantFlea(PlayerDataBoolTest giantFleaPersistenceChecker, PlayMakerFSM __instance)
        {
            if (giantFleaPersistenceChecker == null) { return; }

            logSource.LogInfo("Giant flea flagged");
            giantFleaPersistenceChecker.isTrue = new FsmEvent("");

            Transform giantFlea = (__instance.FsmVariables.GetVariable("Parent").RawValue as GameObject).transform.Find("Giant Flea");

            PlayMakerFSM[] giantFleaFSMs = giantFlea.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM giantFleaFSM in giantFleaFSMs)
            {
                if (giantFleaFSM.FsmName == "Control")
                {
                    ReplaceGiantFleaPickup(giantFlea, giantFleaFSM, __instance);
                }
            }
        }

        private static void HandleVogFlea(PlayerDataVariableTest vogPersistenceChecker, PlayMakerFSM __instance)
        {
            if (vogPersistenceChecker == null) { return; }

            logSource.LogInfo("Vog flagged");
            vogPersistenceChecker.IsExpectedEvent = new FsmEvent("TRUE");

            Replace(__instance.gameObject, genericFleaItemName, true, null);
        }

        private static void HandleFrozenFlea(PlayMakerFSM __instance)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.gameObject.name.Contains("Snowflake Chunk - Flea") && __instance.name.Contains("Control"))
            {
                logSource.LogInfo("Frozen flea flagged");

                Replace(__instance.gameObject, genericFleaItemName, false, null);
            }
        }

        private static void HandleAspidFlea(PlayMakerFSM __instance)
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

                    Replace(__instance.gameObject, genericFleaItemName, false, null);
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

            // Should flag every flea except
            // -> Giant flea
            // -> Vog
            // -> Kratt
            // -> That one aspid flea
            // -> That one frozen flea
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(initState, genericFleaItemName), __instance); // Misc fleas
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(checkState, genericFleaItemName), __instance); // Like bellhart and karak flea
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(sleepState, genericFleaItemName), __instance); // Seepy fleas -> gameObject can be replaced with no restrictions
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(idleState, genericFleaItemName), __instance); // Fancy citadel cage fleas and slab cell flea

            // Specifically for Kratt
            HandleKrattFlea(SearchForPlayerDataBoolTest(initState, "CaravanLechSaved"), __instance);

            // Specifically for giant flea
            HandleGiantFlea(SearchForPlayerDataBoolTest(idleState, "tamedGiantFlea"), __instance);
            
            // Specifically for Vog
            FsmState stillHereState = __instance.Fsm.GetState("Still Here?");
            HandleVogFlea(SearchForPlayerDataVariableTest(stillHereState, "MetTroupeHunterWild"), __instance);

            // Specifically for frozen flea
            HandleFrozenFlea(__instance);

            // Specifically for aspid flea
            HandleAspidFlea(__instance);
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
                        nextState.Actions = AddActionsPre(newActions, nextState.Actions);

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
                save.Actions = AddActionsPre(newActions, save.Actions);

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
                endScene.Actions = AddActionsPre(newActions, endScene.Actions);

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
                if (SceneData.instance.PersistentBools.GetValueOrDefault("Memory_First_Sinner", uniqueID.PickupName + replacementFlag))
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

                GameObject dummyGameObject = new GameObject("Rune Bomb");
                newActions[0] = new GetCheck(dummyGameObject, "Rune Bomb"); // Replace

                //Array.Copy(getRuneBomb.Actions, 0, newActions, numberOfNewActions, getRuneBomb.Actions.Length);
                getRuneBomb.Actions = AddActionsPre(newActions, getRuneBomb.Actions);

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

                UIMsg.Actions = AddActionsPre(newActionsPre, UIMsg.Actions);

                FsmStateAction[] newActionsPost = new FsmStateAction[1];
                newActionsPost[0] = new SetFsmActiveState(__instance.Fsm, UIMsg, __instance.Fsm.GetState("End Pause"));
                AddActionsPost(newActionsPost, UIMsg.Actions);

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

                // Generates an equivalent persistence to test whether the sequence has already been done
                GameObject gameObject = new GameObject("puzzle cylinders");
                UniqueID uniqueID = new UniqueID(gameObject, "Citadel Ascent Melody Architect");
                PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
                SetGenericPersistentInfo(uniqueID, persistent);

                waitForNotify.Actions[0] = new SetFsmStateOnPersistentBool(__instance.Fsm, waitForNotify, hasMelody, persistent, true); // Replaces original persistence check with custom

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmStateOnPlayerDataBool(__instance.Fsm, startLock, waitForNotify, "hasNeedolin", false); // Disables allowing getting the song part without needolin

                //Array.Copy(startLock.Actions, 0, newActions, numberOfNewActions, startLock.Actions.Length);
                startLock.Actions = AddActionsPre(newActions, startLock.Actions);

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

                int numberOfNewActions = 2;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmStateOnPlayerDataBool(__instance.Fsm, questActive, melodyNoQuest, "hasNeedolin", false); // Disables allowing getting the song part without needolin
                newActions[1] = new TestAction();
                questActive.Actions = AddActionsPre(newActions, questActive.Actions);

                // Generates an equivalent persistence to test whether the sequence has already been done
                GameObject gameObject = new GameObject("Last Conductor NPC");
                UniqueID uniqueID = new UniqueID(gameObject, "Citadel Ascent Melody Conductor");
                PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
                SetGenericPersistentInfo(uniqueID, persistent);

                hasItem.Actions[8] = new SetFsmStateOnPersistentBool(__instance.Fsm, hasItem, repeatDlg, persistent, true);
            }
        }

        // Handles FSM checks
        // All fleas have SavedItems that are gotten at the end of their fsms
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerFSM_AwakePostfix(PlayMakerFSM __instance)
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
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(Fsm), "Awake")]
        private static void PlayMakerFSM_AwakePostfix(Fsm __instance)
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
        }*/

        // Handles when FSMs run CollectableItemCollect
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CollectableItemCollect), "DoAction")]
        private static bool CollectableItemCollect_DoActionPrefix(CollectableItemCollect __instance, CollectableItem item)
        {
            ReplaceFsmItemGet(__instance, item);

            return false;
        }

        // Handles when FSMs run SavedItemGet
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGet), "OnEnter")]
        private static bool SavedItemGet_OnEnterPrefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SavedItemGetV2
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGetV2), "OnEnter")]
        private static bool SavedItemGetV2_OnEnterPrefix(SavedItemGet __instance)
        {
            if (__instance.Item.Name.Contains(genericFleaItemName) && __instance.Item.Name.Contains("Generic_Item-"))
            {
                return true;
            }

            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SavedItemGetDelayed
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SavedItemGetDelayed), "DoGet")]
        private static bool SavedItemGetDelayed_DoGetPrefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SetToolUnlocked
        // Should handle the vast majority of cases of being given an item from an NPC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetToolUnlocked), "OnEnter")]
        private static bool SetToolUnlocked_OnEnterPrefix(SetToolUnlocked __instance)
        {
            ReplaceFsmToolGet(__instance);

            __instance.Finish();
            return false;
        }

        // Handles when FSMs run SetToolLocked
        // Stops NPCs locking tools when not actually replacing them
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetToolLocked), "OnEnter")]
        private static bool SetToolLocked_OnEnterPrefix(SetToolLocked __instance)
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

                //Array.Copy(init.Actions, 0, newActions, numberOfNewActions, init.Actions.Length);
                init.Actions = AddActionsPre(newActions, init.Actions);

                //init.Actions = newActions;


                /*FsmState[] states = playMakerFsm.Fsm.States;

                foreach (FsmState state in states)
                {
                    // Space is important as one state is called "Setup And Wait"
                    if (state.Name.Contains("Set "))
                    {
                        int numberOfNewActions = 1;

                        FsmStateAction[] newActions = new FsmStateAction[state.Actions.Length + numberOfNewActions];

                        SendEvent sendEvent = new SendEvent();

                        FsmEventTarget eventTarget = new FsmEventTarget();
                        eventTarget.target = EventTarget.BroadcastAll;

                        sendEvent.eventTarget = eventTarget;
                        sendEvent.sendEvent = FsmEvent.GetFsmEvent("SKIP");
                        sendEvent.delay = 0;
                        sendEvent.everyFrame = false;

                        newActions[0] = sendEvent;

                        state.Transitions.AddItem(new FsmTransition(playMakerFsm.Fsm.GetState("Set Silk Heart").GetTransition(1)));

                        Array.Copy(state.Actions, 0, newActions, numberOfNewActions, state.Actions.Length);

                        state.Actions = newActions;
                    }
                }*/
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
                init.Actions = AddActionsPre(newActionsForInit, init.Actions);

                (wait.Actions[0] as Wait).time = 0;

                FsmStateAction[] newActionsForStopUp = new FsmStateAction[1];
                newActionsForStopUp[newActionsForStopUp.Length - 1] = new SetFsmActiveState(playMakerFsm.Fsm, stopUp, done);
                //Array.Copy(init.Actions, 0, newActionsForStopUp, 0, init.Actions.Length);
                //stopUp.Actions = newActionsForStopUp;
                stopUp.Actions = AddActionsPre(newActionsForStopUp, stopUp.Actions);

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreateUIMsgGetItem), "OnEnter")]
        private static void CreateUIMsgGetItem_OnEnterPrefix(CreateUIMsgGetItem __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.storeObject.Value.transform.GetComponent<PlayMakerFSM>();

            HandleUiMsgGetItem(playMakerFsm);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnObjectFromGlobalPool), "OnEnter")]
        private static void SpawnObjectFromGlobalPool_OnEnterPrefix(SpawnObjectFromGlobalPool __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.gameObject.Value.transform.GetComponent<PlayMakerFSM>();
            if (playMakerFsm == null) { return; }

            HandleUiMsgGetItemMelody(playMakerFsm, __instance);
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(LocalisedString), "ToString", new Type[] { typeof(bool) })]
        public static void LocalisedString_ToString(LocalisedString __instance, ref string __result)
        {
            if (__result.Contains("!!/!!"))
            {
                __result = "boingo";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestManager), "Start")]
        public static void QuestManager_StartPostFix(FullQuestBase __instance)
        {
            var quests = QuestManager.GetAllQuests();

            foreach (var quest in quests)
            {
                logSource.LogInfo(Traverse.Create(quest).Field("DisplayName").GetValue<LocalisedString>());

                logSource.LogInfo(Traverse.Create(quest).Field("DisplayName").SetValue(new LocalisedString("", "")));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BasicQuestBase), "Init")]
        public static void BasicQuestBase_InitPostFix(BasicQuestBase __instance)
        {
            logSource.LogInfo(Traverse.Create(__instance.QuestType).Field("displayName").SetValue(new LocalisedString("", "")));
        }*/

        // Replaces CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        // I have somewhat arbitrarily picked OnEnable over awake here as I am hoping that if there are pickups that start disabled they aren't replaced until they are enabled
        private static bool spawningReplacementCollectableItemPickup = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void CollectableItemPickup_OnEnablePostfix(CollectableItemPickup __instance) //, PersistentBoolItem ___persistent)
        {
            logSource.LogMessage("CollectableItemPickup Enabled");

            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!spawningReplacementCollectableItemPickup)
            {
                // Using Harmony's traverse tool, the private field "persistent" can be copied
                // Persistance tracks data about pickups independantly to the item they contain, so this needs to be preserved to allow tracking of what pickups have been interacted with
                //PersistentBoolItem replacedPersistent = ___persistent;
                
                // Traverse.Create(__instance).Field("persistent").GetValue<PersistentBoolItem>();

                if (__instance.Item.name.Contains("Common Spine")) // will generalise a check for active later
                {
                    return;
                }

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
            if (SceneData.instance.PersistentBools.GetValueOrDefault(persistent.ItemData.SceneName, persistent.ItemData.ID))
            {
                collectableItemPickup.gameObject.SetActive(false);
                logSource.LogInfo("Replacement set inactive");
            }
        }

        public static void SetGenericPersistentInfo(UniqueID uniqueID, PersistentBoolItem persistent)
        {
            // Makes sure that persistent has loaded and that hasSetup = true
            persistent.LoadIfNeverStarted();
            persistent.ItemData.ToString();

            // Sets persistent data
            persistent.ItemData.ID = uniqueID.PickupName + replacementFlag;
            persistent.ItemData.SceneName = uniqueID.SceneName;
            persistent.ItemData.IsSemiPersistent = false;
            persistent.ItemData.Value = false;
            persistent.ItemData.Mutator = SceneData.PersistentMutatorTypes.None;
        }
    }
}
