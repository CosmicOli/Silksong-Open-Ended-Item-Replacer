using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public UniqueID(string PickupName, string SceneName)
        {
            this.PickupName = PickupName;
            this.SceneName = SceneName;
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

        public PersistentBoolItem persistentItemBool;

        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

            // Show popup (if showPopup)
            // Send get request
            logSource.LogInfo("Item get: " + persistentItemBool.ItemData.ID);
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
            Logger.LogInfo("Plugin loaded and initualised.");

            BepInEx.Logging.Logger.Sources.Add(logSource);

            Harmony.CreateAndPatchAll(typeof(Open_Ended_Item_Replacer), null);
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

        // Similarly used for debugging, will be removed later
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "LevelActivated")]
        public static void GameManager_LevelActivatedPostfix(GameManager __instance)
        {
            logSource.LogMessage("Level Activated: " + SceneManager.GetActiveScene().name);

            GameManager GameManager = GameManager.instance;
            HeroController heroControl = GameManager.hero_ctrl;

            bool logging = false;
            if (logging)
            {
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
        }



        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SavedItem), "TryGet")]
        private static void SavedItem_GetPostfix(SavedItem __instance)
        {
            logSource.LogFatal(__instance.name);
        }*/

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(NailSlash), "StartSlash")]
        private static void StartSlashPostfix(NailSlash __instance)
        {
            logSource.LogMessage("Slash Postfix");

            HeroController heroControl = GameManager.instance.hero_ctrl;

            UniqueID uniqueID = new UniqueID("pickupName", "sceneName");
            spawningReplacementCollectableItemPickup = true;
            SpawnGenericInteractablePickup(uniqueID, null, heroControl.transform, new Vector3(0, 0, 0));
            spawningReplacementCollectableItemPickup = false;
        }*/

        private static string replacementFlag = "_(Replacement)";
        private static string replacedFlag = "_(Replaced)";
        
        // Some pickups have 'items', and others do not
        // Those that do need to have their item's name changed
        private static void Replace(GameObject replacedObject, UnityEngine.Object replacedItem, bool interactable, CollectableItemPickup replacementPrefab)
        {
            // Removing flags for processing
            if (!replacedItem.name.EndsWith(replacedFlag))
            {
                replacedItem.name += replacedFlag;
            }

            Replace(replacedObject, replacedItem.name, interactable, replacementPrefab);
        }

        // Deactivates and replaces a given object
        private static void Replace(GameObject replacedObject, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab)
        {
            // Sets up the replacement object to not be replaced itself
            spawningReplacementCollectableItemPickup = true;

            // Removes the original object
            replacedObject.SetActive(false);

            // Removing flags for processing
            string currentInstanceName = replacedObject.name;
            if (currentInstanceName.EndsWith(replacedFlag))
            {
                currentInstanceName = currentInstanceName.Substring(0, currentInstanceName.Length - replacedFlag.Length);
            }
            else
            {
                replacedObject.name += replacedFlag;
            }

            string currentItemName = replacedItemName;
            if (currentItemName.EndsWith(replacedFlag))
            {
                currentItemName = currentItemName.Substring(0, currentItemName.Length - replacedFlag.Length);
            }

            // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
            logSource.LogInfo("Pickup: " + currentInstanceName);

            // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
            logSource.LogInfo("Pickup At: " + replacedObject.transform.position);

            // Defining the unique id for the new pickup
            string pickupName = currentInstanceName + "-" + currentItemName;
            string sceneName = GameManager.GetBaseSceneName(replacedObject.scene.name);

            UniqueID uniqueID = new UniqueID(pickupName, sceneName);

            // Attempts to spawn the replacement object
            logSource.LogInfo("Pickup Drop Attempt Start");
            if (interactable)
            {
                SpawnGenericInteractablePickup(uniqueID, replacementPrefab, replacedObject.transform, new Vector3(0, 0, 0));
            }
            else
            {
                SpawnGenericCollisionPickup(uniqueID, replacementPrefab, replacedObject.transform, new Vector3(0, 0, 0));
            }
            logSource.LogInfo("Pickup Drop Attempt End");

            spawningReplacementCollectableItemPickup = false;
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

        // Handles anything that contains a flea object
        private static void HandleFleaContainer(PlayMakerFSM __instance)
        {
            if (__instance == null) { return; }

            FsmVariables variables = __instance.FsmVariables;

            if (variables.Contains("Flea"))
            {
                logSource.LogFatal("Flea holder found");

                FsmGameObject fleaFsmGameObject = variables.GetFsmGameObject("Flea");
                if (fleaFsmGameObject?.Value == null) { return; }

                if (fleaFsmGameObject.Value?.transform?.parent)
                {
                    Transform parent = fleaFsmGameObject.Value.transform.parent;
                    logSource.LogFatal("Parent found: " + parent.name);

                    for (int i = 0; i < parent.childCount; i++)
                    {
                        logSource.LogInfo(parent.GetChild(i).name);

                        if (parent.GetChild(i).name.ToLower().Contains("flea"))
                        {
                            parent.GetChild(i).gameObject.SetActive(false);

                            PlayMakerFSM[] fleaFSMs = parent.GetChild(i).GetComponents<PlayMakerFSM>();
                            // Disables any fsms relating to getting a flea
                            if (fleaFSMs != null)
                            {
                                foreach (PlayMakerFSM fleaFSM in fleaFSMs)
                                {
                                    fleaFSM.enabled = false;
                                }
                            }
                        }
                    }

                    // Checks if anything enables the flea we want disabled, and then removes the ability to enable it
                    PlayMakerFSM[] parentFSMs = parent.GetComponents<PlayMakerFSM>();
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
                        }
                    }
                }
            }
        }

        // Handles anything that is a flea
        // Distinct from containers as some fleas are not contained
        private static void HandleFlea(PlayMakerFSM __instance)
        {

        }

        // Handles FSM checks
        // All fleas have SavedItems that are gotten at the end of their fsms
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerFSM_AwakePostfix(PlayMakerFSM __instance)
        {
            HandleFleaContainer(__instance);
            /*FsmState[] states = __instance.FsmStates;
            if (states == null) { return; }

            foreach (FsmState state in states)
            {
                FsmStateAction[] actions = state.Actions;
                if (actions == null) { continue; }

                foreach (FsmStateAction action in actions)
                {
                    ActivateGameObject actionActivateGameObject = action as ActivateGameObject;

                    if (actionActivateGameObject == null) { continue; }
                    if (actionActivateGameObject.gameObject == null) { continue; }
                    if (actionActivateGameObject.gameObject.GameObject == null) { continue; }

                    FsmGameObject fleaFsmGameObject = actionActivateGameObject.gameObject.GameObject;

                    if (fleaFsmGameObject.Name.ToLower().StartsWith("flea"))
                    {
                        logSource.LogFatal("Flea holder found");

                        fleaFsmGameObject.
                    }
                }
            }*/


            /*if (__instance.gameObject.name.StartsWith("Aspid Collector"))
            {
                UnityEngine.Component[] components = __instance.gameObject.GetComponents<UnityEngine.Component>();

                foreach (var component in components)
                {
                    logSource.LogMessage(component);
                }

                int count = __instance.gameObject.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    logSource.LogMessage(__instance.gameObject.transform.GetChild(i));
                }
            }*/
        }


        // Replaces CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        // I have somewhat arbitrarily picked OnEnable over awake here as I am hoping that if there are pickups that start disabled they aren't replaced until they are enabled
        private static bool spawningReplacementCollectableItemPickup = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void CollectableItemPickup_OnEnablePostfix(CollectableItemPickup __instance)
        {
            logSource.LogMessage("CollectableItemPickup Enabled");

            // Currently all replacement prefabs have to be CollectableItemPickups, so they need to not be replaced themselves
            if (!spawningReplacementCollectableItemPickup)
            {
                // Using Harmony's traverse tool, the private field "persistent" can be copied
                // Persistance tracks data about pickups independantly to the item they contain, so this needs to be preserved to allow tracking of what pickups have been interacted with
                PersistentBoolItem replacedPersistent = Traverse.Create(__instance).Field("persistent").GetValue<PersistentBoolItem>();

                Replace(__instance.gameObject, __instance.Item, true, null);
            }
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        private static void SpawnGenericInteractablePickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
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
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        private static void SpawnGenericCollisionPickup(UniqueID uniqueID, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
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
        }

        private static void SetGenericPickupInfo(UniqueID uniqueID, CollectableItemPickup collectableItemPickup)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

            // This logs where the pickup has been placed
            logSource.LogInfo("New Pickup Placed At: " + collectableItemPickup.transform.position);

            PersistentBoolItem persistent = Traverse.Create(collectableItemPickup).Field("persistent").GetValue<PersistentBoolItem>();

            // Makes sure that persistent has loaded and that hasSetup = true
            persistent.LoadIfNeverStarted();
            persistent.ItemData.ToString();

            // Sets persistent data
            persistent.ItemData.ID = uniqueID.PickupName + replacementFlag;
            persistent.ItemData.SceneName = uniqueID.SceneName;
            persistent.ItemData.IsSemiPersistent = false;
            persistent.ItemData.Value = true;
            persistent.ItemData.Mutator = SceneData.PersistentMutatorTypes.None;

            genericItem.persistentItemBool = persistent;

            // Sets the item granted upon pickup
            collectableItemPickup.SetItem(genericItem, true);

            logSource.LogInfo("Pickup Item Set: " + genericItem.name);
        }
    }
}
