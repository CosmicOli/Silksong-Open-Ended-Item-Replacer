using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class LugoliHandler
    {
        public static void Handle_Chef(PlayMakerFSM __instance)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.Fsm.Name == "Death" && __instance.gameObject.name.Contains("Corpse Roachkeeper Chef"))
            {
                FsmState splashIn = __instance.Fsm.GetState("Splash In");
                if (splashIn == null) { return; }

                GameObject dummyGameObject = new GameObject("Pickup");
                splashIn.Actions[9] = new GetCheck(dummyGameObject, "Pickled Roach Egg");
            }
        }
    }
}
