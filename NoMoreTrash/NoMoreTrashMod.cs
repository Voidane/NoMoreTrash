using System;
using System.IO;
using System.Reflection;
using Il2CppScheduleOne.Trash;
using MelonLoader;
using UnityEngine;
using System.Collections;
using Il2CppSteamworks;
using Il2Cpp;
using System.Net.Http;

namespace NoMoreTrash
{
    public class NoMoreTrashMod : MelonMod
    {
        public static TrashManager trashManager;
        public static ConfigData configData;

        private const string versionCurrent = "1.0.1";
        private const string versionMostUpToDateURL = "https://raw.githubusercontent.com/Voidane/NoMoreTrash/refs/heads/master/NoMoreTrash/Version.txt";
        private string versionUpdate = null;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg($"===========================================");
            MelonLogger.Msg($"Initializing, Created by Voidane.");
            MelonLogger.Msg($"Discord: discord.gg/XB7ruKtJje");

            configData = new ConfigData();
            HarmonyPatches();
            CheckForUpdates();
        }

        private async void CheckForUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync(versionMostUpToDateURL);
                    versionUpdate = content.Trim();
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"Could not fetch most up to date version {e.Message}");
            }

            if (versionCurrent != versionUpdate)
            {
                MelonLogger.Msg($"New Update for no trash mod! https://www.nexusmods.com/schedule1/mods/221?tab=files, Current: {versionCurrent}, Update: {versionUpdate}");
            }

            MelonLogger.Msg($"Has been initialized...");
            MelonLogger.Msg($"===========================================");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                MelonLogger.Msg("Main scene loaded!");
                MelonCoroutines.Start(WaitOnSceneLoad(null, "Managers", 20.0F, (managers) =>
                {
                    MelonCoroutines.Start(WaitOnSceneLoad(managers, "@Trash", 5.0F, (tm) =>
                    {
                        if (tm.TryGetComponent<TrashManager>(out TrashManager _))
                        {
                            trashManager = TrashManager.Instance;
                        }
                    }
                    ));
                }));
            }
        }

        private IEnumerator WaitOnSceneLoad(Transform parent, string name, float timeoutLimit, Action<Transform> onComplete)
        {
            Transform target = null;
            float timeOutCounter = 0F;
            int attempt = 0;

            MelonLogger.Msg($"Looking for {name} inside of {(parent == null ? "Hierarchy" : parent.gameObject.name)}");
            while (target == null && timeOutCounter < timeoutLimit)
            {
                target = (parent == null) ? GameObject.Find(name).transform : parent.Find(name);
                if (target == null)
                {
                    timeOutCounter += 0.5F;
                    yield return new WaitForSeconds(0.5F);
                }
            }

            if (target != null)
            {
                onComplete?.Invoke(target);
            }
            else
            {
                MelonLogger.Error("Failed to find target object within timeout period!");
                onComplete?.Invoke(null);
            }

            yield return target;
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

            if (__instance.transform.parent.gameObject.name.Contains("_Temp"))
            {
                if (!configData.TrashItems.TryGetValue(__instance.ID, out bool value))
                {
                    MelonLogger.Error($"could not find: {__instance.ID} in config");
                    MelonLogger.Error($"This might be a new or custom item that needs to added to the config!");
                    MelonLogger.Error($"Report here: discord.gg/XB7ruKtJje");
                }

                if (value)
                {
                    __instance.DestroyTrash();
                }
            }
        }
    }
}
