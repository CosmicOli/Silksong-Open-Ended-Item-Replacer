using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class SilkHeartHandler
    {
        public static void HandleSilkHeart(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Silk Heart Memory Return" && __instance.gameObject?.name == "Silk Heart Memory Return")
            {
                string silkHeart = "Silk Heart";

                FsmState save = __instance.Fsm.GetState("Save");

                int numberOfNewActions = 2;

                FsmStateAction[] newActions = new FsmStateAction[save.Actions.Length + numberOfNewActions];

                GameObject dummyGameObject = new GameObject(silkHeart);
                newActions[0] = new GetCheck(dummyGameObject, silkHeart); // Replace
                newActions[1] = new RemoveExtraSilkHeart();

                //Array.Copy(save.Actions, 0, newActions, numberOfNewActions, save.Actions.Length);
                save.Actions = ReturnCombinedActions(newActions, save.Actions);

                //save.Actions = newActions;
            }
        }
    }
}
