using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Open_Ended_Item_Replacer
{
    // This object defines the item that replaces intended items
    // TODO: 
    // -> Show popup
    // -> Make functions make open ended requests
    public class GenericSavedItem : SavedItem
    {
        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

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
            GameObject [] rootObjects = CurrentScene.GetRootGameObjects();

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

        // Initially drafted code for replacing CollectableItemPickups
        // Done in post to avoid any following code attempting to run after the associated game object has been destroyed
        private static bool replacing = true;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void OnEnablePostfix(CollectableItemPickup __instance)
        {
            GameManager GameManager = GameManager.instance;
            HeroController heroControl = GameManager.hero_ctrl;

            logSource.LogMessage("Pickup Enabled");
            logSource.LogInfo("Pickup At: " + __instance.transform.position);

            if (replacing)
            {
                replacing = false;
                Destroy(__instance.gameObject);
                logSource.LogInfo("Deleted Pickup Containing: " + __instance.Item.name);

                GenericSavedItem testItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                testItem.name = "testItem";

                SpawnGenericItemDrop(testItem, null, __instance.gameObject.transform, new Vector3(0, 0, 0));
                logSource.LogInfo("Item Drop Attempted");
            }
            else
            {
                replacing = true;
            }
        }

        private static void SpawnGenericItemDrop(SavedItem dropItem, CollectableItemPickup prefab, Transform spawnPoint, Vector3 offset)
        {
            if (!prefab)
            {
                logSource.LogInfo("No prefab provided, using CollectableItemPickupPrefab");
                prefab = Gameplay.CollectableItemPickupPrefab;
            }

            Vector3 vector = spawnPoint.TransformPoint(offset);
            Vector3 position = vector;

            PrefabCollectable prefabCollectable = dropItem as PrefabCollectable;

            if (prefabCollectable != null)
            {
                prefabCollectable.Spawn();
            }
            else
            {
                CollectableItemPickup collectableItemPickup;

                collectableItemPickup = Instantiate(prefab);
                collectableItemPickup.transform.position = position;

                collectableItemPickup.SetItem(dropItem);
                logSource.LogInfo("Pickup Item Set: " + dropItem.name);
            }
        }
    }
}
