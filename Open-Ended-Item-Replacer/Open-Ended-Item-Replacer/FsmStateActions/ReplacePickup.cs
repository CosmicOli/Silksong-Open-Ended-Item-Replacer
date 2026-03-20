using HutongGames.PlayMaker;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.ReplaceUtils;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class ReplacePickup : FsmStateAction
    {
        GameObject gameObject;
        string itemName;

        public ReplacePickup(GameObject gameObject, string itemName)
        {
            this.gameObject = gameObject;
            this.itemName = itemName;
        }

        public override void OnEnter()
        {
            Replace(gameObject, itemName, true);

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
