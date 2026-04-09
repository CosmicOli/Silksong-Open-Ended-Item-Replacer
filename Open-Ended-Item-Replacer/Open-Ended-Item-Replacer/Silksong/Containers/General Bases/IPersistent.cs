using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_Ended_Item_Replacer.Silksong.Containers.General_Bases
{
    public interface IPersistent
    {
        PersistentBoolItem ContainerPersistentBoolItem
        {
            get;
        }
    }
}
