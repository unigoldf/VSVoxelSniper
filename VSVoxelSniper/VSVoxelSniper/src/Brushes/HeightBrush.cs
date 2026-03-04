using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.IO.Compression;

namespace VSVoxelSniper.Brushes;

public class HeightBrush{
    //private ServerSideUserWorkspace ws;
    public ICoreServerAPI sapi;

    private List<BitmapExternal> maps;
    
    public void LoadHeightMaps(IPlayer player){
        string foldername = "VoxelSniperHeightMaps";
        string serverpath = Environment.CurrentDirectory;
        string finalpath = Path.Combine(serverpath, foldername);
        if (!Directory.Exists(finalpath)){
            Directory.CreateDirectory(finalpath);
        }
        string modpath = sapi.ModLoader.GetMod("vsvoxelsniper").SourcePath;
        ZipArchive zip = ZipFile.OpenRead(modpath);
        List<String> finalpaths = new List<String>();
        foreach (ZipArchiveEntry entry in zip.Entries){
            if (entry.FullName.StartsWith(foldername) && !entry.FullName.EndsWith("/")){
                string finalname = Path.Combine(finalpath, entry.Name);
                finalpaths.Add(finalname);
                if (!File.Exists(finalname)){   
                    entry.ExtractToFile(finalname);
                }
                if (File.Exists(finalname)){
                    SendChatMessage(player, finalname + " exists");
                    try{
                        maps.Add(new BitmapExternal(finalname));
                        SendChatMessage(player, "added to array");
                    }
                    catch{
                        SendChatMessage(player, "it didn't work");
                    }
                }
                else{
                    SendChatMessage(player, "it not there");
                }
            }
        }
        SendChatMessage(player,"aksjdhgf");
        foreach (BitmapExternal map in maps){
            SendChatMessage(player, "a" + map.Height.ToString() + " " + map.Width.ToString());
        }

    }
    private void SendChatMessage(IPlayer player, string text) {
        TextMessage message = new TextMessage();
        message.text = text;
        sapi.Network.GetChannel("VintageStoryVoxelSniper").SendPacket(message, (IServerPlayer)player);
    }
}