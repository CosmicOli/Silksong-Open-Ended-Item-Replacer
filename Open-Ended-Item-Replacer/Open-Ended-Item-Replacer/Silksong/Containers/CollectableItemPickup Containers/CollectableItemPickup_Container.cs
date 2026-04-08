using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Containers;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public class CollectableItemPickup_Container : CollectableItemPickup_Abstract_Container, IInteractable
    {
        public InteractEvents interactEvents
        {
            get
            {
                return Traverse.Create(CollectableItemPickupInstance).Field("interactEvents").GetValue<InteractEvents>();
            }
        }
    }
}
