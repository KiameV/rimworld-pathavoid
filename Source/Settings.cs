using UnityEngine;
using Verse;

namespace PathAvoid
{
    public class SettingsController : Mod
    {
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
            GUI.BeginGroup(new Rect(0, 60, 600, 200));
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
        }
    }

    class Settings : ModSettings
    {
        public static string ButtonLocationString = "0";

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<string>(ref (ButtonLocationString), "PathAvoid.ButtonOrder", "0", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                ApplyLocation();
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