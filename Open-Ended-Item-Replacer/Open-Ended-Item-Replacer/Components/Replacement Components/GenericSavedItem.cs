using BepInEx.Logging;
using System;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Components
{
    // This object defines the item that replaces intended items
    // TODO: 
    // -> Show popup
    // -> Make functions make open ended requests
    public class GenericSavedItem : SavedItem
    {
        private UniqueID uniqueID;
        public UniqueID UniqueID
        {
            get
            {
                return uniqueID;
            }

            set
            {
                uniqueID = value;
                name = "Generic_Item-" + uniqueID.PickupName + "-" + uniqueID.SceneName;
            }
        }

        public PersistentBoolItem persistentBoolItem;

        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

            // Show popup (if showPopup)
            // Send get request
            persistentBoolItem.ItemData.Value = true;
            SceneData.instance.PersistentBools.SetValue(persistentBoolItem.ItemData);
            logSource.LogInfo("Item get:  " + persistentBoolItem.ItemData.ID + "  In Scene: " + persistentBoolItem.ItemData.SceneName);
        }

        public override bool CanGetMore()
        {
            return true;
        }

        public override Sprite GetPopupIcon()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.LogException(new NotImplementedException());
            }

            return null;
        }

        public override string GetPopupName()
        {
            if (Application.isPlaying)
            {
                //UnityEngine.Debug.LogException(new NotImplementedException());
                return persistentBoolItem.ToString();
            }

            return null;
        }
    }
}
