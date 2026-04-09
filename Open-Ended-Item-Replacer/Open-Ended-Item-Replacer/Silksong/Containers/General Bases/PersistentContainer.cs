using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Core.Containers;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.InfoUtils;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Containers.General_Bases
{
    public abstract class PersistentContainer : MonoBehaviour, IContainer, IPersistent
    {
        public abstract PersistentBoolItem ContainerPersistentBoolItem
        {
            get;
        }

        public abstract IGenericItem Item
        {
            get;
            set;
        }

        public virtual void Setup(UniqueID uniqueID)
        {
            SetGenericPickupInfo(uniqueID, this);
        }
    }
}
