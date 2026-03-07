using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.IO.Compression;
using VSVoxelSniper.Brushes;

namespace VSVoxelSniper;

public class Config{
    public string ConfigFolderName = "VSVoxelSniperConfig";
    public ICoreServerAPI sapi;
    public string ConfigPath = "";

    public HeightBrush heightBrush;

    public void Initialize(){
        string ServerConfigFolder = sapi.ModLoader.GetMod("vsvoxelsniper").SourcePath;
        ServerConfigFolder = Directory.GetParent(Directory.GetParent(ServerConfigFolder).ToString()).ToString();
        ServerConfigFolder = Path.Combine(ServerConfigFolder, "ModConfig");
        if (!Directory.Exists(ServerConfigFolder)){
            Directory.CreateDirectory(ServerConfigFolder);
        }
        
        ConfigPath = Path.Combine(ServerConfigFolder, ConfigFolderName);
        if (!Directory.Exists(ConfigPath)){
            Directory.CreateDirectory(ConfigPath);
        }
        heightBrush.ExtractHeightMaps(ConfigPath);
        heightBrush.LoadHeightMaps(ConfigPath);
    }

    public Config(HeightBrush hb, ICoreServerAPI sapi){
        heightBrush = hb;
        this.sapi = sapi;
    }
}