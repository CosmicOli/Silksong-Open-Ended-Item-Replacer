using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class MossDruidHandler
    {
        public static void HandleMossDruid(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Conversation Control" && __instance.gameObject?.name == "Moss Creep NPC")
            {
                FsmState choice = __instance.Fsm.GetState("Choice");
                FsmState ingredientConvo = __instance.Fsm.GetState("Ingredient Convo");
                if (choice == null || ingredientConvo == null) { return; }

                choice.Actions[5].Enabled = false;
                choice.Actions[6].Enabled = false;
                choice.Actions[7].Enabled = false;
                choice.Actions[8] = new SetFsmActiveState(__instance.Fsm, choice, ingredientConvo, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Moss Creep NPC", "Mossberry Stew")), GetFalseFunc());
            }
        }
    }
}
