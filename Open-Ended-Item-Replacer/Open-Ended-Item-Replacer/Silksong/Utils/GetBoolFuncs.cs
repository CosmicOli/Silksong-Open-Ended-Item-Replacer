using System;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    public class GetBoolFuncs
    {
        public static Func<bool> GetTrueFunc()
        {
            bool GetTrue() { return true; }
            return GetTrue;
        }

        public static Func<bool> GetFalseFunc()
        {
            bool GetFalse() { return false; }
            return GetFalse;
        }

        public static Func<bool> GetPersistentBoolFromDataFunc(PersistentItemData<bool> persistentData)
        {
            bool GetBool() { return GetPersistentBoolFromData(persistentData); }

            return GetBool;
        }

        public static Func<bool> GetPlayerDataBoolFunc(string playerDataBool)
        {
            bool GetBool() { return GetPlayerDataBool(playerDataBool); }

            return GetBool;
        }
    }
}
