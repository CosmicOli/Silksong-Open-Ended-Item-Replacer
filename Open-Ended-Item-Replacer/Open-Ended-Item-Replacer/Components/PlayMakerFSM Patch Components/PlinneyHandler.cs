using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class PlinneyHandler
    {
        public static void HandlePlinney(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Plinney Inside")
            {
                FsmState upgradeState = __instance.Fsm.GetState("Upgrade State");
                FsmState setUpgrade1 = __instance.Fsm.GetState("Set Upgrade 1");
                FsmState setUpgrade2 = __instance.Fsm.GetState("Set Upgrade 2");
                FsmState furtherUpgrades = __instance.Fsm.GetState("Further Upgrades");
                FsmState completeRepeat = __instance.Fsm.GetState(" Complete Repeat"); // I kid you not, this state has a space at the beginning of its name, and yes I did have to spend time debugging to discover this lmao
                if (upgradeState == null || setUpgrade1 == null || setUpgrade2 == null || furtherUpgrades == null || completeRepeat == null) { return; }

                FsmInt storeValue = (upgradeState.Actions[0] as GetPlayerDataInt).storeValue; // Gets the variable responsible for tracking current needle upgrade

                upgradeState.Actions = new FsmStateAction[5];
                upgradeState.Actions[0] = new GetReplacedProgressiveLevel(1, 4, __instance.Fsm.Owner.name, "Needle Upgrade", storeValue); // Sets the variable responsible for tracking current needle upgrade by checking the progressive persistent bools; necessary for price and dialogue functionality
                upgradeState.Actions[1] = new SetFsmActiveState(__instance.Fsm, upgradeState, setUpgrade1, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 1")), GetFalseFunc()); // Upgrade 1
                upgradeState.Actions[2] = new SetFsmActiveState(__instance.Fsm, upgradeState, setUpgrade2, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 2")), GetFalseFunc()); // Upgrade 2
                upgradeState.Actions[3] = new SetFsmActiveState(__instance.Fsm, upgradeState, furtherUpgrades, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 4")), GetFalseFunc()); // Upgrade 3 and 4
                upgradeState.Actions[4] = new SetFsmActiveState(__instance.Fsm, upgradeState, completeRepeat, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(__instance.Fsm.Owner.name, "Needle Upgrade 4")), GetTrueFunc()); // All upgrades
            }
        }
    }
}
