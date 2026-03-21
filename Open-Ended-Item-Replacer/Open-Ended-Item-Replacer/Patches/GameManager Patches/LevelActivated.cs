using HarmonyLib;
using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Components;
using Open_Ended_Item_Replacer.FsmStateActions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "LevelActivated")]
    internal class LevelActivated
    {
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

        private static bool hasGranted;

        public static void Postfix(GameManager __instance)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            logSource.LogMessage("Level Activated: " + sceneName);

            bool debugging = false;
            if (debugging)
            {
                LevelActivatedDebugging();
            }

            // On entering act 3, get missables
            if (sceneName == "Song_Tower_Destroyed" && PlayerData.instance.blackThreadWorld)
            {
                PersistentItemData<bool> ChurchkeeperSoul = GeneratePersistentBoolData_SameScene("", "");
                PersistentItemData<bool> BellHermitSoul = GeneratePersistentBoolData_SameScene("", "");
                PersistentItemData<bool> ArchitectMelody = GeneratePersistentBoolData_SameScene("", "");
                PersistentItemData<bool> SteelSpines = GeneratePersistentBoolData_SameScene("", "");

                GenericSavedItem genericSavedItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                if (GetPersistentBoolFromData(ChurchkeeperSoul))
                {
                    GetCheck
                    genericSavedItem.PersistentBoolItem = ChurchkeeperSoul;
                    genericSavedItem.Get();
                }
            }

            // Stops softlocking in memories
            PlayerData playerData = PlayerData.instance;
            if (!hasGranted)
            {
                if (sceneName.ToLowerInvariant().Contains("memory") && (sceneName.ToLowerInvariant().Contains("silk_heart") || sceneName.ToLowerInvariant().Contains("needolin") || sceneName.ToLowerInvariant().Contains("first_sinner")))
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
                if (!sceneName.ToLowerInvariant().Contains("memory"))
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
    }
}
