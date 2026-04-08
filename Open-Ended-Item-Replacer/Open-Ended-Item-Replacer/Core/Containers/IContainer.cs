using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;

namespace Open_Ended_Item_Replacer.Core.Containers
{
    public interface IContainer
    {
        // A container needs to be able to handle setting itself up on creation
        // It must have an getter for the container's persistence, which is necessary as this needs to be attached to an object and is frequently part of a spawned prefab for an object
        // It must have a generic item
        void Setup();

        PersistentBoolItem ContainerPersistentBoolItem
        {
            get;
        }

        GenericSavedItem Item
        {
            get;
            set;
        }
    }
}
