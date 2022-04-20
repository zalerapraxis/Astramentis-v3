using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Datasets
{
    public class MarketOrderGearsetDataset
    {
        public static List<string> fendingSet = new List<string>()
        {
            "Exarchic Sword",
            "Exarchic Tower Shield",
            "Exarchic Guillotine",
            "Exarchic Axe",
            "Exarchic Gunblade",
            "Exarchic Circlet of Fending",
            "Exarchic Coat of Fending",
            "Exarchic Gauntlets of Fending",
            "Exarchic Plate Belt of Fending",
            "Exarchic Hose of Fending",
            "Exarchic Sabatons of Fending",
            "Exarchic Earrings of Fending",
            "Exarchic Choker of Fending",
            "Exarchic Bracelet of Fending",
            "Exarchic Ring of Fending"
        };

        public static List<string> healingSet = new List<string>()
        {
            "Exarchic Cane",
            "Exarchic Codex",
            "Exarchic Star Globe",
            "Exarchic Circlet of Healing",
            "Exarchic Coat of Healing",
            "Exarchic Gloves of Healing",
            "Exarchic Plate Belt of Healing",
            "Exarchic Hose of Healing",
            "Exarchic Shoes of Healing",
            "Exarchic Earrings of Healing",
            "Exarchic Choker of Healing",
            "Exarchic Bracelet of Healing",
            "Exarchic Ring of Healing"
        };

        public static List<string> strikingSet = new List<string>()
        {
            "Exarchic Baghnakhs",
            "Exarchic Blade",
            "Exarchic Hood of Striking",
            "Exarchic Top of Striking",
            "Exarchic Armguards of Striking",
            "Exarchic Sash of Striking",
            "Exarchic Bottoms of Striking",
            "Exarchic Boots of Striking",
            "Exarchic Bracelet of Slaying",
            "Exarchic Earrings of Slaying",
            "Exarchic Choker of Slaying",
            "Exarchic Ring of Slaying"
        };

        public static List<string> maimingSet = new List<string>()
        {
            "Exarchic Spear",
            "Exarchic Circlet of Maiming",
            "Exarchic Mail of Maiming",
            "Exarchic Gauntlets of Maiming",
            "Exarchic Plate Belt of Maiming",
            "Exarchic Hose of Maiming",
            "Exarchic Sabatons of Maiming",
            "Exarchic Earrings of Slaying",
            "Exarchic Choker of Slaying",
            "Exarchic Bracelet of Slaying",
            "Exarchic Ring of Slaying",
        };

        public static List<string> scoutingSet = new List<string>()
        {
            "Exarchic Daggers",
            "Exarchic Hood of Scouting",
            "Exarchic Top of Scouting",
            "Exarchic Armguards of Scouting",
            "Exarchic Sash of Scouting",
            "Exarchic Bottoms of Scouting",
            "Exarchic Boots of Scouting",
            "Exarchic Earrings of Aiming",
            "Exarchic Choker of Aiming",
            "Exarchic Bracelet of Aiming",
            "Exarchic Ring of Aiming",
        };

        public static List<string> aimingSet = new List<string>()
        {
            "Exarchic Glaives",
            "Exarchic Longbow",
            "Exarchic Handgonne",
            "Exarchic Hood of Aiming",
            "Exarchic Top of Aiming",
            "Exarchic Armguards of Aiming",
            "Exarchic Sash of Aiming",
            "Exarchic Bottoms of Aiming",
            "Exarchic Boots of Aiming",
            "Exarchic Earrings of Aiming",
            "Exarchic Choker of Aiming",
            "Exarchic Bracelet of Aiming",
            "Exarchic Ring of Aiming",
        };

        public static List<string> castingSet = new List<string>()
        {
            "Exarchic Rod",
            "Exarchic Rapier",
            "Exarchic Grimoire",
            "Exarchic Hat of Casting",
            "Exarchic Coat of Casting",
            "Exarchic Gloves of Casting",
            "Exarchic Plate Belt of Casting",
            "Exarchic Hose of Casting",
            "Exarchic Shoes of Casting",
            "Exarchic Earrings of Casting",
            "Exarchic Choker of Casting",
            "Exarchic Bracelet of Casting",
            "Exarchic Ring of Casting",
        };


        public static List<string> craftingSet = new List<string>()
        {
            "Aesthete's Cap Of Crafting",
            "Aesthete's Doublet Of Crafting",
            "Aesthete's Fingerless Gloves Of Crafting",
            "Aesthete's Tool Belt",
            "Aesthete's Trousers Of Crafting",
            "Aesthete's Boots Of Crafting",
            "Aesthete's Ear Cuffs of Crafting",
            "Aesthete's Choker of Crafting",
            "Aesthete's Bracelets of Crafting",
            "Aesthete's Ring of Crafting",
            "Aesthete's Saw",
            "Aesthete's Claw Hammer",
            "Aesthete's Cross-Pein Hammer",
            "Aesthete's File",
            "Aesthete's Raising Hammer",
            "Aesthete's Pliers",
            "Aesthete's Mallet",
            "Aesthete's Grinding Wheel",
            "Aesthete's Round Knife",
            "Aesthete's Awl",
            "Aesthete's Needle",
            "Aesthete's Spinning Wheel",
            "Aesthete's Alembic",
            "Aesthete's Mortar",
            "Aesthete's Frypan",
            "Aesthete's Culinary Knife",
        };


        public static List<string> gatheringSet = new List<string>()
        {
            "Aesthete's Hat Of Gathering",
            "Aesthete's Doublet Of Gathering",
            "Aesthete's Halfgloves Of Gathering",
            "Aesthete's Trousers Of Gathering",
            "Aesthete's Boots Of Gathering",
            "Aesthete's Belt of Gathering",
            "Aesthete's Earrings of Gathering",
            "Aesthete's Necklace of Gathering",
            "Aesthete's Bracelet of Gathering",
            "Aesthete's Ring of Gathering",
            "Aesthete's Pickaxe",
            "Aesthete's Sledgehammer",
            "Aesthete's Scythe",
            "Aesthete's Hatchet",
            "Aesthete's Fishing Rod",
        };

        public static Dictionary<string, List<string>> Gearsets = new Dictionary<string, List<string>>()
        {
            { "fending", fendingSet },
            { "healing", healingSet },
            { "striking", strikingSet },
            { "maiming", maimingSet },
            { "scouting", scoutingSet },
            { "aiming", aimingSet },
            { "casting", castingSet },
            { "crafting", craftingSet },
            { "gathering", gatheringSet },
        };
    }
}
