using System;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Components
{
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

        [Obsolete]
        public UniqueID(string PickupName, string SceneName)
        {
            this.PickupName = PickupName;
            this.SceneName = SceneName;
        }

        public UniqueID(GameObject replacedObject, string replacedItemName)
        {
            // Defining the unique id for the new pickup
            this.PickupName = replacedObject.name + "-" + replacedItemName + Open_Ended_Item_Replacer.replacementFlag;
            this.SceneName = GameManager.GetBaseSceneName(replacedObject.scene.name);
        }

        public override string ToString()
        {
            return PickupName + "-" + SceneName;
        }
    }
}
