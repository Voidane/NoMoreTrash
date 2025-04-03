using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Il2CppScheduleOne.Interaction;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Trash;
using MelonLoader;
using UnityEngine;

namespace NoMoreTrash
{
    public class NoMoreTrashMod : MelonMod
    {
        public static readonly string MOD_PREFIX = "[NoMoreTrash]";

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg($"{MOD_PREFIX} Initializing, Created by Voidane.");
            MelonLogger.Msg($"{MOD_PREFIX} Discord: discord.gg/XB7ruKtJje");
            HarmonyPatches();
            MelonLogger.Msg($"{MOD_PREFIX} Has been initialized...");
        }

        private void HarmonyPatches()
        {
            HarmonyLib.Harmony patcher = new HarmonyLib.Harmony("com.voidane.nomoretrash");

            patcher.Patch(typeof(TrashItem).GetMethod("Start"), null, new HarmonyLib.HarmonyMethod(
                typeof(NoMoreTrashMod).GetMethod(nameof(Patch_TrashItem_Start), BindingFlags.Static | BindingFlags.NonPublic)),
                null, null, null);
        }

        private static void Patch_TrashItem_Start(TrashItem __instance)
        {
            __instance.DestroyTrash();
        }
    }
}
