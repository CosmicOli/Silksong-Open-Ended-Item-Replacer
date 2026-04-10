using System;
using UnityEngine;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.ReplaceUtils;

namespace Open_Ended_Item_Replacer.Core.Components.Replacement_Components
{
    public struct UniqueID
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

        [Obsolete]
        public UniqueID(string PickupName, string SceneName)
        {
            this.PickupName = PickupName;
            this.SceneName = SceneName;
        }

        // Preferred way of making a UniqueID, as it guarantees the correct current scene name is picked
        public UniqueID(GameObject replacedObject, string replacedItemName)
        {
            // Defining the unique id for the new pickup
            this.PickupName = replacedObject.name + "-" + replacedItemName + replacementFlag;
            this.SceneName = GameManager.GetBaseSceneName(replacedObject.scene.name);
        }

        // Use when you cannot make a game object in the correct scene
        public UniqueID(string replacedObjectName, string replacedObjectSceneName, string replacedItemName)
        {
            // Defining the unique id for the new pickup
            this.PickupName = replacedObjectName + "-" + replacedItemName + replacementFlag;
            this.SceneName = GameManager.GetBaseSceneName(replacedObjectSceneName);
        }

        public override string ToString()
        {
            return PickupName + "-" + SceneName;
        }
    }
}
