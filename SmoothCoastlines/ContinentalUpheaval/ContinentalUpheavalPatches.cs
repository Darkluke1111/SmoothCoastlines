using HarmonyLib;
using SmoothCoastlines.LandformHeights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines.ContinentalUpheaval {

    [HarmonyPatch]
    public class ContinentalUpheavalPatches {

        public static MethodBase TargetMethod() {
            var type = AccessTools.FirstInner(typeof(GenTerra), t => t.Name.Contains("<>c__DisplayClass33_0"));
            var method = AccessTools.FirstMethod(type, m => m.Name.Contains("<generate>b__0"));
            return method;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int ldelemaCount = 0;
            int indexOfOceanicityCompVal = -1;
            var mapsizeField = AccessTools.Field(AccessTools.FirstInner(typeof(GenTerra), t => t.Name.Contains("<>c__DisplayClass33_0")), "mapsizeY");
            var mapsizem2Field = AccessTools.Field(AccessTools.FirstInner(typeof(GenTerra), t => t.Name.Contains("<>c__DisplayClass33_0")), "mapsizeYm2");
            int indexMapsizeField = -1;
            int indexMapsizeM2Field = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (ldelemaCount == 0 && codes[i].opcode == OpCodes.Ldelema) {
                    ldelemaCount++;
                    continue;
                }

                if (ldelemaCount == 1 && codes[i].opcode == OpCodes.Ldelema) {
                    if (codes[i + 2].opcode == OpCodes.Ldc_R4) {
                        ldelemaCount++;
                        indexOfOceanicityCompVal = i + 2;
                        continue;
                    }
                }

                if (indexOfOceanicityCompVal > -1 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == mapsizem2Field) {
                    indexMapsizeM2Field = i + 1;
                    continue;
                }

                if (indexMapsizeM2Field > -1 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == mapsizeField) {
                    indexMapsizeField = i + 1;
                    break;
                }
            }

            var getHeightmapCompMethod = AccessTools.Method(typeof(ContinentalUpheavalPatches), "GetHeightmapCompValue", new Type[3] { typeof(int), typeof(int), typeof(float) });
            var continentalUpheavalMethod = AccessTools.Method(typeof(ContinentalUpheavalPatches), "ContinentalUpheavalHook", new Type[3] { typeof(float), typeof(double), typeof(double) });

            var factorHeightmapAgainstOceanicity = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_S, 11),
                new CodeInstruction(OpCodes.Call, getHeightmapCompMethod)
            };

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            if (indexOfOceanicityCompVal > -1 && indexMapsizeM2Field > -1 && indexMapsizeField > -1) {
                //codes[indexOfOceanicityCompVal - 9].operand = continentalUpheavalMethod;
                //codes[indexOfOceanicityCompVal - 10].opcode = OpCodes.Nop;
                //codes[indexOfOceanicityCompVal - 16].opcode = OpCodes.Nop;
                //codes[indexOfOceanicityCompVal - 17].opcode = OpCodes.Nop;
                codes.InsertRange(indexMapsizeField, sub64FromWorldHeight);
                codes.InsertRange(indexMapsizeM2Field, sub64FromWorldHeight);
                codes.RemoveAt(indexOfOceanicityCompVal);
                codes.InsertRange(indexOfOceanicityCompVal, factorHeightmapAgainstOceanicity);
            } else {
                SmoothCoastlinesModSystem.Logger.Error("Transpiler on GenTerra's Generate Lambda Method has failed. Shoving the Sea Water placement closer to the coast will not function.");
                if (ldelemaCount < 1) {
                    SmoothCoastlinesModSystem.Logger.Error("Could not locate first ldelema instruction.");
                } else if (ldelemaCount < 2) {
                    SmoothCoastlinesModSystem.Logger.Error("Could not find the second ldelema call. Only found " + ldelemaCount);
                } else if (indexMapsizeM2Field == -1) {
                    SmoothCoastlinesModSystem.Logger.Error("Could not locate the loading of the MapsizeM2 Field.");
                } else if (indexMapsizeField == -1) {
                    SmoothCoastlinesModSystem.Logger.Error("Could not locate the loading of the Mapsize Field.");
                }
            }

            return codes.AsEnumerable();
        }

        private static float GetHeightmapCompValue(int worldx, int worldz, float oceanicity) {
            return MapLayerLandformsSmooth.noiseLandforms.GetCompValueForOceanicity(worldx, worldz, oceanicity);
        }

        private static float ContinentalUpheavalHook(float upheavalStrength, double worldX, double worldZ) {
            return 30f;
        }

    }

    [HarmonyPatch]
    public class MoreContinentalUpheavalPatches {

        //A series of patches to attempt sinking the overall world-level downwards by one step while keeping the blocks free above it up to world level.
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GenTerra), "generate")]
        public static IEnumerable<CodeInstruction> GenTerraGenerateTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfSealevelWorldHeight = -1;
            int indexOfTimesPointNine = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (indexOfSealevelWorldHeight == -1 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 256) {
                    indexOfSealevelWorldHeight = i;
                    continue;
                }

                if (indexOfSealevelWorldHeight > -1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0.9f) {
                    indexOfTimesPointNine = i;
                    break;
                }
            }

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            codes.InsertRange(indexOfTimesPointNine - 1, sub64FromWorldHeight); //Tweaks the Taper Threshold to account for the - 64 to World Height
            codes.InsertRange(indexOfSealevelWorldHeight, sub64FromWorldHeight); //Sets the Oceanicity Factor WorldHeight - 64

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GenTerra), nameof(GenTerra.initWorldGen))]
        public static IEnumerable<CodeInstruction> GenTerraInitWorldGenTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfLoad256F = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 256f) {
                    indexOfLoad256F = i;
                    break;
                }
            }

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            codes.InsertRange(indexOfLoad256F + 9, sub64FromWorldHeight); //Sets the WorldHeight sent to TerrainOctaves to WorldHeight - 64
            codes.InsertRange(indexOfLoad256F - 1, sub64FromWorldHeight); //Sets the NoiseScale to WorldHeight - 64

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GenTerra), "loadGamePre")] //This patch drops the SeaLevel down by 64 blocks, which is 1 step on the World Size scale.
        public static IEnumerable<CodeInstruction> GenTerraAssetsFinalizeTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfLDCR8 = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.4313725490196078) {
                    indexOfLDCR8 = i;
                    break;
                }
            }

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            codes.InsertRange(indexOfLDCR8 + 5, sub64FromWorldHeight);

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LandformVariant), nameof(LandformVariant.Init))]
        public static IEnumerable<CodeInstruction> LandformVariantInitTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfCallVirt = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Callvirt) {
                    indexOfCallVirt = i;
                    break;
                }
            }

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            codes.InsertRange(indexOfCallVirt + 1, sub64FromWorldHeight);

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LandformVariant), "expandOctaves")]
        public static IEnumerable<CodeInstruction> LandformVariantExpandOctavesTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfCallVirt = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Callvirt) {
                    indexOfCallVirt = i;
                    break;
                }
            }

            var sub64FromWorldHeight = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldc_I4_S, 64),
                new CodeInstruction(OpCodes.Sub)
            };

            codes.InsertRange(indexOfCallVirt + 1, sub64FromWorldHeight);

            return codes.AsEnumerable();
        }
    }
}
