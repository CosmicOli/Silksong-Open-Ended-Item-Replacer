using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class FaydownHandler
    {
        public static void Handle_FaydownCloak(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "DJ Get Sequence" && __instance.gameObject?.name == "DJ Get Sequence")
            {
                FsmState hasDJ = __instance.Fsm.GetState("Has DJ?");
                FsmState startBlizzardAudio = __instance.Fsm.GetState("Start Blizzard Audio");
                FsmState completed = __instance.Fsm.GetState("Completed");
                FsmState breakTuningFork = __instance.Fsm.GetState("Break Tuning Fork");

                if (hasDJ == null || startBlizzardAudio == null || completed == null || breakTuningFork == null) { return; }

                string faydownCloak = "Faydown Cloak";

                hasDJ.Actions = new FsmStateAction[2];
                hasDJ.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasDJ, startBlizzardAudio, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(faydownCloak, faydownCloak)), GetFalseFunc());
                hasDJ.Actions[1] = new SetFsmActiveState(__instance.Fsm, hasDJ, completed, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(faydownCloak, faydownCloak)), GetTrueFunc());

                GameObject dummyGameObject = new GameObject(faydownCloak);
                breakTuningFork.Actions[3] = new GetCheck(dummyGameObject, faydownCloak);
            }
        }
    }
}
