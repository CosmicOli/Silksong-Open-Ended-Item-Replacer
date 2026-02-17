using BepInEx;
using BepInEx.Logging;
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
                Console.WriteLine(gameObject.transform.name);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CollectableItemPickup), "OnEnable")]
        private static void OnEnablePostfix()
        {
            logSource.LogMessage("Collectable enabled");
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(NailSlash), "StartSlash")]
        public static void StartSlashPostfix(NailSlash __instance)
        {
            logSource.LogMessage("Slash Postfix");

            GameManager GameManager = GameManager.instance;
            HeroController heroControl = GameManager.hero_ctrl;

            Scene CurrentScene = SceneManager.GetActiveScene();

            GameObject[] rootObjects = CurrentScene.GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                Console.WriteLine(rootObject.transform.name);
            }
        }*/
    }
}
