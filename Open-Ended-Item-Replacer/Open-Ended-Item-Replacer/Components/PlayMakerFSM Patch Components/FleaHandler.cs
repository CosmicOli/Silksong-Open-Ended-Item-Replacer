using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.ReplaceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class FleaHandler
    {
        public static string GenericFleaItemName = "FleasCollected Target";

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
                            if (variable.Name.ToLowerInvariant().Contains("flea") && variable.Name.ToLowerInvariant().Contains("sprite"))
                            {
                                containsFleaSprite = true;
                            }
                        }

                        if (!containsFleaSprite)
                        {
                            fleaObject.transform.position = __instance.transform.position;
                            Replace(fleaObject, gameObject, GenericFleaItemName, false);
                            return;
                        }
                    }

                    // The slab cage flea specifically looks like a flea but is not animated the same way
                    if (gameObject.name.ToLowerInvariant().Contains("flea slab cage"))
                    {
                        fleaObject.transform.position = __instance.transform.position;
                        Replace(fleaObject, gameObject, GenericFleaItemName, true);
                        return;
                    }

                    PersistentItemData<bool> persistentBoolData = GeneratePersistentBoolData(gameObject, GenericFleaItemName);

                    Transform replacmentTransform;

                    // Replace any contained fleas
                    FsmGameObject fleaFsmGameObject = __instance.FsmVariables.GetFsmGameObject("Flea");
                    if (fleaFsmGameObject.Value == null)
                    {
                        fleaObject.transform.position = __instance.transform.position;
                        replacmentTransform = Replace(fleaObject, gameObject, GenericFleaItemName, true);
                    }
                    else
                    {
                        fleaObject.transform.position = fleaFsmGameObject.Value.transform.position;
                        replacmentTransform = Replace(fleaObject, fleaFsmGameObject.Value, GenericFleaItemName, true);
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
                                    if (associatedGameObjectName.ToLowerInvariant().Contains("flea"))
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
                        Vector3 oldLocation = replacmentTransform.position;
                        gameObject.transform.position = defaultReplacedParentLocation;
                        replacmentTransform.position = oldLocation;
                        logSource.LogInfo("Container moved");
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
                    Replace(fleaObject, GenericFleaItemName, true);
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
            Replace(fleaObject, GenericFleaItemName, true);
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
            Replace(fleaObject, GenericFleaItemName, true);
        }

        private static void HandleFrozenFlea(PlayMakerFSM __instance, GameObject fleaObject)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.gameObject.name.Contains("Snowflake Chunk - Flea") && __instance.Fsm.Name.Contains("Control"))
            {
                logSource.LogInfo("Frozen flea flagged");

                fleaObject.transform.position = __instance.transform.position;
                __instance.gameObject.SetActive(false);
                Replace(fleaObject, GenericFleaItemName, false); // Note that in this case parenthood is given to the dummy object; for some reason giving it to the original flea causes random displacement
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
                    Replace(fleaObject, GenericFleaItemName, false);
                }
            }
        }

        // Handles anything that is or contains a flea
        public static void HandleFlea(PlayMakerFSM __instance)
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
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(initState, GenericFleaItemName), __instance, fleaObject); // Misc fleas
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(checkState, GenericFleaItemName), __instance, fleaObject); // Like bellhart and karak flea
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(sleepState, GenericFleaItemName), __instance, fleaObject); // Seepy fleas -> gameObject can be replaced with no restrictions
            HandleCheckQuestPdSceneBoolFlea(SearchForCheckQuestPdSceneBool(idleState, GenericFleaItemName), __instance, fleaObject); // Fancy citadel cage fleas and slab cell flea

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
    }
}
