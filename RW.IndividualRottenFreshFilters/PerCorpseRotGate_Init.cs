using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace IndividualRottenFreshFilters
{
    [StaticConstructorOnStartup]
    static class PerCorpseRotGate_Init
    {
        internal static SpecialThingFilterDef FreshAnimal;
        internal static SpecialThingFilterDef RottenAnimal;
        internal static SpecialThingFilterDef FreshHuman;
        internal static SpecialThingFilterDef RottenHuman;
        internal static SpecialThingFilterDef FreshInsect;
        internal static SpecialThingFilterDef RottenInsect;
        internal static SpecialThingFilterDef FreshMech;
        internal static SpecialThingFilterDef RottenMech;

        static PerCorpseRotGate_Init()
        {
            // Grab your custom filter defs by defName (update names if you used different ones)
            FreshAnimal = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshAnimalCorpses");
            RottenAnimal = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenAnimalCorpses");
            FreshHuman = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshHumanlikeCorpses");
            RottenHuman = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenHumanlikeCorpses");
            FreshInsect = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshInsectCorpses");
            RottenInsect = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenInsectCorpses");
            FreshMech = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshMechanoidCorpses");
            RottenMech = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenMechanoidCorpses");

            new Harmony("IndividualRottenFreshFilters.GateCorpsesByCategory").Patch(
                AccessTools.Method(typeof(ThingFilter), nameof(ThingFilter.Allows), new Type[] { typeof(Thing) }),
                postfix: new HarmonyMethod(typeof(PerCorpseRotGate_Patch), nameof(PerCorpseRotGate_Patch.Postfix))
            );
        }
    }

    static class PerCorpseRotGate_Patch
    {
        public static void Postfix(Thing t, ThingFilter __instance, ref bool __result)
        {
            var corpse = t as Corpse;
            if (corpse == null) return;

            // Figure rot state (we only gate Fresh/Rotting; Dessicated falls through to vanilla)
            var rot = corpse.GetRotStage();
            if (rot != RotStage.Fresh && rot != RotStage.Rotting) return;

            var pawn = corpse.InnerPawn;
            if (pawn == null || pawn.RaceProps == null) return;

            // Determine race bucket
            bool isHuman = pawn.RaceProps.Humanlike;
            bool isAnimal = pawn.RaceProps.Animal;
            bool isInsect = IsInsect(pawn);
            bool isMech = IsMechanoid(pawn);

            // Pick the matching per-category filter
            SpecialThingFilterDef gate = null;
            if (isHuman)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshHuman : PerCorpseRotGate_Init.RottenHuman;
            else if (isMech)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshMech : PerCorpseRotGate_Init.RottenMech;
            else if (isInsect)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshInsect : PerCorpseRotGate_Init.RottenInsect;
            else if (isAnimal)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshAnimal : PerCorpseRotGate_Init.RottenAnimal;

            if (gate == null) return; // no specific gate for this combo -> leave vanilla result

            // Authoritative gate: if your per-category toggle is ON, allow; if OFF, disallow.
            // This overrides vanilla global "Allow fresh/rotten".
            __result = __instance.Allows(gate);
        }

        // Helpers (compatible with old/new RW versions)
        private static bool IsInsect(Pawn p)
        {
            try { return p.RaceProps.Insect; } catch { }
            return p.RaceProps.FleshType == FleshTypeDefOf.Insectoid;
        }

        private static bool IsMechanoid(Pawn p)
        {
            try { return p.RaceProps.IsMechanoid; } catch { }
            return p.RaceProps.FleshType == FleshTypeDefOf.Mechanoid;
        }
    }
}
