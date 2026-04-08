using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using QuestPlaymakerActions;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using HutongGames.PlayMaker.Actions;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class MossDruidHandler
    {
        public static void Handle_MossDruid(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Conversation Control" && __instance.gameObject?.name == "Moss Creep NPC")
            {
                FsmState choice = __instance.Fsm.GetState("Choice");
                FsmState ingredientConvo = __instance.Fsm.GetState("Ingredient Convo");
                FsmState tradeCheck = __instance.Fsm.GetState("Trade Check");
                FsmState finalBerry = __instance.Fsm.GetState("Final Berry");
                if (choice == null || ingredientConvo == null) { return; }

                choice.Actions[5].Enabled = false;
                choice.Actions[6].Enabled = false;
                choice.Actions[7].Enabled = false;
                choice.Actions[8] = new SetFsmActiveState(__instance.Fsm, choice, ingredientConvo, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Moss Creep NPC", "Mossberry Stew")), GetFalseFunc());

                PlayerdataIntCompare playerdataIntCompare = new PlayerdataIntCompare();
                playerdataIntCompare.compareTo = 4;
                playerdataIntCompare.playerdataInt = "druidMossBerriesSold";
                playerdataIntCompare.equal = FsmEvent.GetFsmEvent("COMPLETE");
                playerdataIntCompare.greaterThan = FsmEvent.GetFsmEvent("COMPLETE");
                tradeCheck.Actions[0] = playerdataIntCompare;

                PlayerDataIntAdd playerDataIntAdd = new PlayerDataIntAdd();
                playerDataIntAdd.intName = "druidMossBerriesSold";
                playerDataIntAdd.amount = 1;
                finalBerry.Actions = ReturnCombinedActions(new FsmStateAction[] { playerDataIntAdd }, finalBerry.Actions);
            }
        }
    }
}
