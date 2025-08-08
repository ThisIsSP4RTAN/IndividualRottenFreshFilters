using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace IndividualRottenFreshFilters
{
    public class CorpseRotFilterExtension : DefModExtension
    {
        public bool humanlikes;
        public bool animals;
        public bool insects;
        public bool entities;
        public string rot;
    }

    public class SpecialThingFilterWorker_CorpseRotByRace : SpecialThingFilterWorker
    {
        // In some RW versions the worker stores its def in "def", in others "parent".
        // Use reflection so this compiles against either.
        private static readonly FieldInfo WorkerDefField =
            AccessTools.Field(typeof(SpecialThingFilterWorker), "def")
            ?? AccessTools.Field(typeof(SpecialThingFilterWorker), "parent");

        public override bool CanEverMatch(ThingDef tDef)
        {
            // We can’t rely on ThingDefOf.Corpse existing in every ref pack,
            // so check the thingClass instead.
            return tDef != null && tDef.thingClass == typeof(Corpse);
        }

        public override bool Matches(Thing t)
        {
            var corpse = t as Corpse;
            if (corpse == null) return false;

            // Get this filter's SpecialThingFilterDef (where the ModExtension lives)
            var filterDef = WorkerDefField?.GetValue(this) as SpecialThingFilterDef;
            var ext = filterDef?.GetModExtension<CorpseRotFilterExtension>();
            if (ext == null) return false;

            var pawn = corpse.InnerPawn;
            if (pawn == null) return false;

            // Race gates: if a flag is true, it must match
            if (ext.humanlikes && !pawn.RaceProps.Humanlike) return false;
            if (ext.animals && !pawn.RaceProps.Animal) return false;
            if (ext.insects && !pawn.RaceProps.Insect) return false;
            if (ext.entities && !pawn.RaceProps.IsAnomalyEntity) return false;

            // Rot stage gate
            RotStage targetStage = RotStage.Fresh;
            if (ext.rot == "Rotting") targetStage = RotStage.Rotting;
            else if (ext.rot == "Dessicated") targetStage = RotStage.Dessicated;

            var rottable = t.TryGetComp<CompRottable>();
            var actualStage = rottable != null ? rottable.Stage : RotStage.Fresh;

            return actualStage == targetStage;
        }
    }
}