using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace IndividualRottenFreshFilters
{
    [HarmonyPatch(typeof(Zone_Stockpile), "DrawSettings")]
    static class Patch_StockpileUI_DrawSettings
    {
        static void Postfix(Zone_Stockpile __instance, Rect rect)
        {
            // find the “Corpses” filter section’s position & indent
            float y = y = rect.y + 260f;
            Rect row = new Rect(rect.x + 24, y, 300, 24);

            Widgets.CheckboxLabeled(row, "Fresh humanlikes", ref __instance.allowFreshHumanlikes);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Rotten humanlikes", ref __instance.allowRottenHumanlikes);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Fresh animals", ref __instance.allowFreshAnimals);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Rotten animals", ref __instance.allowRottenAnimals);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Fresh insects", ref __instance.allowFreshInsects);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Rotten insects", ref __instance.allowRottenInsects);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Fresh entities", ref __instance.allowFreshEntities);
            row.y += 24;
            Widgets.CheckboxLabeled(row, "Rotten entities", ref __instance.allowRottenEntities);

        }
    }

    [HarmonyPatch(typeof(Zone_Stockpile), "Allows")]
    static class Patch_Stockpile_Allows
    {
        static bool Prefix(Zone_Stockpile __instance, Thing t, ref bool __result)
        {
            // only care about corpses
            if (t is Corpse corpse)
            {
                var rottable = corpse.GetComp<CompRottable>();
                bool isRotten = rottable != null && rottable.Stage == RotStage.Rotting;

                var innerPawn = corpse.InnerPawn;
                bool isHumanlike = innerPawn.RaceProps.Humanlike;
                bool isAnimal = innerPawn.RaceProps.Animal;
                bool isInsect = innerPawn.RaceProps.Insect;
                bool isEntity = innerPawn.RaceProps.IsAnomalyEntity;

                // pick the right toggle
                if (isHumanlike)
                    __result = isRotten
                      ? __instance.allowRottenHumanlikes
                      : __instance.allowFreshHumanlikes;
                else if (isAnimal)
                    __result = isRotten
                      ? __instance.allowRottenAnimals
                      : __instance.allowFreshAnimals;
                else if (isInsect)
                    __result = isRotten
                      ? __instance.allowRottenInsects
                      : __instance.allowFreshInsects;
                else if (isEntity)
                    __result = isRotten
                        ? __instance.allowRottenEntities
                        : __instance.allowFreshEntities;
                else
                    __result = true; // any other corpse type, fallback to normal

                return false; // skip the original corpse‐filter check entirely
            }

            return true; // not a corpse, let vanilla decide
        }
    }
}