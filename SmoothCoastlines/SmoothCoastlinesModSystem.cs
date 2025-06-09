using Vintagestory.API.Server;
using Vintagestory.API.Common;
using HarmonyLib;
using System;
using Vintagestory.API.MathTools;
using MapLayer;
using Vintagestory.ServerMods;
using Vintagestory.GameContent;
using System.Collections.Generic;
using Vintagestory.ServerMods.NoObf;
using System.Linq;

namespace SmoothCoastlines;

public class SmoothCoastlinesModSystem : ModSystem
{

    public static WorldGenConfig config;

    public Harmony harmony;
    private ICoreServerAPI api;

    public LandformsWorldProperty landforms;
    public LandformAltitudeInfoWorldProperty altitudeInfo;

    public override void StartPre(ICoreAPI api)
    {
        if (!Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll();
        }

    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        this.api = api;
        LoadLandforms(api);
        LoadAltitudeInfo(api);
        TryToLoadConfig(api);
    }

    public static WorldGenConfig TryToLoadConfig(ICoreAPI api)
    {
        try
        {
            config = api.LoadModConfig<WorldGenConfig>("SmoothCoastlines.json");
            if (config == null)
            {
                config = new WorldGenConfig();
            }

            api.StoreModConfig<WorldGenConfig>(config, "SmoothCoastlines.json");
        }
        catch (Exception e)
        {
            api.Logger.Error("Could not load config! Loading default settings instead.");
            api.Logger.Error(e);
            config = new WorldGenConfig();
        }
        return config;
    }

    public void LoadLandforms(ICoreServerAPI api)
    {
        IAsset asset = api.Assets.Get("worldgen/landforms.json");
        landforms = asset.ToObject<LandformsWorldProperty>();

        int quantityMutations = 0;

        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            LandformVariant variant = landforms.Variants[i];
            variant.index = i;
            variant.Init(api.WorldManager, i);

            if (variant.Mutations != null)
            {
                quantityMutations += variant.Mutations.Length;
            }
        }

        landforms.LandFormsByIndex = new LandformVariant[quantityMutations + landforms.Variants.Length];

        // Mutations get indices after the parent ones
        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            landforms.LandFormsByIndex[i] = landforms.Variants[i];
        }

        int nextIndex = landforms.Variants.Length;
        for (int i = 0; i < landforms.Variants.Length; i++)
        {
            LandformVariant variant = landforms.Variants[i];
            if (variant.Mutations != null)
            {
                for (int j = 0; j < variant.Mutations.Length; j++)
                {
                    LandformVariant variantMut = variant.Mutations[j];

                    if (variantMut.TerrainOctaves == null)
                    {
                        variantMut.TerrainOctaves = variant.TerrainOctaves;
                    }
                    if (variantMut.TerrainOctaveThresholds == null)
                    {
                        variantMut.TerrainOctaveThresholds = variant.TerrainOctaveThresholds;
                    }
                    if (variantMut.TerrainYKeyPositions == null)
                    {
                        variantMut.TerrainYKeyPositions = variant.TerrainYKeyPositions;
                    }
                    if (variantMut.TerrainYKeyThresholds == null)
                    {
                        variantMut.TerrainYKeyThresholds = variant.TerrainYKeyThresholds;
                    }


                    landforms.LandFormsByIndex[nextIndex] = variantMut;
                    variantMut.Init(api.WorldManager, nextIndex);
                    nextIndex++;
                }
            }
        }
    }

    public void LoadAltitudeInfo(ICoreServerAPI api)
    {
        IAsset asset = api.Assets.Get("smoothcoastlines:worldgen/altitudeinfo.json");
        altitudeInfo = asset.ToObject<LandformAltitudeInfoWorldProperty>();

        altitudeInfo.InitIndices(landforms);
    }
}
