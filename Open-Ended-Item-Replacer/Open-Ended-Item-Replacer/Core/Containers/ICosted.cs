using System;

namespace Open_Ended_Item_Replacer.Core.Containers
{
    public interface ICosted
    {
        // It is recommended to call SpawnUtils.OnCancel() in any implementation of this
        void OnCancel();

        void SpawnSetup();

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
