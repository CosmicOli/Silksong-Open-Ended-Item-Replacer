using GlobalSettings;
using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public class CollectableItemPickup_Container : CollectableItemPickup_Abstract_Container, IInteractable
    {
        public static CollectableItemPickup_Container Prefab
        {
            get
            {
                SpawningReplacement = true;
                GameObject CollectableItemPickup_Container_GameObject = Instantiate(Gameplay.CollectableItemPickupPrefab).gameObject;
                SpawningReplacement = false;
                CollectableItemPickup_Container_GameObject.SetActive(false);
                return CollectableItemPickup_Container_GameObject.AddComponent<CollectableItemPickup_Container>();
            }
        }

        public InteractEvents InteractEvents
        {
            get
            {
                return Traverse.Create(CollectableItemPickupInstance).Field("interactEvents").GetValue<InteractEvents>();
            }
        }
    }
}
