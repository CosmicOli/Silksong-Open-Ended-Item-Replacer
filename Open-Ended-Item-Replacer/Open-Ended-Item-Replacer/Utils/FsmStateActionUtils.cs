using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open_Ended_Item_Replacer.Utils
{
    public class FsmStateActionUtils
    {
        public static FsmStateAction[] ReturnCombinedActions(FsmStateAction[] preActions, FsmStateAction[] postActions)
        {
            FsmStateAction[] replacementActions = new FsmStateAction[preActions.Length + postActions.Length];

            Array.Copy(preActions, 0, replacementActions, 0, preActions.Length);
            Array.Copy(postActions, 0, replacementActions, preActions.Length, postActions.Length);

            return replacementActions;
        }

        public static CheckQuestPdSceneBool SearchForCheckQuestPdSceneBool(FsmState state, string questTargetName)
        {
            List<CheckQuestPdSceneBool> actions = state?.Actions?.OfType<CheckQuestPdSceneBool>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                QuestTargetPlayerDataBools questTarget = (action.QuestTarget?.RawValue as QuestTargetPlayerDataBools);
                if (questTarget == null) { continue; }

                if (questTarget.name.Contains(questTargetName))
                {
                    return action;
                }
            }

            return null;
        }

        public static PlayerDataBoolTest SearchForPlayerDataBoolTest(FsmState state, string boolName)
        {
            List<PlayerDataBoolTest> actions = state?.Actions?.OfType<PlayerDataBoolTest>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                FsmString fsmBoolName = action?.boolName;
                if (fsmBoolName == null) { continue; }

                if (fsmBoolName.Value == boolName)
                {
                    return action;
                }
            }

            return null;
        }

        public static PlayerDataVariableTest SearchForPlayerDataVariableTest(FsmState state, string variableName)
        {
            List<PlayerDataVariableTest> actions = state?.Actions?.OfType<PlayerDataVariableTest>().ToList();
            if (actions == null) { return null; }

            foreach (var action in actions)
            {
                FsmString fsmVariableName = action?.VariableName;
                if (fsmVariableName == null) { continue; }

                if (fsmVariableName.Value == variableName)
                {
                    return action;
                }
            }

            return null;
        }
    }
}
