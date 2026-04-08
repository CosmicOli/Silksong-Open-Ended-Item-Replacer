using GlobalSettings;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using System.Reflection;

namespace Open_Ended_Item_Replacer.Silksong.Components.Grant_Components
{
    internal class MaskShardGranter
    {
        public static void TryGrant_MaskShard(string functionID)
        {
            if (functionID == nameof(TryGrant_MaskShard))
            {
                Grant_MaskShard();
            }
        }

        public static void Grant_MaskShard()
        {
            GameObject.Instantiate(HeartPieceInstant, HeroController.instance.transform.position, HeroController.instance.transform.rotation);
        }
    }
}
