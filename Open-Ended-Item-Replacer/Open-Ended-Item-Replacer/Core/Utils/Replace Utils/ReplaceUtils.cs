using System;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.SpawnUtils;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;

namespace Open_Ended_Item_Replacer.Core.Utils.Replace_Utils
{
    internal class ReplaceUtils
    {
        public static string replacementFlag = "-(Replacement)";

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, string replacedItemName, Vector3 offset = new Vector3())
        {
            return Replace(replacedObject, replacedObject, replacedItemName, offset);
        }

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, GameObject activeParent, string replacedItemName, Vector3 offset = new Vector3())
        {
            try
            {
                if (!LoadGameRunPatched)
                {
                    logSource.LogInfo("Attempted to replace while !LoadGameRunPatched, returning null");
                    return null;
                }

                logSource.LogInfo("Pickup: " + replacedObject.name);
                logSource.LogInfo("Pickup At: " + replacedObject.transform.position);

                UniqueID uniqueID = new UniqueID(replacedObject, replacedItemName);

                Transform output;

                // Attempts to spawn the replacement object
                logSource.LogInfo("Pickup Drop Attempt Start");
                output = SpawnGenericPickup(BarrelFleaContainer, uniqueID, replacedObject.transform, offset);
                logSource.LogInfo("Pickup Drop Attempt End");

                if (output == null) { return output; }

                HandleReplacedObject(replacedObject, activeParent, output);

                return output;
            }
            catch (Exception e)
            {
                logSource.LogError("Failed to replace: " + e);
            }

            return null;
        }

        public static void HandleReplacedObject(GameObject replacedObject, GameObject activeParent, Transform output)
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
