using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;

namespace Open_Ended_Item_Replacer.Core.Containers
{
    public interface IContainer
    {
        void Setup(UniqueID uniqueID);

        IGenericItem Item
        {
            get;
            set;
        }
    }
}
