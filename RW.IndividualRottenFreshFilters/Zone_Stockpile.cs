using UnityEngine;
using Verse;

namespace IndividualRottenFreshFilters
{
    public class Zone_Stockpile : Zone
    {
        // … existing stockpile code …  

        // new toggles  
        public bool allowFreshAnimals = true;
        public bool allowRottenAnimals = true;
        public bool allowFreshInsects = true;
        public bool allowRottenInsects = true;
        public bool allowFreshHumanlikes = true;
        public bool allowRottenHumanlikes = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowFreshAnimals, "allowFreshAnimals", true);
            Scribe_Values.Look(ref allowRottenAnimals, "allowRottenAnimals", true);
            Scribe_Values.Look(ref allowFreshInsects, "allowFreshInsects", true);
            Scribe_Values.Look(ref allowRottenInsects, "allowRottenInsects", true);
            Scribe_Values.Look(ref allowFreshHumanlikes, "allowFreshHumanlikes", true);
            Scribe_Values.Look(ref allowRottenHumanlikes, "allowRottenHumanlikes", true);
        }

        // Implementing the abstract property from Zone  
        protected override Color NextZoneColor
        {
            get
            {
                // Return a default color or a color specific to Zone_Stockpile  
                return Color.green;
            }
        }
    }
}