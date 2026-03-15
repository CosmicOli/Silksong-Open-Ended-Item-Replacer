using HutongGames.PlayMaker;
using UnityEngine;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class ReplacePickup : FsmStateAction
    {
        GameObject gameObject;
        string itemName;

        public ReplacePickup(GameObject gameObject, string itemName)
        {
            this.gameObject = gameObject;
        }

        public override void OnEnter()
        {
            Open_Ended_Item_Replacer.Replace(gameObject, itemName, true, null);

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
