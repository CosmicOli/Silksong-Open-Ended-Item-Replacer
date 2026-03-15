using HutongGames.PlayMaker;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class RemoveExtraSilkHeart : FsmStateAction
    {
        public override void OnEnter()
        {
            HeroController.instance.AddToMaxSilkRegen(-1);

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
