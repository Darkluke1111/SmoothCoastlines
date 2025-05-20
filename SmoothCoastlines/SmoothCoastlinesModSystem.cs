using Vintagestory.API.Server;
using Vintagestory.API.Common;
using HarmonyLib;
using System;

namespace SmoothCoastlines;

public class SmoothCoastlinesModSystem : ModSystem
{

    public static WorldGenConfig config;

    public Harmony harmony;
    private ICoreServerAPI api;

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
}
