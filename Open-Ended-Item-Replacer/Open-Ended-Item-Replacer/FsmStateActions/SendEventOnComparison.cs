using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class SendEventOnComparison : SendEvent
    {
        Func<bool> comparisonFirstHalf;
        Func<bool> comparisonSecondHalf;

        public SendEventOnComparison(FsmEventTarget eventTarget, FsmEvent sendEvent, FsmFloat delay, bool everyFrame, Func<bool> comparisonFirstHalf, Func<bool> comparisonSecondHalf)
        {
            this.eventTarget = eventTarget;
            this.sendEvent = sendEvent;
            this.delay = delay;
            this.everyFrame = everyFrame;
            this.comparisonFirstHalf = comparisonFirstHalf;
            this.comparisonSecondHalf = comparisonSecondHalf;
        }

        public override void OnEnter()
        {
            if (comparisonFirstHalf.Invoke() == comparisonSecondHalf.Invoke())
            {
                base.OnEnter();
            }
        }
    }
}
