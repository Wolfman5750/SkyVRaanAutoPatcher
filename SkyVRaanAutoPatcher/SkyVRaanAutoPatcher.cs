using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyVRaanCubemapPatcher
{
    public class SkyVRaanWeatherPatcher
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .SetTypicalOpen(GameRelease.SkyrimSE, "SkyVRaanWeatherPatcher.esp")
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var WRLDCounter = 0;
            var CellCounter = 0;
            var WaterStaticCounter = 0;

            var WRLDBlacklist = new HashSet<IFormLinkGetter<IWorldspaceGetter>>
            {
                Dawnguard.Worldspace.DLC1ForebearsHoldout,
                Dawnguard.Worldspace.DLC1AncestorsGladeWorld,
                Dragonborn.Worldspace.DLC2ApocryphaWorld
            };

            var WaterStaticBlacklist = new HashSet<IFormLinkGetter<IActivatorGetter>>
            {
                //*******************************************************************************
                //** Formkeys included in this blacklist will not be converted to DefaultWater **
                //*******************************************************************************
                
                //********  Not used in any worldspace ***********

                Skyrim.Activator.WaterCreekDeepInterior01,
                Skyrim.Activator.WRTreeCircleWater01ACT,
                Skyrim.Activator.TG08Water1024,
                Skyrim.Activator.Water1024DefaultIntWaistStill,
                Skyrim.Activator.Water2048DefaultIntWaistStill,
                Skyrim.Activator.Water2048x1024DefaultIntWaistStill,
                Skyrim.Activator.Water1024DefaultIntAnkleStill,
                Skyrim.Activator.Water2048x1024DefaultIntAnkleStill,
                Skyrim.Activator.Water2048DefaultIntAnkleStill,
                Skyrim.Activator.Water512,
                Skyrim.Activator.Water512DefaultIntAnkleStill,
                Skyrim.Activator.Water512DefaultIntWaistStill,
                Skyrim.Activator.WaterPuddleLiteFog,
                Skyrim.Activator.WaterPuddleFlow,
                Skyrim.Activator.Water1024River,            //Not used in Vanilla. Keep an eye out for Mods using this Activator
                Skyrim.Activator.WaterPuddleLongFlow,
                Dawnguard.Activator.DLC1TestBloodWater1024,
                Dawnguard.Activator.DLC1Water2048x1024WaterFastFlow,
                Dawnguard.Activator.Water1024DLC01DarkFallCave,
                Dawnguard.Activator.DLC1Water1024Frostreach,
                Dawnguard.Activator.DLC1Water1024FrostreachSE,
                Dawnguard.Activator.DLC1Water1024Frostreach,
                Dawnguard.Activator.Lava1024,
                Dawnguard.Activator.Blood_RainWaterBarrel,
                Dawnguard.Activator.LargeRainBarrel_BloodWater01,
                Dawnguard.Activator.DLC1BloodChaliceActivatorEmpty,
                Dawnguard.Activator.DLC1BloodChaliceActivatorFull,
                Dawnguard.Activator.DLC1BloodMagicShrine,
                Dragonborn.Activator.DLC2dunKolbjornWater,
                Dragonborn.Activator.DLC2KagrumezWater,
                Dragonborn.Activator.DLC2WaterPuddle01,
                

                //*********Used sparingly in Worldspaces*********

                Skyrim.Activator.Water1024Murky,            //Used in Darkfalls Passage and Forebears Holdout in WRLD. Consider removing if there are CTDs
                 //Skyrim.Activator.Water1024NoReflections,  //Used in Blackreach Worldspace. I'd like to change it to DefaultWater. Reenable if dungeons look off.               

                Skyrim.Activator.WaterPuddleDefaultIntAnkleStill,   //Used in Southfringe world. Should be ok...

                //I want Markarth Waters to be SkyVRaan Waters. These activators are noted for reference in case they do not look correct.
                //Skyrim.Activator.MrkWaterSystemMillPond01act,
                //Skyrim.Activator.MrkWaterSystemWarrens01,
                //Skyrim.Activator.MrkWaterSystemMillPond01,
                //Skyrim.Activator.MarkarthLowerWater,
                //Skyrim.Activator.MarkarthWaterSystemStream,

                //Apocrypha already has fake reflections and should not be touched
                Dragonborn.Activator.DLC2WaterApocrypha1024,
                Dragonborn.Activator.DLC2WaterApocrypha1024Small,
                Dragonborn.Activator.DLC2WaterApocrypha1024Small_Movable,
                Dragonborn.Activator.DLC2ApoControlRing01WaterApocrypha,
                Dragonborn.Activator.DLC2ApoControlRing01WaterClean,
                Dragonborn.Activator.ApoFakeWater


                //Not Blacklisted because no known issues, but used in worldspaces. Consider enabling if you experience crashes.
                //Skyrim.Activator.Water1024Volcanic        //Volcanic Water is skipped for now. It doesn't seem to be an issue yet. 
            };

            var WaterBlacklist = new HashSet<IFormLinkGetter<IWaterGetter>>
            {
                Skyrim.Water.DefaultVolcanicWater,          //Volcanic Water is skipped for now. It doesn't seem to be an issue yet. Two watertypes should be ok.
                Skyrim.Water.BlackreachWater,
                Dragonborn.Water.DLC2ApocryphaWater,
                Dragonborn.Water.DLC2ApocryphaWaterSmall,
                Dragonborn.Water.DLC2ApocryphaWaterTamriel,
                Skyrim.Water.LavaWater,
                Dawnguard.Water.DLC1BloodWater
            };

            Console.WriteLine($"Patching Worldspaces ...");

            foreach (var WRLDContext in state.LoadOrder.PriorityOrder.Worldspace().WinningContextOverrides())
            {
                var WRLD = WRLDContext.Record;
                if (!WRLDBlacklist.Contains(WRLD.AsLink()))
                {
                    if (WRLD.Water.FormKey != null)
                    {
                        var OverriddenWRLD = WRLDContext.GetOrAddAsOverride(state.PatchMod);
                        if (!OverriddenWRLD.Equals(Skyrim.Worldspace.Blackreach))
                        {
                            OverriddenWRLD.LodWater.SetTo(Skyrim.Water.DefaultWater);
                            OverriddenWRLD.Water.SetTo(Skyrim.Water.DefaultWater);
                        }
                        OverriddenWRLD.WaterEnvironmentMap = $"Data\\Textures\\cubemaps\\OutputCube.dds";
                        WRLDCounter++;
                    }

                }

            }

            Console.WriteLine($"Patching Worldspace Cells ...");

            foreach (var cellContext in state.LoadOrder.PriorityOrder.Cell().WinningContextOverrides(state.LinkCache))
            {

                var cell = cellContext.Record;

                if (cellContext.TryGetParent<IWorldspaceGetter>(out var CellWRLD))
                {
                    if (!WRLDBlacklist.Contains(CellWRLD.AsLink()))
                    {
                        if (CellWRLD.Water.FormKey != null)
                        {
                            var overriddenCell = cellContext.GetOrAddAsOverride(state.PatchMod);
                            overriddenCell.WaterEnvironmentMap = $"Data\\Textures\\cubemaps\\OutputCube.dds";

                            if (!WaterBlacklist.Contains(overriddenCell.Water) && !CellWRLD.Equals(Skyrim.Worldspace.Blackreach))
                            {
                                overriddenCell.Water.SetTo(Skyrim.Water.DefaultWater);
                            }
                            CellCounter++;
                        }
                    }
                }
            }

            Console.WriteLine($"Patching Water Statics ... (UPDATED)");

            foreach (var ActivatorContext in state.LoadOrder.PriorityOrder.Activator().WinningContextOverrides())
            {
                if (ActivatorContext != null)
                {
                    var WaterActivator = ActivatorContext.Record;
                    Console.WriteLine($"Checking WaterActivator: " + WaterActivator.FormKey);

                    if (WaterActivator.WaterType.TryResolve(state.LinkCache, out var WaterActivatorType))
                    {
                        if (!WaterStaticBlacklist.Contains(WaterActivator) && !WaterBlacklist.Contains(WaterActivatorType))
                        {
                            var OverriddenWaterStatic = ActivatorContext.GetOrAddAsOverride(state.PatchMod);
                            OverriddenWaterStatic.WaterType.SetTo(Skyrim.Water.DefaultWater);
                            WaterStaticCounter++;
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Patched {CellCounter} CELL records in {WRLDCounter} Worldspaces and {WaterStaticCounter} Water Statics");
        }
    }
}
