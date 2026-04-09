using GlobalSettings;
using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases
{
    public abstract class CollectableItemPickup_Abstract_Container : PersistentContainer
    {
        public CollectableItemPickup CollectableItemPickupInstance
        {
            get { return gameObject.GetComponent<CollectableItemPickup>(); }
        }

        // In this class, there are two items that represent basically the same thing
        // You can desync them to make the collectableItemPickup function on a different/dummy item, e.g. a fake collectable when you need granting to be handled seperately
        private IGenericItem item;
        public override IGenericItem Item
        {
            get
            {
                return item;
            }
            set
            {
                if (value as SavedItem)
                {
                    CollectableItemPickupInstance.SetItem(value as SavedItem, true);
                }
                else
                {
                    logSource.LogWarning("Value provided was not a SavedItem or derivate, only assigning item and not collectableItemPickupInstance.Item");
                }

                item = value;
            }
        }

        // This allows for desyncing Item and the collectableItemPickup.Item
        public void SetCollectableItemPickupItem(SavedItem item, bool keepPersistence = false)
        {
            CollectableItemPickupInstance.SetItem(item, keepPersistence);
        }

        public override void Setup(UniqueID uniqueID)
        {
            base.Setup(uniqueID);

            // Ignores areas that disallow replacement pickups such that it can exist anyways
            Traverse.Create(CollectableItemPickupInstance).Field("ignoreCanExist").SetValue(true);

            // Ensures new replacements made post-pickup don't reset the persistent data
            Traverse.Create(CollectableItemPickupInstance).Field("activatedSave").SetValue(true);
        }

        public override PersistentBoolItem ContainerPersistentBoolItem
        {
            get
            {
                return Traverse.Create(CollectableItemPickupInstance).Field("persistent").GetValue<PersistentBoolItem>();
            }
        }
    }
}
