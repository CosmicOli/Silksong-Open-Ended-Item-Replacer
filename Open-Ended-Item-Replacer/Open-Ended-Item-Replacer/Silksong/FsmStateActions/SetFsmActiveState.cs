using HarmonyLib;
using HutongGames.PlayMaker;
using System;

namespace Open_Ended_Item_Replacer.Silksong.FsmStateActions
{
    public class SetFsmActiveState : FsmStateAction
    {
        bool[] cachedEnabled;
        bool revert = false;

        Fsm fsm;
        FsmState oldState;
        FsmState newState;
        Func<bool> comparisonFirstHalf;
        Func<bool> comparisonSecondHalf;
        bool blockRemainingActions = false;

        public SetFsmActiveState(Fsm fsm, FsmState oldState, FsmState newState, bool blockRemainingActions = true)
        {
            bool getTrue() { return true; }

            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.comparisonFirstHalf = getTrue;
            this.comparisonSecondHalf = getTrue;
            this.blockRemainingActions = blockRemainingActions;
        }

        public SetFsmActiveState(Fsm fsm, FsmState newState)
        {
            bool getTrue() { return true; }

            this.fsm = fsm;
            this.newState = newState;
            this.comparisonFirstHalf = getTrue;
            this.comparisonSecondHalf = getTrue;
        }

        public SetFsmActiveState(Fsm fsm, FsmState newState, Func<bool> comparisonFirstHalf, Func<bool> comparisonSecondHalf)
        {
            this.fsm = fsm;
            this.newState = newState;
            this.comparisonFirstHalf = comparisonFirstHalf;
            this.comparisonSecondHalf = comparisonSecondHalf;
        }

        public SetFsmActiveState(Fsm fsm, FsmState oldState, FsmState newState, Func<bool> comparisonFirstHalf, Func<bool> comparisonSecondHalf, bool blockRemainingActions = true)
        {
            this.fsm = fsm;
            this.oldState = oldState;
            this.newState = newState;
            this.comparisonFirstHalf = comparisonFirstHalf;
            this.comparisonSecondHalf = comparisonSecondHalf;
            this.blockRemainingActions = blockRemainingActions;
        }

        private void HandleBlockRemainingActions()
        {
            if (revert)
            {
                FsmStateAction[] dummyArray = new FsmStateAction[cachedEnabled.Length];
                Array.Copy(oldState.Actions, dummyArray, dummyArray.Length);
                oldState.Actions = dummyArray;

                for (int i = 0; i < cachedEnabled.Length; i++)
                {
                    oldState.Actions[i].Enabled = cachedEnabled[i];
                }

                revert = false;

                Active = false;
                Finished = true;
                Finish();
            }
            else
            {
                if (comparisonFirstHalf.Invoke() == comparisonSecondHalf.Invoke())
                {
                    int length = oldState.Actions.Length;
                    cachedEnabled = new bool[length];
                    for (int i = 0; i < length; i++)
                    {
                        cachedEnabled[i] = oldState.Actions[i].Enabled;
                    }

                    FsmStateAction[] cachedActions = new FsmStateAction[length];
                    Array.Copy(oldState.Actions, cachedActions, length);
                    oldState.Actions = new FsmStateAction[length + 1];
                    Array.Copy(cachedActions, oldState.Actions, length);
                    revert = true;

                    for (int i = 0; i < length; i++)
                    {
                        oldState.Actions[i].Enabled = false;
                    }

                    Enabled = true;
                    oldState.Actions[length] = this;

                    Traverse.Create(fsm).Field("activeState").SetValue(newState);
                    Traverse.Create(fsm).Field("activeStateName").SetValue(newState.Name);
                    fsm.Start();
                }
                Finish();
            }
        }

        public override void OnEnter()
        {
            if (blockRemainingActions)
            {
                HandleBlockRemainingActions();
                return;
            }

            if (comparisonFirstHalf.Invoke() == comparisonSecondHalf.Invoke())
            {
                fsm.SwitchState(newState);

                //Traverse.Create(fsm).Field("activeState").SetValue(newState);
                //Traverse.Create(fsm).Field("activeStateName").SetValue(newState.Name);
                //fsm.Start();
            }

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
