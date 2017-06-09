using System.Collections.Generic;
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
                Messages.Message("\"Pathing\" Button will be at the top (requires a re-load of a save).", MessageSound.Benefit);
            }
            if (Widgets.ButtonText(new Rect(175, 60, 150, 22), "Bottom"))
            {
                Settings.ButtonLocationString = "0";
                Settings.ApplyLocation();
                Messages.Message("\"Pathing\" Button will be at the bottom (requires a re-load of a save).", MessageSound.Benefit);
            }

            Widgets.Label(new Rect(0, 95, 150, 22), "Custom Position:");
            Settings.ButtonLocationString = Widgets.TextField(new Rect(175, 95, 60, 20), Settings.ButtonLocationString);
            if (Widgets.ButtonText(new Rect(175, 125, 40, 22), "Apply"))
            {
                int result;
                if (int.TryParse(Settings.ButtonLocationString, out result))
                {
                    Settings.ButtonLocationString = result.ToString();
                    Settings.ApplyLocation();
                    Messages.Message("Custom position applied (requires a re-load of a save).", MessageSound.Benefit);
                }
                else
                    Messages.Message("Position is not a whole number", MessageSound.Negative);
            }

            Widgets.Label(new Rect(0, 160, 150, 22), "Advanced Settings: ");
            Widgets.Checkbox(new Vector2(150, 159), ref AdvancedModeEnabled);

            if (AdvancedModeEnabled)
            {
                IEnumerable<PathAvoidDef> avoidPathDefs = DefDatabase<PathAvoidDef>.AllDefs;
                if (PathAvoidDefNameValue.Count == 0)
                {
                    foreach (PathAvoidDef current in avoidPathDefs)
                        PathAvoidDefNameValue.Add(current.name, current.level.ToString());
                }
                int y = 200;
                foreach (PathAvoidDef current in avoidPathDefs)
                {
                    Widgets.Label(new Rect(20, y, 50, 22), current.name);
                    PathAvoidDefNameValue[current.name] = Widgets.TextField(new Rect(80, y, 50, 22), PathAvoidDefNameValue[current.name]);
                    y += 30;
                }

                if (Widgets.ButtonText(new Rect(40, y, 60, 22), "Apply"))
                {
                    ApplyLevelSettings(avoidPathDefs);
                }
                if (Widgets.ButtonText(new Rect(140, y, 60, 22), "Default"))
                {
                    SetDefaults(PathAvoidDefNameValue);
                    ApplyLevelSettings(avoidPathDefs);
                }
            }
        }

        public static void SetDefaults(Dictionary<string, string> d)
        {
            SetValue(d, "Prefer", "0");
            SetValue(d, "Normal", "18");
            SetValue(d, "Dislike", "60");
            SetValue(d, "Hate", "123");
            SetValue(d, "Strong", "255");
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
            foreach (PathAvoidDef current in avoidPathDefs)
            {
                int value;
                if (int.TryParse(PathAvoidDefNameValue[current.name], out value))
                {
                    if (value < 0)
                    {
                        Messages.Message(current.name + " value cannot be less than 0. Setting to 0.", MessageSound.Negative);
                        PathAvoidDefNameValue[current.name] = "0";
                    }
                    else if (value > 255)
                    {
                        Messages.Message(current.name + " value cannot be greather than 255. Setting to 255.", MessageSound.Negative);
                        PathAvoidDefNameValue[current.name] = "255";
                    }
                    current.level = value;
                }
                else
                {
                    Messages.Message(current.name + " value is not a number.", MessageSound.Negative);
                    return;
                }
            }
            Messages.Message("Avoid Path settings applied", MessageSound.Benefit);
        }
    }

    class Settings : ModSettings
    {
        public static string ButtonLocationString = "0";

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<string>(ref ButtonLocationString, "PathAvoid.ButtonOrder", "0", false);
            Scribe_Values.Look<bool>(ref SettingsController.AdvancedModeEnabled, "PathAvoice.AdvancedModeEnabled", false, false);

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
        }

        public static void ApplyLocation()
        {
            int loc;
            if (int.TryParse(ButtonLocationString, out loc))
            {
                foreach (PathDesignationCategoryDef def in DefDatabase<PathDesignationCategoryDef>.AllDefs)
                {
                    def.order = loc;
                }
            }
        }
    }
}