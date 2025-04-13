using System;
using System.Collections;
using System.Net.Http;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using ScheduleOne.Trash;
using UnityEngine;

namespace NoMoreTrashMono
{
    public class NoMoreTrash : MelonMod
    {

        public static TrashManager trashManager;
        public static ConfigData configData;

        private const string versionCurrent = "1.0.1";
        private const string versionMostUpToDateURL = "https://raw.githubusercontent.com/Voidane/NoMoreTrash/refs/heads/Mono/NoMoreTrashMono/Version.txt";
        private const string githubBranchURL = "https://github.com/Voidane/NoMoreTrash/tree/Mono";
        private const string nexusOrThunderURL = "https://www.nexusmods.com/schedule1/mods/221?tab=files or thunderstore link";
        private string versionUpdate = null;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg($"===========================================");
            MelonLogger.Msg($"Initializing No Trash Mod (MONO) {versionCurrent}");
            MelonLogger.Msg($"Discord: discord.gg/XB7ruKtJje");

            configData = new ConfigData();
            new HarmonyLib.Harmony("com.voidane.nomoretrashmono").PatchAll();
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
                MelonLogger.Msg($"New Update for no trash mod (MONO)! {nexusOrThunderURL}, Current: {versionCurrent}, Update: {versionUpdate}");
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

        #region Harmony
        [HarmonyPatch(typeof(TrashItem), "Start")]
        public static class Patch_TrashItem_Destroying
        {
            [HarmonyPostfix]
            public static void Postfix(TrashItem __instance)
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
        #endregion
    }
}
