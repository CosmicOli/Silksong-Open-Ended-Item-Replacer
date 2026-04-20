using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Containers.Flea_Containers.Bases
{
    public abstract class Flea_Abstract_Container : PersistentContainer
    {
        public abstract PlayMakerFSM FleaRescueActivation
        {
            // Not all copied fleas have the same fsm name for the same flea activation functionality, so this is handled in each individual container
            get;
        }

        private IGenericItem item;
        public override IGenericItem Item
        {
            get
            {
                return item;
            }
            set
            {
                item = value;
                SetFleaRescueActivationItem();
            }
        }

        public void SetFleaRescueActivationItem()
        {
            FleaRescueActivation.Fsm.GetState("End").Actions[0] = new GetCheck(Item);
        }

        public override void Setup(UniqueID uniqueID)
        {
            gameObject.AddComponent<PersistentBoolItem>();

            base.Setup(uniqueID);

            // Replaces the regular flea item get with the correct generic item
            SetFleaRescueActivationItem();
        }

        public override PersistentBoolItem ContainerPersistentBoolItem
        {
            get
            {
                return gameObject.GetComponent<PersistentBoolItem>();
            }
        }
    }
}
