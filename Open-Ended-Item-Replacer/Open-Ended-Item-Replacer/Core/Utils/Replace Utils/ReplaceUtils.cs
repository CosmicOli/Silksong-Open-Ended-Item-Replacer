using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.Components;
using Open_Ended_Item_Replacer.Silksong.Components.Grant_Components;
using Open_Ended_Item_Replacer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches.Awake;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.SpawnUtils;
using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;

namespace Open_Ended_Item_Replacer.Core.Utils.Replace_Utils
{
    internal class ReplaceUtils
    {
        public static string replacementFlag = "-(Replacement)";

        // Moves and replaces a given object
        public static Transform Core_Replace<Container, InteractableContainer, CollisionableContainer>(GameObject replacedObject, string replacedItemName, bool interactable, Vector3 offset = new Vector3())
            where Container : MonoBehaviour, IContainer
            where InteractableContainer : MonoBehaviour, IContainer, IInteractable
            where CollisionableContainer : MonoBehaviour, IContainer, ICollisionable
        {
            return Core_Replace<Container, InteractableContainer, CollisionableContainer>(replacedObject, replacedObject, replacedItemName, interactable, offset);
        }

        // Moves and replaces a given object
        public static Transform Core_Replace<Container, InteractableContainer, CollisionableContainer>(GameObject replacedObject, GameObject activeParent, string replacedItemName, bool interactable, Vector3 offset = new Vector3())
            where Container : MonoBehaviour, IContainer
            where InteractableContainer : MonoBehaviour, IContainer, IInteractable
            where CollisionableContainer : MonoBehaviour, IContainer, ICollisionable
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
                    output = SpawnGenericPickup<InteractableContainer>(interactable, uniqueID, replacedObject.transform, offset);
                }
                else
                {
                    output = SpawnGenericPickup<InteractableContainer>(interactable, uniqueID, replacedObject.transform, offset);
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
        public static Transform Core_ReplaceWithCostedPickup<CostedContainer>(GameObject replacedObject, string replacedItemName, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, Vector3 offset = new Vector3())
            where CostedContainer : MonoBehaviour, IContainer, IInteractable, ICosted
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
                output = SpawnGenericCostedPickup<CostedContainer>(uniqueID, replacedObject.transform, offset, currencyType, currencyAmount, requiredItems, itemAmounts);
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
    }
}
