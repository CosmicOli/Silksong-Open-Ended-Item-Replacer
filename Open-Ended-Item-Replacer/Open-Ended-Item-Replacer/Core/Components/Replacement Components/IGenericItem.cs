using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Open_Ended_Item_Replacer.Core.Components.Replacement_Components
{
    public interface IGenericItem
    {
        UniqueID UniqueID
        {
            get;
            set;
        }

        PersistentBoolItem PersistentBoolItem
        {
            get;
            set;
        }

        void Get(bool showPopup = true);
    }
}
