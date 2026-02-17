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
    public class GenericSavedItem : SavedItem
    {
        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = new ManualLogSource("logSource");

            // Show popup (if showPopup)
            // Send get request
            logSource.LogMessage("Item get");
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

    [BepInPlugin("com.oli.OEIR", "OEIR", "1.0.0")]
    public class Open_Ended_Item_Replacer : BaseUnityPlugin
    {
        private static ManualLogSource logSource = new ManualLogSource("logSource");

        private void Awake()
        {
            Logger.LogInfo("Plugin loaded and initualised.");

            BepInEx.Logging.Logger.Sources.Add(logSource);

            Harmony.CreateAndPatchAll(typeof(Open_Ended_Item_Replacer), null);
        }

        List<GameObject> startingGameObjects = new List<GameObject>();
        
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

        // Seems to have issues replacing more than one item in a room
        static bool replacing = true;
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

                SpawnItemDrop(testItem, 1, null, __instance.gameObject.transform, 1, new Vector3(0, 0, 0));
                logSource.LogInfo("Item Drop Attempted");
            }
            else
            {
                replacing = true;
            }
        }

        private static void SpawnItemDrop(SavedItem dropItem, int count, CollectableItemPickup prefab, Transform spawnPoint, int limit, Vector3 effectOrigin)
        {
            if (!dropItem || !dropItem.CanGetMore())
            {
                return;
            }

            if (!prefab)
            {
                prefab = Gameplay.CollectableItemPickupPrefab;
            }

            if (!prefab)
            {
                return;
            }

            Vector3 vector = spawnPoint.TransformPoint(effectOrigin);
            for (int i = 0; i < count; i++)
            {
                Vector3 position = vector;
                PrefabCollectable prefabCollectable = dropItem as PrefabCollectable;
                GameObject gameObject;
                bool flag;
                if (prefabCollectable != null)
                {
                    gameObject = prefabCollectable.Spawn();
                    flag = false;
                }
                else
                {
                    CollectableItemPickup collectableItemPickup;
                    if (limit > 0)
                    {
                        collectableItemPickup = ObjectPool.Spawn(prefab.gameObject, null, position, Quaternion.identity, stealActiveSpawned: true).GetComponent<CollectableItemPickup>();
                    }
                    else
                    {
                        collectableItemPickup = UnityEngine.Object.Instantiate(prefab);
                        collectableItemPickup.transform.position = position;
                    }

                    flag = true;
                    collectableItemPickup.SetItem(dropItem);
                    logSource.LogInfo("Pickup Item Set: " + dropItem.name);
                    gameObject = collectableItemPickup.gameObject;
                    //logSource.LogInfo(collectableItemPickup.Item.name);
                }
            }
        }
    }
}
