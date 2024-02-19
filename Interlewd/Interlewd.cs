using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;


[assembly: AssemblyVersion(COM3D2.Interlewd.PluginInfo.PLUGIN_VERSION + ".*")]
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace COM3D2.Interlewd
{
    public static class PluginInfo
    {
        // The name of this assembly.
        public const string PLUGIN_GUID = "COM3D2.Interlewd";
        // The name of this plugin.
        public const string PLUGIN_NAME = "Interlewd";
        // The version of this plugin.
        public const string PLUGIN_VERSION = "1.1";
    }
}



namespace COM3D2.Interlewd
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public sealed class Interlewd : BaseUnityPlugin
    {
        public static Interlewd Instance { get; private set; }
        internal static new ManualLogSource Logger => Instance?._Logger;
        private ManualLogSource _Logger => base.Logger;


        private void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(Interlewd));
        }

        [HarmonyPatch(typeof(ScriptManager), nameof(ScriptManager.ReplacePersonal))]
        [HarmonyPatch(new Type[] { typeof(Maid[]), typeof(string) })]
        [HarmonyPostfix]
        private static void ReplacePersonal_sufix(Maid[] maid_array, string text, ref string __result)
        {
            if (__result.EndsWith(".ks"))
            {
                string script = __result.ToLower();

                // check if a script exists 
                if (!GameUty.FileSystem.IsExistentFile(script))
                {
                    if(Instance.replacementDic.ContainsKey(script))
                    {
                        __result = Instance.replacementDic[script];
                    }
                    else
                    {
                        //recovering all scripts the game has.
                        Instance.scripts ??= GameUty.FileSystem.GetFileListAtExtension(".ks");

                        //get script components
                        string scriptBase = script.Substring(script.IndexOf('_'));
                        string personalityToken = script.Substring(0, script.IndexOf('_'));

                        //get potential replacement scripts
                        string[] validScripts = Instance.scripts.Where(s => s.EndsWith(scriptBase)).Select(s => Path.GetFileName(s)).ToArray();                          

                        //get a list of personalities that can access this script in normal circumstances.
                        List<string> validPersonalities = new();
                        foreach (string ks in validScripts)
                        {
                            string token = ks.Substring(0, ks.IndexOf('_'));
                            validPersonalities.Add(token);
                        }

                        //Add a backup in case nothing fits
                        if (validScripts.Length == 0)
                        {
                            if (script.Contains("FT"))
                                validScripts = new string[] { "a1_ft_00003.ks" };
                            else
                                validScripts = new string[] { "a1_job_00001.ks" };
                        }

                        //select a random script from the potential ones
                        int rand = UnityEngine.Random.Range(0, validPersonalities.Count);
                        __result = $"{validScripts[rand]}".ToUpper();

                        //Add to replacement dic for consistant replacement
                        Instance.replacementDic.Add(script, __result);

                        //Trying to explain what's going on to the end user.
                        Logger.LogWarning($"■■■■■■■■■■■■■■■■■■■■");

                        Logger.LogError($"{script.ToUpper()} doesn't exist.");
                        Logger.LogWarning($"This yotogi or event is currently NOT available for {Instance.tokens[personalityToken]}");

                        string mess = "";
                        if (validPersonalities.Count > 0)
                        {
                            foreach (string personality in validPersonalities)
                                mess += $"{Instance.tokens[personality]}, ";

                            Logger.LogWarning($"This yotogi or event is currently ONLY available for {mess}");
                        }
                        else
                            Logger.LogWarning($"This yotogi or event isn't available for any personality");

                        Logger.LogWarning($"To avoid a game freeze, Interlewd will use {__result} until next game restart.");
                        Logger.LogWarning($"VOICE and BEHAVIOUR will NOT match your currently selected maid's personality, this is expected!");

                        Logger.LogWarning($"■■■■■■■■■■■■■■■■■■■■");
                    }
                }
            }
        }


        private string[] scripts = null;

        private readonly Dictionary<string, string> replacementDic = new();

        private readonly Dictionary<string, string> tokens = new()
        {
            {"a", "Tsundere" },
            {"b", "Kuudere" },
            {"c", "Pure" },
            {"d", "Yandere" },
            {"e","Onee-chan" },
            {"f","Genki" },
            {"g","Sadistic Queen" },
            {"a1","Muku" },
            {"b1","Majime" },
            {"c1","Rindere" },
            {"d1","Bookworm" },
            {"e1","Koakuma" },
            {"f1","Ladylike" },
            {"g1","Secretary" },
            {"h1","Imouto" },
            {"j1","Wary" },
            {"k1","Ojousama" },
            {"l1","Osananajimi" },
            {"m1","Masochist" },
            {"n1","Haraguro" },
            {"p1","Gyaru" },
            {"v1","Kimajime" },
            {"w1","Kisakude" }
        };
    }
}
