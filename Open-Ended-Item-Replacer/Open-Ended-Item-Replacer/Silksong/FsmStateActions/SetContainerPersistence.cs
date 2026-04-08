using HutongGames.PlayMaker;

namespace Open_Ended_Item_Replacer.Silksong.FsmStateActions
{
    public class SetContainerPersistence : FsmStateAction
    {
        PersistentItemData<bool> persistentBoolData;

        public SetContainerPersistence(PersistentItemData<bool> persistentBoolData)
        {
            this.persistentBoolData = persistentBoolData;
        }

        public override void OnEnter()
        {
            persistentBoolData.Value = true;
            SceneData.instance.PersistentBools.SetValue(persistentBoolData);

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
