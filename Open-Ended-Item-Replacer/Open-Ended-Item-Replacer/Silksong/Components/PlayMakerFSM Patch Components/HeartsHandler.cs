using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class HeartsHandler
    {
        // First thing to note; Karmelita is just already fully working and handled, which is pretty nice
        // This does mean I would need to add a seperate karm flagger though, as without waking up from the memory the heart doesn't flag
        // This is also the case for Nyleth and Verdania from the looks of it

        public static void Handle_CoralHeart(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "activate memory on tool pickup" && __instance.gameObject?.name == "before")
            {
                FsmState state1 = __instance.Fsm.GetState("State 1");
                if (state1 == null) { return; }

                (state1.Actions[0] as ActivateGameObject).activate = true;
                (state1.Actions[1] as ActivateGameObject).activate = true;
                (state1.Actions[2] as SetFsmBool).setValue = true;
            }
        }
    }
}
