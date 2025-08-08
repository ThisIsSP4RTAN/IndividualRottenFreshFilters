using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace IndividualRottenFreshFilters
{
    [StaticConstructorOnStartup]
    static class PerCorpseRotGate_Init
    {
        internal static SpecialThingFilterDef FreshHuman;
        internal static SpecialThingFilterDef RottenHuman;
        internal static SpecialThingFilterDef FreshAnimal;
        internal static SpecialThingFilterDef RottenAnimal;
        internal static SpecialThingFilterDef FreshInsect;
        internal static SpecialThingFilterDef RottenInsect;
        internal static SpecialThingFilterDef FreshEntity;
        internal static SpecialThingFilterDef RottenEntity;

        // Vanilla master switches (resolve by name to handle version differences)
        internal static SpecialThingFilterDef VanillaAllowFresh;
        internal static SpecialThingFilterDef VanillaAllowRot; // "AllowRotten" or "AllowRotting" depending on version

        static PerCorpseRotGate_Init()
        {
            // Grab your custom filter defs by defName (update names if you used different ones)
            FreshHuman = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshHumanlikeCorpses");
            RottenHuman = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenHumanlikeCorpses");

            FreshAnimal = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshAnimalCorpses");
            RottenAnimal = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenAnimalCorpses");

            FreshInsect = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshInsectCorpses");
            RottenInsect = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenInsectCorpses");

            FreshEntity = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowFreshEntityCorpses");
            RottenEntity = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("IRFF_AllowRottenEntityCorpses");

            // Vanilla master filters (name differs across versions)
            VanillaAllowFresh = DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("AllowFresh");
            VanillaAllowRot =
                DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("AllowRotten") ??
                DefDatabase<SpecialThingFilterDef>.GetNamedSilentFail("AllowRotting");

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

            var rot = corpse.GetRotStage();
            if (rot != RotStage.Fresh && rot != RotStage.Rotting) return;

            if (!__result) return; // vanilla already rejected (wrong category/allowed thing/etc.)

            // Vanilla master switches (null => assume enabled so we don't hard-block)
            bool vanillaFreshOn = (PerCorpseRotGate_Init.VanillaAllowFresh == null) ||
                                   __instance.Allows(PerCorpseRotGate_Init.VanillaAllowFresh);
            bool vanillaRotOn = (PerCorpseRotGate_Init.VanillaAllowRot == null) ||
                                   __instance.Allows(PerCorpseRotGate_Init.VanillaAllowRot);

            if (rot == RotStage.Fresh && !vanillaFreshOn) { __result = false; return; }
            if (rot == RotStage.Rotting && !vanillaRotOn) { __result = false; return; }

            var pawn = corpse.InnerPawn;
            if (pawn == null || pawn.RaceProps == null) return;

            // Insects are animals in vanilla—check insects first to keep them independent
            bool isHuman = pawn.RaceProps.Humanlike;
            bool isInsect = IsInsect(pawn);
            bool isAnimalNonInsect = pawn.RaceProps.Animal && !isInsect; // <- insects are animals, so exclude them here
            bool isEntity = pawn.RaceProps.IsAnomalyEntity;

            // Pick the matching per-category filter (insects before animals!)
            SpecialThingFilterDef gate = null;
            if (isHuman)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshHuman : PerCorpseRotGate_Init.RottenHuman;
            else if (isInsect)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshInsect : PerCorpseRotGate_Init.RottenInsect;
            else if (isAnimalNonInsect)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshAnimal : PerCorpseRotGate_Init.RottenAnimal;
            else if (isEntity)
                gate = (rot == RotStage.Fresh) ? PerCorpseRotGate_Init.FreshEntity : PerCorpseRotGate_Init.RottenEntity;

            if (gate == null) return;

            // Your per-category toggle is authoritative over vanilla fresh/rotten
            __result = __instance.Allows(gate);
        }

        private static bool IsInsect(Pawn p)
        {
            try { return p.RaceProps.Insect; } catch { }
            return p.RaceProps.FleshType == FleshTypeDefOf.Insectoid;
        }
    }
}