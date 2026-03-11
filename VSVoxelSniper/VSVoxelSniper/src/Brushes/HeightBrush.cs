using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.IO.Compression;
using SkiaSharp;
using Vintagestory.API.MathTools;
using Cairo;
using SkiaSharp;
using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Util;
using Path = System.IO.Path;

namespace VSVoxelSniper.Brushes;

public class HeightBrush{
    //private ServerSideUserWorkspace ws;
    public ICoreServerAPI sapi;
    string foldername = "HeightMaps";

    private List<vsvsbitmap> maps = new List<vsvsbitmap>();

    public List<BlockPos> HeightBrushOperation(BlockPos pos, int radius, float maxheight, ref int lastrotation, BrushDataPacket p, IBlockAccessorRevertable bar, IPlayer player){
        List<BlockPos> points = new List<BlockPos>();
        if (maps.Count == 0){ return points; }
        vsvsbitmap map = GetMapFromName(p.HeightBrushMap, player);
        if (map == null){ return points; }
        int randomrotation = 0;
        RotateMap(ref map, ref lastrotation, ref randomrotation, p.HeightBrushRotationMode);
        float xiteration = 0;
        float zinteration = 0;
        for (int x = pos.X - radius; x < pos.X + radius + 1; x++){
            for (int z = pos.Z - radius; z < pos.Z + radius + 1; z++){
                float whitevalue = GetWhiteValueOnMap(xiteration, zinteration, radius * 2 + 2, map, player);
                int yheight = (int)(whitevalue * maxheight);
                BlockPos curpos = new BlockPos(x, pos.Y, z, pos.dimension);
                int startpos = GetBase(curpos, radius, bar);
                if (!p.HeightBrushInversion){
                    for (int y = startpos + 1; y < startpos + yheight; y++){
                        points.Add(new BlockPos(x, y, z));
                    }
                }
                else{
                    for (int y = startpos; y > startpos - yheight; y--){
                        points.Add(new BlockPos(x, y, z));
                    }
                }
                zinteration++;
            }
            zinteration = 0;
            xiteration++;
        }
        ReturnMap(ref map, ref lastrotation, randomrotation, p.HeightBrushRotationMode);
        return points;
    }

    public void RotateMap(ref vsvsbitmap map, ref int lastrotation, ref int randomrotation, SniperData.HeightBrushRotationMode mode){
        if (mode == SniperData.HeightBrushRotationMode.cycle){
            if (lastrotation == 0){
                map.Rotate(90);
            }
            else if (lastrotation == 90){
                map.Rotate(90);
                map.Rotate(90);
            }
            else if (lastrotation == 180){
                map.Rotate(90);
                map.Rotate(90);
                map.Rotate(90);
            }
        }
        else if (mode == SniperData.HeightBrushRotationMode.random){
            Random rand = new Random();
            randomrotation = rand.Next(0, 4);
            for (int r = 0; r < randomrotation; r++){
                map.Rotate(90);
            }
        }
        else if (mode == SniperData.HeightBrushRotationMode.ninety){
            map.Rotate(90);
        }
        else if (mode == SniperData.HeightBrushRotationMode.oneeighty){
            map.Rotate(90);
            map.Rotate(90);
        }
        else if (mode == SniperData.HeightBrushRotationMode.twoseventy){
            map.Rotate(90);
            map.Rotate(90);
            map.Rotate(90);
        }
    }
    public void ReturnMap(ref vsvsbitmap map, ref int lastrotation, int randomrotation, SniperData.HeightBrushRotationMode mode){
        if (mode == SniperData.HeightBrushRotationMode.cycle){
            if (lastrotation == 0){
                map.Rotate(90);
                map.Rotate(90);
                map.Rotate(90);
            }
            else if (lastrotation == 90){
                map.Rotate(90);
                map.Rotate(90);
            }
            else if (lastrotation == 180){
                map.Rotate(90);
            }
            lastrotation += 90;
            if (lastrotation == 360){
                lastrotation = 0;
            }
        }
        else if (mode == SniperData.HeightBrushRotationMode.random){
            for (int r = 0; r < 4 - randomrotation; r++){
                map.Rotate(90);
            }
        }
        else if (mode == SniperData.HeightBrushRotationMode.ninety){
            map.Rotate(90);
            map.Rotate(90);
            map.Rotate(90);
        }
        else if (mode == SniperData.HeightBrushRotationMode.oneeighty){
            map.Rotate(90);
            map.Rotate(90);
        }
        else if (mode == SniperData.HeightBrushRotationMode.twoseventy){
            map.Rotate(90);
        }
    }
    
    private float GetWhiteValueOnMap(float x, float z, int size, vsvsbitmap map, IPlayer player){
        float value = 0;
        int xpos = (int)((x / size) * map.Width + (map.Width / size * 2));
        int zpos = (int)((z / size) * map.Height + (map.Height / size * 2));
        SKColor color = map.GetPixel(xpos, zpos);
        value = (color.Blue + color.Green + color.Red) * .33333f * .00392f;
        return value;
    }

    private int GetBase(BlockPos pos, int brushsize, IBlockAccessorRevertable bar){
        if (bar.GetBlock(pos).BlockId == 0){
            for (int y = pos.Y - 1; y > 0; y--){
                BlockPos temppos = new BlockPos(pos.X, y, pos.Z, pos.dimension);
                Block b = bar.GetBlock(temppos, BlockLayersAccess.Solid);
                if (b.BlockId != 0){
                    return y;
                }
            }
        }
        else{
            for (int y = pos.Y; y < bar.MapSizeY; y++){
                BlockPos temppos = new BlockPos(pos.X, y, pos.Z, pos.dimension);
                Block b = bar.GetBlock(temppos, BlockLayersAccess.Solid);
                if (b.BlockId == 0){
                    return y - 1;
                }
            }
        }

        return 0;
    }

    public void ExtractHeightMaps(string ConfigFolderPath){
        string mappath = Path.Combine(ConfigFolderPath, foldername);
        if (!Directory.Exists(mappath)){
            Directory.CreateDirectory(mappath);
        }

        string modpath = sapi.ModLoader.GetMod("vsvoxelsniper").SourcePath;
        ZipArchive zip = ZipFile.OpenRead(modpath);
        foreach (ZipArchiveEntry entry in zip.Entries){
            if (entry.FullName.StartsWith(foldername) && !entry.FullName.EndsWith("/")){
                string finalname = Path.Combine(mappath, entry.Name);
                if (!File.Exists(finalname)){
                    entry.ExtractToFile(finalname);
                }
            }
        }
    }

    public void LoadHeightMaps(string ConfigFolderPath){
        maps.Clear();
        String HeightMapPath = Path.Combine(ConfigFolderPath, foldername);
        foreach (string file in Directory.EnumerateFiles(HeightMapPath)){
            if (File.Exists(file)){
                try{
                    maps.Add(new vsvsbitmap(file));
                }
                catch (Exception ex){
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        Console.WriteLine($"VSVoxelsniper Loaded {maps.Count} maps.");
    }

    public String[] GetHeightMaps(){
        String[] heightmapnames = new String[maps.Count];
        for (int i = 0; i < maps.Count; i++){
            heightmapnames[i] = maps[i].name;
        }
        return heightmapnames;
    }

    public vsvsbitmap GetMapFromName(string name, IPlayer player){
        foreach (vsvsbitmap map in maps){
            if (name.ToLower() == map.name.ToLower()){
                return map;
            }
        }
        return null;
    }

    private void SendChatMessage(IPlayer player, string text){
        TextMessage message = new TextMessage();
        message.text = text;
        sapi.Network.GetChannel("VintageStoryVoxelSniper").SendPacket(message, (IServerPlayer)player);
    }


    #region custom bitmap class
    
    public class vsvsbitmap : BitmapRef{
        public SKBitmap bmp;
        public string name;
        public override int Height => bmp.Height;
        public override int Width => bmp.Width;
        public override int[] Pixels => Array.ConvertAll(bmp.Pixels, p => (int)(uint)p);
        public IntPtr PixelsPtrAndLock => bmp.GetPixels();

        public vsvsbitmap(string filePath, ILogger? logger = null){
            try{
                bmp = Decode(File.ReadAllBytes(filePath));
                name = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex){
                if (logger != null){
                    logger.Error("Failed loading bitmap from data. Will default to an empty 1x1 bitmap.");
                    logger.Error(ex);
                }

                bmp = new SKBitmap(1, 1);
                bmp.SetPixel(0, 0, SKColors.Orange);
            }
        }

        public void Rotate(int angle){
            bmp = SKRotate(bmp, angle);
        }

        public unsafe static SKBitmap Decode(ReadOnlySpan<byte> buffer){
            fixed (byte* ptr = buffer){
                using SKData data = SKData.Create((IntPtr)ptr, buffer.Length);
                using SKCodec codec = SKCodec.Create(data);

                SKImageInfo bitmapInfo = codec.Info;
                bitmapInfo.AlphaType = SKAlphaType.Unpremul;
                // needs to be set so on MacOS so we load the pixel in the correct color format for the GPU upload, else we get R / B swaped channels
                bitmapInfo.ColorType = SKColorType.Bgra8888;
                return SKBitmap.Decode(codec, bitmapInfo);
            }
        }

        public override void Dispose(){
            bmp.Dispose();
        }

        public override void Save(string filename){
            bmp.Save(filename);
        }

        public override SKColor GetPixel(int x, int y){
            return bmp.GetPixel(x, y);
        }

        public override SKColor GetPixelRel(float x, float y){
            return bmp.GetPixel((int)Math.Min(bmp.Width - 1, x * bmp.Width),
                (int)Math.Min(bmp.Height - 1, (y * bmp.Height)));
        }

        public override unsafe void MulAlpha(int alpha = 255){
            var len = Width * Height;
            var af = alpha / 255f;
            var colp = (byte*)bmp.GetPixels().ToPointer();
            for (var i = 0; i < len; i++){
                int a = colp[3];
                colp[0] = (byte)(colp[0] * af);
                colp[1] = (byte)(colp[1] * af);
                colp[2] = (byte)(colp[2] * af);

                colp[3] = (byte)(a * af);
                colp += 4;
            }
        }
        private static SKBitmap SKRotate(SKBitmap map, int angle)
        {
            SKBitmap rotated = new SKBitmap(map.Height, map.Width);
            using (SKCanvas surface = new SKCanvas(rotated))
            {
                surface.Translate(rotated.Width, 0);
                surface.RotateDegrees(angle);
                surface.DrawBitmap(map, 0, 0);
            }
            return rotated;
        }
        
        public override int[] GetPixelsTransformed(int rot = 0, int mulAlpha = 255){
            int[] bmpPixels = new int[Width * Height];
            /*int width = bmp.Width;
            int height = bmp.Height;
            FastBitmap fastBitmap = new FastBitmap();
            fastBitmap.bmp = bmp;
            int stride = fastBitmap.Stride;
            switch (rot){
                case 0:{
                    for (int y = 0; y < height; y++){
                        fastBitmap.GetPixelRow(width, y * stride, bmpPixels, y * width);
                    }

                    break;
                }
                case 90:{
                    for (int x = 0; x < width; x++){
                        int baseY = x * width;
                        for (int y = 0; y < height; y++){
                            bmpPixels[y + baseY] = fastBitmap.GetPixel(width - x - 1, y * stride);
                        }
                    }

                    break;
                }
                case 180:{
                    for (int y = 0; y < height; y++){
                        int baseX = y * width;
                        int yStride = (height - y - 1) * stride;
                        for (int x = 0; x < width; x++){
                            bmpPixels[x + baseX] = fastBitmap.GetPixel(width - x - 1, yStride);
                        }
                    }

                    break;
                }
                case 270:{
                    for (int x = 0; x < width; x++){
                        int baseY = x * width;
                        for (int y = 0; y < height; y++){
                            bmpPixels[y + baseY] = fastBitmap.GetPixel(x, (height - y - 1) * stride);
                        }
                    }

                    break;
                }
            }

            if (mulAlpha != 255){
                var alpaP = mulAlpha / 255f;
                int clearAlpha = ~(0xff << 24);

                for (int i = 0; i < bmpPixels.Length; i++){
                    var col = bmpPixels[i];
                    var curAlpha = (uint)col >> 24;
                    col &= clearAlpha;

                    bmpPixels[i] = col | ((int)(curAlpha * alpaP) << 24);
                }
            }
            */

            return bmpPixels;
        }

        public override BitmapExternal CropTo(int newSize){
            SKBitmap bmpPixels = new(newSize, newSize);
            /*int width = bmp.Width;
            int height = bmp.Height;
            int centerOffsetX = (width - newSize);
            int centerOffsetY = 0;
            int brown = (29 << 16) + (11 << 8) + 1;
            for (int x = 0; x < newSize; x++){
                for (int y = 0; y < newSize; y++){
                    var color = bmp.GetPixel(x + centerOffsetX, y + centerOffsetY);
                    int alpha = (int)((uint)color >> 24);
                    if (alpha < 255){
                        int rgb = (int)((uint)color & 0xFFFFFFu);
                        color = (uint)MathTools.ColorUtil.ColorOverlay(brown, rgb, alpha / 255f) | 0xFF000000u;
                    }

                    bmpPixels.SetPixel(x, y, color);
                }
            }*/

            return new BitmapExternal(bmpPixels);
        }
    }
    #endregion
}