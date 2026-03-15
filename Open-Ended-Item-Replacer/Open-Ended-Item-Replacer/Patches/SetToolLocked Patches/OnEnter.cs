using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_Ended_Item_Replacer.Patches.SetToolLocked_Patches
{
    [HarmonyPatch(typeof(SetToolLocked), "OnEnter")]
    internal class OnEnter
    {
        // Handles when FSMs run SetToolLocked
        // Stops NPCs locking tools when not actually replacing them
        private static bool Prefix(SetToolLocked __instance)
        {
            /*string name = ((ToolItem)__instance.Tool.Value).name;
            if (name == "Silk Snare" || name == "Extractor")
            {
                return true;
            }
            else
            {
                __instance.Finish();
                return false;
            }*/

            __instance.Finish();
            return false;
        }
    }
}
