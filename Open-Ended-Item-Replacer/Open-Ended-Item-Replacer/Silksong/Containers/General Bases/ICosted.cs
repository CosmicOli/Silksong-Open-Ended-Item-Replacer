using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System;

namespace Open_Ended_Item_Replacer.Silksong.Containers.General_Bases
{
    public interface ICosted : IInteractable
    {
        // It is recommended to call SpawnUtils.OnCancel() in any implementation of this
        void OnCancel();

        void InteractSetup();

        Action Yes
        {
            get;
        }

        Action No
        {
            get;
        }
    }
}
