using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using System.ComponentModel;
using System.Xml.Linq;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public class CollectableItemPickup_Container : CollectableItemPickup_Abstract_Container, IInteractable
    {
        public InteractEvents InteractEvents
        {
            get
            {
                return Traverse.Create(CollectableItemPickupInstance).Field("interactEvents").GetValue<InteractEvents>();
            }
        }
    }
}
