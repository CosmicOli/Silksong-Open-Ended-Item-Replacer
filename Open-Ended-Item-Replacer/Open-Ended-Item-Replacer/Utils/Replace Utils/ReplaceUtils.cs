using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Components;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GlobalSettings;
using static Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches.Awake;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.SpawnUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Utils.Replace_Utils
{
    internal class ReplaceUtils
    {
        public static string replacementFlag = "-(Replacement)";

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab = null, Vector3 offset = new Vector3())
        {
            return Replace(replacedObject, replacedObject, replacedItemName, interactable, replacementPrefab, offset);
        }

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, GameObject activeParent, string replacedItemName, bool interactable, CollectableItemPickup replacementPrefab = null, Vector3 offset = new Vector3())
        {
            try
            {
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
                    output = SpawnGenericInteractablePickup(uniqueID, replacementPrefab, replacedObject.transform, offset);
                }
                else
                {
                    output = SpawnGenericCollisionPickup(uniqueID, replacementPrefab, replacedObject.transform, offset);
                }
                logSource.LogInfo("Pickup Drop Attempt End");

                HandleReplacedObject(replacedObject, activeParent, output);

                return output;
            }
            catch (Exception e)
            {
                logSource.LogError("Failed to replace: " + e);
            }

            return null;
        }

        // Moves and replaces a given object
        public static Transform ReplaceWithCostedPickup(GameObject replacedObject, string replacedItemName, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, Vector3 offset = new Vector3())
        {
            try
            {
                // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
                logSource.LogInfo("Pickup: " + replacedObject.name);

                // This logs where the pickup is; placed inside the if statement as the counterpart is after the position is updated in SpawnGenericItemPickup
                logSource.LogInfo("Pickup At: " + replacedObject.transform.position);

                UniqueID uniqueID = new UniqueID(replacedObject, replacedItemName);

                Transform output;

                // Attempts to spawn the replacement object
                logSource.LogInfo("Pickup Drop Attempt Start");
                output = SpawnGenericCostedPickup(uniqueID, replacedObject.transform, offset, currencyType, currencyAmount, requiredItems, itemAmounts);
                logSource.LogInfo("Pickup Drop Attempt End");

                HandleReplacedObject(replacedObject, replacedObject, output);

                return output;
            }
            catch (Exception e)
            {
                logSource.LogError("Failed to replace: " + e);
            }

            return null;
        }

        private static void HandleReplacedObject(GameObject replacedObject, GameObject activeParent, Transform output)
        {
            // Removes the original object, along with removing its gravity and collision
            // Note that scenes in this game only extend in postive x and y, so -250 -250 should be plenty out of the way
            replacedObject.transform.position = defaultReplacedParentLocation;
            activeParent.transform.position = defaultReplacedParentLocation;

            Rigidbody2D replacementRigidBody2D = replacedObject.GetComponent<Rigidbody2D>();
            if (replacementRigidBody2D != null)
            {
                Rigidbody2D outputRigidBody2D = output.GetComponent<Rigidbody2D>();
                if (outputRigidBody2D != null)
                {
                    outputRigidBody2D.gravityScale = replacementRigidBody2D.gravityScale;
                    outputRigidBody2D.constraints = replacementRigidBody2D.constraints;
                }

                replacementRigidBody2D.gravityScale = 0;
                replacementRigidBody2D.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            Collider2D replacementCollider2D = replacedObject.GetComponent<Collider2D>();
            if (replacementCollider2D != null)
            {
                replacementCollider2D.enabled = false;
            }

            output.parent = activeParent.transform;
        }

        public static void ReplaceGiantFleaPickup(Transform giantFlea, PlayMakerFSM giantFleaFSM, PlayMakerFSM __instance, GameObject fleaObject)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(fleaObject, GenericFleaItemName, genericItem);

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
    }
}
