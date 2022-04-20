using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Datasets
{
    public class CollectableRecipesDataset
    {
        private static CollectableMaterial DimythriteOre = new CollectableMaterial()
        {
            Name = "Dimythrite Ore",
            ItemID = 27703,
        };

        private static CollectableMaterial MythriteOre = new CollectableMaterial()
        {
            Name = "Mythrite Ore",
            ItemID = 12534,
            IsGatherable = false // this is an RNG node mat
        };

        private static CollectableMaterial VolcanicTuff = new CollectableMaterial()
        {
            Name = "Volcanic Tuff",
            ItemID = 27803,
        };

        private static CollectableMaterial LignumVitaeLog = new CollectableMaterial()
        {
            Name = "Lignum Vitae Log",
            ItemID = 27687,
        };

        private static CollectableMaterial MythriteSand = new CollectableMaterial()
        {
            Name = "Mythrite Sand",
            ItemID = 12531,
        };

        private static CollectableMaterial DimythriteSand = new CollectableMaterial()
        {
            Name = "Dimythrite Sand",
            ItemID = 27702,
        };

        private static CollectableMaterial SeaSwallowSkin = new CollectableMaterial()
        {
            Name = "Sea Swallow Skin",
            ItemID = 27736,
            IsGatherable = false
        };

        private static CollectableMaterial YellowAlumen = new CollectableMaterial()
        {
            Name = "Yellow Alumen",
            ItemID = 27817,
        };

        private static CollectableMaterial IridescentCocoon = new CollectableMaterial()
        {
            Name = "Iridescent Cocoon",
            ItemID = 27750,
        };

        private static CollectableMaterial DwarvenCottonBoll = new CollectableMaterial()
        {
            Name = "Dwarven Cotton Boll",
            ItemID = 27759,
        };

        private static CollectableMaterial EffervescentWater = new CollectableMaterial()
        {
            Name = "Effervescent Water",
            ItemID = 5491,
        };

        private static CollectableMaterial VampireCupVine = new CollectableMaterial()
        {
            Name = "Vampire Cup Vine",
            ItemID = 27773,
            IsGatherable = false
        };

        private static CollectableMaterial UndergroundSpringWater = new CollectableMaterial()
        {
            Name = "Underground Spring Water",
            ItemID = 27782,
        };

        private static CollectableMaterial AmberCloves = new CollectableMaterial()
        {
            Name = "Amber Cloves",
            ItemID = 27821,
        };

        private static CollectableMaterial Lemonette = new CollectableMaterial()
        {
            Name = "Lemonette",
            ItemID = 27835,
            IsTimedNode = true
        };

        private static CollectableMaterial GianthiveChip = new CollectableMaterial()
        {
            Name = "Gianthive Chip",
            ItemID = 27834,
        };

        public static List<Collectable> YellowCrafterScripTurnInsList = new List<Collectable>()
        {
            new Collectable()
            {
                Name = "Rarefied Lignum Vitae Grinding Wheel",
                TurnInAmount = 120,
                SubcraftCount = 4,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteOre,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = VolcanicTuff,
                        Quantity = 3
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = LignumVitaeLog,
                        Quantity = 8
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteOre,
                        Quantity = 4
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Mythril Hatchet",
                TurnInAmount = 120,
                SubcraftCount = 4,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteOre,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteSand,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteSand,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = LignumVitaeLog,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteOre,
                        Quantity = 8
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Mythril Alembic",
                TurnInAmount = 120,
                SubcraftCount = 4,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = SeaSwallowSkin,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteOre,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteSand,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = YellowAlumen,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteSand,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteOre,
                        Quantity = 8
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Mythril Ring",
                TurnInAmount = 120,
                SubcraftCount = 4,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteSand,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteSand,
                        Quantity = 8
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = IridescentCocoon,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DwarvenCottonBoll,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = EffervescentWater,
                        Quantity = 1
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Swallowskin Coat",
                TurnInAmount = 120,
                SubcraftCount = 5,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = SeaSwallowSkin,
                        Quantity = 8
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteSand,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = YellowAlumen,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteSand,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DwarvenCottonBoll,
                        Quantity = 4
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Dwarven Cotton Beret",
                TurnInAmount = 120,
                SubcraftCount = 6,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = SeaSwallowSkin,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = YellowAlumen,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DwarvenCottonBoll,
                        Quantity = 12
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Dwarven Mythril Grimoire",
                TurnInAmount = 120,
                SubcraftCount = 4,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = VampireCupVine,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteOre,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = MythriteSand,
                        Quantity = 3
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = LignumVitaeLog,
                        Quantity = 4
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = UndergroundSpringWater,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = DimythriteOre,
                        Quantity = 8
                    },
                }
            },
            new Collectable()
            {
                Name = "Rarefied Lemonade",
                TurnInAmount = 120,
                SubcraftCount = 2,
                RecipeMaterials = new List<CollectableRecipeMaterial>()
                {
                    new CollectableRecipeMaterial()
                    {
                        Material = AmberCloves,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = Lemonette,
                        Quantity = 2
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = UndergroundSpringWater,
                        Quantity = 1
                    },
                    new CollectableRecipeMaterial()
                    {
                        Material = GianthiveChip,
                        Quantity = 5
                    },
                }
            }
        };
    }

    public class Collectable
    {
        /// <summary>
        /// The name of the collectable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The amount of scrips you earn from turning this collectable in
        /// </summary>
        public int TurnInAmount { get; set; }

        /// <summary>
        /// The number of requisite crafts required to make this collectable
        /// </summary>
        public int SubcraftCount { get; set; }

        /// <summary>
        /// The total cost of all recipe materials for this collectable.
        /// This is assigned programmatically and shouldn't be hardcoded.
        /// </summary>
        public int TotalPrice { get; set; }

        /// <summary>
        /// The total time it will take to gather all gatherable recipes materials for this collectable.
        /// This is assigned programmatically and should not be hardcoded.
        /// </summary>
        public double TotalGatherTime { get; set; }

        /// <summary>
        /// The total time it will take to craft all subcrafts for this collectable.
        /// This is assigned programmatically and should not be hardcoded.
        /// </summary>
        public double TotalSubcraftTime { get; set; }

        /// <summary>
        /// How many times we'll need to gather from a timed node for this material
        /// This is assigned programmatically and should not be hardcoded.
        /// </summary>
        public double TotalTimedNodeCycles { get; set; }

        /// <summary>
        /// A list of the recipe materials required for this collectable
        /// </summary>
        public List<CollectableRecipeMaterial> RecipeMaterials { get; set; }
    }

    public class CollectableRecipeMaterial
    {
        /// <summary>
        /// The base material, which contains its name and ID
        /// </summary>
        public CollectableMaterial Material { get; set; }

        /// <summary>
        /// Number of items required by this recipe
        /// </summary>
        public int Quantity { get; set; }
    }

    public class CollectableMaterial
    {
        /// <summary>
        /// The name of the material
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The item's ingame ID
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        /// Whether this material is gatherable (or should be gathered)
        /// RNG items (eg Mythrite Ore or Titanium Ore) should have this set to false
        /// </summary>
        public bool IsGatherable { get; set; } = true;

        /// <summary>
        /// Whether this material is gathered from a timed node 
        /// </summary>
        public bool IsTimedNode { get; set; } = false;
    }
}
