using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Open_Ended_Item_Replacer
{
    public class testPB : PersistentBoolItem
    {
        public testPB()
        {

        }

        public void forceAwake()
        {
            Awake();
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }

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

        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

            SceneData sceneData = SceneData.instance;

            // Show popup (if showPopup)
            // Send get request
            logSource.LogInfo("Item get");
        }

        public override bool CanGetMore()
        {
            return true;
        }

        public override Sprite GetPopupIcon()
        {
            if (Application.isPlaying)
            {
                Debug.LogException(new NotImplementedException());
            }

            return null;
        }

        public override string GetPopupName()
        {
            if (Application.isPlaying)
            {
                Debug.LogException(new NotImplementedException());
            }

            return null;
        }
    }

    // I'm sure I'll update the version number at some point, right?
    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        public static ManualLogSource logSource = new ManualLogSource("logSource");

        //public static Scene FirstScene;
        //public static GameObject templatePickup;
        //public static PersistentBoolItem templatePersistentBoolItem;

        private void Awake()
        {
            //FirstScene = SceneManager.GetSceneByName("Tut_01");
            //templatePickup = Array.Find(FirstScene.GetRootGameObjects(), x => x.name == "Collectable Item Pickup");
            //templatePersistentBoolItem = Traverse.Create(templatePickup).Field("persistent").GetValue<PersistentBoolItem>();

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
        public static void LevelActivatedPostfix(GameManager __instance)
        {
            logSource.LogMessage("Level Activated");

            GameManager GameManager = GameManager.instance;
            HeroController heroControl = GameManager.hero_ctrl;

            List<GameObject> gameObjectsInCurrentScene = FindAllObjectsInCurrentScene();

            foreach (GameObject gameObject in gameObjectsInCurrentScene)
            {
                //Console.WriteLine(gameObject.transform.name);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PersistentBoolItem), "Awake")]
        private static void AwakeTest(PersistentBoolItem __instance)
        {
            //logSource.LogFatal("AWAKE RAN");
        }

        // Initially drafted code for replacing CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        private static bool replacing = true;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void OnEnablePostfix(CollectableItemPickup __instance)
        {
            GameManager gameManager = GameManager.instance;

            logSource.LogMessage("Pickup Enabled");
            logSource.LogInfo("Pickup At: " + __instance.transform.position);

            // When replacing a CollectableItemPickup with a CollectableItemPickup, whether the item enabled is supposed to be replaced needs to be temporarily tracked
            if (replacing)
            {
                // Sets up the replacement object to not be replaced itself
                replacing = false;

                // Defining the unique id for the new pickup
                string pickupName = __instance.gameObject.name;
                string sceneName = gameManager.GetSceneNameString();

                UniqueID uniqueID = new UniqueID(pickupName, sceneName);

                // Using Harmony's traverse tool, the private field "persistent" can be copied
                // Persistance tracks data about pickups independantly to the item they contain, so this needs to be preserved to allow tracking of what pickups have been interacted with
                PersistentBoolItem persistent = Traverse.Create(__instance).Field("persistent").GetValue<PersistentBoolItem>();

                // Pickups for items like relics and tools are tracked internally by checking for possession
                // Hence, to track pickups independantly, persistence has to be created
                if (!(bool)persistent)
                {
                    //testPB persistent2 = new testPB();
                    //MethodInfo AwakeMethod = Traverse.Create(persistent).Method("Awake").GetValue<MethodInfo>();
                    //MethodInfo AwakeMethod = AccessTools.Method(typeof(PersistentBoolItem), "Awake");
                    //AwakeMethod.Invoke(persistent, new object[0]);

                    //logSource.LogMessage("Attempted to make new pb");

                    //persistent2.forceAwake();
                    //logSource.LogMessage("Attempted to awake");

                    //logSource.LogMessage("Attempted to awake");

                    //persistent = persistent2;
                    //logSource.LogMessage(persistent2.ItemData);

                    GameObject dummyGameObject = new GameObject();
                    persistent = dummyGameObject.AddComponent<PersistentBoolItem>();

                    Traverse.Create(__instance).Field("persistent").SetValue(persistent);

                    PersistentItemData<bool> itemData = new PersistentItemData<bool>();

                    itemData.ID = pickupName;
                    itemData.SceneName = sceneName;
                    itemData.IsSemiPersistent = false;
                    itemData.Value = true;
                    itemData.Mutator = SceneData.PersistentMutatorTypes.None;

                    Traverse.Create(persistent).Field("SerializedItemData").SetValue(itemData);

                    logSource.LogMessage("Attempted to set value");

                    persistent.PreSetup();

                    logSource.LogMessage("Attempted to setup");

                    /*persistent.ItemData.ID = pickupName;
                    persistent.ItemData.SceneName = sceneName;
                    persistent.ItemData.IsSemiPersistent = false;
                    persistent.ItemData.Value = true;
                    persistent.ItemData.Mutator = SceneData.PersistentMutatorTypes.None;*/

                    //logSource.LogMessage("Attempted to change data");
                }

                persistent.ItemData.ID += "_(Replacement)";

                // Removes the original object
                __instance.gameObject.SetActive(false);
                logSource.LogInfo("Deactivated Pickup Containing: " + __instance.Item.name);

                // Renaming old CollectableItemPickup to avoid persistent bool tracking issues
                // This is because for some reason the ID saved to the player's save data is not PersistentBoolItem.ItemData.ID but instead CollectableItemPickup.gameObject.name
                __instance.gameObject.name = __instance.gameObject.name + "_(Replaced)";
                __instance.Item.name = __instance.Item.name + "_(Replaced)";

                // Attempts to spawn the replacement object
                SpawnGenericItemDrop(uniqueID, persistent, null, __instance.gameObject.transform, new Vector3(0, 0, 0));
                logSource.LogInfo("Item Drop Attempted");

                // The next CollectableItemPickup enabled will need replacing
                replacing = true;
            }
        }

        // Spawns a replacement pickup, defining the item with uniqueID
        private static void SpawnGenericItemDrop(UniqueID uniqueID, PersistentBoolItem persistent, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            genericItem.UniqueID = uniqueID;

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

            // Rename the new CollectableItemPickup
            // This is because for some reason the ID saved to the player's save data is not PersistentBoolItem.ItemData.ID but instead CollectableItemPickup.gameObject.name
            collectableItemPickup.gameObject.name = uniqueID.PickupName + "_(Replacement)";

            // Persistance data is transfered to the new handler
            Traverse.Create(collectableItemPickup).Field("persistent").SetValue(persistent);

            // Sets the item granted upon pickup
            collectableItemPickup.SetItem(genericItem);

            logSource.LogInfo("Pickup Item Set: " + genericItem.name);
        }
    }
}
