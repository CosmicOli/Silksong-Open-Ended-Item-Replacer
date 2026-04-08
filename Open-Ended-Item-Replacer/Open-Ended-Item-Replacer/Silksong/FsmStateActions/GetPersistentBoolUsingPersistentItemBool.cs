using HutongGames.PlayMaker;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.FsmStateActions
{
    public class GetPersistentBoolUsingPersistentItemBool : FsmStateAction
    {
        PersistentItemData<bool> persistence;
        FsmBool storeResult;

        public GetPersistentBoolUsingPersistentItemBool(PersistentItemData<bool> persistence, FsmBool storeResult)
        {
            this.persistence = persistence;
            this.storeResult = storeResult;
        }

        public override void OnEnter()
        {
            storeResult.Value = GetPersistentBoolFromData(persistence);
        }
    }
}
