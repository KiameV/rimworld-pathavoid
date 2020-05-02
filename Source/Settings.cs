﻿using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PathAvoid
{
    public class SettingsController : Mod
    {
        public static bool AdvancedModeEnabled = false;
        public static Dictionary<string, string> PathAvoidDefNameValue = new Dictionary<string, string>();

        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Path Avoid";
        }

        public static byte GetBaseGridValue()
        {
            foreach (PathAvoidDef d in DefDatabase<PathAvoidDef>.AllDefs)
            {
                if (d.defName.Equals("PathAvoidNormal"))
                {
                    return (byte)d.level;
                }
            }

            if (PathAvoidDefNameValue.TryGetValue("Normal", out string s))
            {
                if (byte.TryParse(s, out byte v))
                {
                    return v;
                }
            }

            return 0;
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            GUI.BeginGroup(new Rect(0, 60, 600, 400));
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, 300, 40), "\"Pathing\" Button Location");
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(0, 60, 150, 22), "Top"))
            {
                Settings.ButtonLocationString = "1000";
                Settings.ApplyLocation();
                Messages.Message("\"Pathing\" Button will be at the top (requires a re-load of a save).", MessageTypeDefOf.PositiveEvent);
            }
            if (Widgets.ButtonText(new Rect(175, 60, 150, 22), "Bottom"))
            {
                Settings.ButtonLocationString = "0";
                Settings.ApplyLocation();
                Messages.Message("\"Pathing\" Button will be at the bottom (requires a re-load of a save).", MessageTypeDefOf.PositiveEvent);
            }

            Widgets.Label(new Rect(0, 95, 150, 22), "Custom Position:");
            Settings.ButtonLocationString = Widgets.TextField(new Rect(175, 95, 60, 20), Settings.ButtonLocationString);
            if (Widgets.ButtonText(new Rect(175, 125, 40, 22), "Apply"))
            {
                if (int.TryParse(Settings.ButtonLocationString, out int result))
                {
                    Settings.ButtonLocationString = result.ToString();
                    Settings.ApplyLocation();
                    Messages.Message("Custom position applied (requires a re-load of a save).", MessageTypeDefOf.RejectInput);
                }
                else
                    Messages.Message("Position is not a whole number", MessageTypeDefOf.NegativeEvent);
            }

            Widgets.Label(new Rect(0, 160, 150, 22), "Advanced Settings: ");
            Widgets.Checkbox(new Vector2(150, 159), ref AdvancedModeEnabled);

            if (AdvancedModeEnabled)
            {
                int y = 200;

                Widgets.Label(new Rect(20, y, 250, 22), "Enable Preferred (restart required)");
                bool b = Settings.IsPreferredEnabled;
                Widgets.Checkbox(new Vector2(260, y), ref Settings.IsPreferredEnabled);
                if (b != Settings.IsPreferredEnabled)
                {
                    foreach(PathAvoidDef d in DefDatabase<PathAvoidDef>.AllDefs)
                    {
                        if (d.defName.Equals("PathAvoidPrefer"))
                        {
                            d.display = Settings.IsPreferredEnabled;
                            break;
                        }
                    }

                    if (Settings.IsPreferredEnabled)
                    {
                        if (PathAvoidDefNameValue.ContainsKey("Prefer"))
                            PathAvoidDefNameValue["Prefer"] = "0";
                        if (PathAvoidDefNameValue.ContainsKey("Normal"))
                            PathAvoidDefNameValue["Normal"] = "10";
                    }
                    else
                    {
                        if (PathAvoidDefNameValue.ContainsKey("Normal"))
                            PathAvoidDefNameValue["Normal"] = "0";
                    }
                }
                y += 30;

                IEnumerable<PathAvoidDef> avoidPathDefs = DefDatabase<PathAvoidDef>.AllDefs;
                if (PathAvoidDefNameValue.Count == 0)
                {
                    foreach (PathAvoidDef current in avoidPathDefs)
                        PathAvoidDefNameValue.Add(current.name, current.level.ToString());
                }

                foreach (PathAvoidDef current in avoidPathDefs)
                {
                    if (!Settings.IsPreferredEnabled && current.isPrefer)
                        continue;

                    Widgets.Label(new Rect(20, y, 50, 22), current.name);

                    if (PathAvoidDefNameValue.TryGetValue(current.name, out string v))
                    {
                        v = Widgets.TextField(new Rect(80, y, 50, 22), v);

                        PathAvoidDefNameValue[current.name] = v;
                    }
                    else
                    {
                        PathAvoidDefNameValue.Add(current.name, current.level.ToString());
                    }
                    y += 30;
                }

                if (Widgets.ButtonText(new Rect(40, y, 60, 22), "Apply"))
                {
                    ApplyLevelSettings(avoidPathDefs);
                    Messages.Message("Settings Applied", MessageTypeDefOf.PositiveEvent);
                }
                if (Widgets.ButtonText(new Rect(140, y, 60, 22), "Default"))
                {
                    SetDefaults(PathAvoidDefNameValue);
                    ApplyLevelSettings(avoidPathDefs);
                    Messages.Message("Default Settings Applied", MessageTypeDefOf.PositiveEvent);
                }
            }
            GUI.EndGroup();
        }

        public static void SetDefaults(Dictionary<string, string> d)
        {
            SetValue(d, "Prefer", "0");
            SetValue(d, "Normal", "0");
            SetValue(d, "Dislike", "25");
            SetValue(d, "Hate", "50");
            SetValue(d, "Strong", "200");
        }

        public static void SetValue(Dictionary<string, string> d, string name, string value)
        {
            if (d.ContainsKey(name))
                d[name] = value;
            else
                d.Add(name, value);
        }

        public static void ApplyLevelSettings(IEnumerable<PathAvoidDef> avoidPathDefs)
        {
            if (PathAvoidDefNameValue == null || PathAvoidDefNameValue.Count == 0)
                return;

            foreach (PathAvoidDef current in avoidPathDefs)
            {
                if (!PathAvoidDefNameValue.ContainsKey(current.name))
                    continue;
                if (int.TryParse(PathAvoidDefNameValue[current.name], out int value))
                {
                    if (value < 0)
                    {
                        Messages.Message(current.name + " value cannot be less than 0. Setting to 0.", MessageTypeDefOf.RejectInput);
                        PathAvoidDefNameValue[current.name] = "0";
                    }
                    else if (value > 255)
                    {
                        Messages.Message(current.name + " value cannot be greather than 255. Setting to 255.", MessageTypeDefOf.RejectInput);
                        PathAvoidDefNameValue[current.name] = "255";
                    }
                    current.level = value;
                }
                else
                {
                    Messages.Message(current.name + " value is not a number.", MessageTypeDefOf.RejectInput);
                    return;
                }
            }
        }
    }

    class Settings : ModSettings
    {
        private const string VERSION = "1.0";
        public static string ButtonLocationString = "0";
        public static bool IsPreferredEnabled = false;

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (!SettingsController.AdvancedModeEnabled)
                    IsPreferredEnabled = false;
            }

            string version = VERSION;
            Scribe_Values.Look<string>(ref version, "PathAvoid.Version", null, true);
            Scribe_Values.Look<string>(ref ButtonLocationString, "PathAvoid.ButtonOrder", "0", false);
            Scribe_Values.Look<bool>(ref SettingsController.AdvancedModeEnabled, "PathAvoice.AdvancedModeEnabled", false, false);
            Scribe_Values.Look<bool>(ref IsPreferredEnabled, "PathAvoid.IsPreferredEnabled", false, false);
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (version == null || !version.Equals(VERSION))
                {
                    SettingsController.AdvancedModeEnabled = false;
                }
            }

            if (SettingsController.AdvancedModeEnabled)
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                SettingsController.SetDefaults(d);

                foreach (KeyValuePair<string, string> kv in d)
                {
                    int value = int.Parse(kv.Value);
                    if (SettingsController.PathAvoidDefNameValue.ContainsKey(kv.Key))
                    {
                        int.TryParse(SettingsController.PathAvoidDefNameValue[kv.Key], out value);
                    }

                    Scribe_Values.Look<int>(ref value, "PathAvoid." + kv.Key, int.Parse(kv.Value), false);

                    if (Scribe.mode != LoadSaveMode.Saving)
                    {
                        SettingsController.SetValue(SettingsController.PathAvoidDefNameValue, kv.Key, value.ToString());
                    }
                }
            }
            else // Not advanced mode
            {
                IsPreferredEnabled = false;
            }

            if (SettingsController.PathAvoidDefNameValue == null)
            {
                SettingsController.PathAvoidDefNameValue = new Dictionary<string, string>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ApplyLocation();
                if (!SettingsController.AdvancedModeEnabled)
                {
                    SettingsController.PathAvoidDefNameValue.Clear();
                }
                else
                {
                    SettingsController.ApplyLevelSettings(DefDatabase<PathAvoidDef>.AllDefs);
                }
            }
            SetIsPreferEnabled();
        }

        public static void ApplyLocation()
        {
            if (int.TryParse(ButtonLocationString, out int loc))
            {
                foreach (DesignationCategoryDef def in DefDatabase<DesignationCategoryDef>.AllDefsListForReading)
                {
                    if (def.defName == "Pathing")
                    {
                        def.order = loc;
                        break;
                    }
                }
            }
        }

        public static void SetIsPreferEnabled()
        {
            var game = Current.Game;
            if (game != null)
            {
                var rules = game.GetType().GetField("rules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game) as GameRules;
                rules.SetAllowDesignator(typeof(Designator_PathAvoid_Prefer), IsPreferredEnabled);
            }
        }
    }
}