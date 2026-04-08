using HutongGames.PlayMaker;

namespace Open_Ended_Item_Replacer.Silksong.FsmStateActions
{
    public class TestAction : FsmStateAction
    {
        public override void OnEnter()
        {
            Open_Ended_Item_Replacer.logSource.LogWarning("action ran");
            Open_Ended_Item_Replacer.logSource.LogInfo(this.State.Name);

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
