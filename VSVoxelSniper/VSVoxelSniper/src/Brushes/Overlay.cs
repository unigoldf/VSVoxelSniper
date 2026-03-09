using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSVoxelSniper.Brushes {
    static class Overlay {

        private const int MaxScanDebth = 30;
        private static string[] ExclustionKeyWords = {
            "log",
            "stair",
            "slab",
            "roof",
            "ore-",
            "chiseled",
            "brick",
            "leaf",
            "leaves",
            "coral",
            "aged",
            "glass",
            "flower",
            "speleothem",
            "stalag",
            "stones",
            "water"
        };

        public static List<BlockPos> DoOverlay(IBlockAccessorRevertable bar, BrushDataPacket p, int brushsize, List<BlockPos> splatter = null, bool replaceAll = true) {

            List<BlockPos> Blocks = new List<BlockPos>();

            double AdjustedRadius = brushsize + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            int[,] memory = new int[brushsize * 2 + 1, brushsize * 2 + 1];

            for (int z = -brushsize; z < brushsize; z++) {

                for (int x = -brushsize; x < brushsize; x++) {

                    for (int y = p.BlockPos.Y; y > p.BlockPos.Y - MaxScanDebth; y--) {

                        if (memory[x + brushsize, z + brushsize] == 1) { continue; }
                        if ((Math.Pow(x, 2) + Math.Pow(z, 2)) > radiusSquared) { break; }

                        BlockPos BelowPos = new BlockPos(p.BlockPos.X + x, y - 1, p.BlockPos.Z + z);
                        Block BelowBlock = bar.GetBlock(BelowPos);

                        if (BelowBlock.BlockId == 0) { continue; } //block below must not be air

                        BlockPos AbovePos = new BlockPos(p.BlockPos.X + x, y + 1, p.BlockPos.Z + z);
                        Block AboveBlock = bar.GetBlock(AbovePos);

                        if ((AboveBlock.BlockId != 0 || !AboveBlock.IsLiquid()) && AboveBlock.LightAbsorption > 2) {  continue; } //block above must be air or liquid. and light absorbtion greater than 2

                        BlockPos CurPos = new BlockPos(p.BlockPos.X + x, y, p.BlockPos.Z + z);
                        Block CurBlock = bar.GetBlock(CurPos);

                        if (splatter != null) {
                            bool placable = false;
                            for (int i = 0; i < splatter.Count; i++) {
                                if (splatter[i].X == CurPos.X && splatter[i].Z == CurPos.Z) {
                                    placable = true;
                                    break;
                                }
                            }
                            if (!placable) { continue; }
                        }

                        if (CurBlock.BlockId == 0) { continue; } //must not be air
                        if (CurBlock.IsLiquid()) { continue; }//must not be liquid

                        for (int i = CurPos.Y; i > CurPos.Y - p.OverlayDebth; i--) {
                            BlockPos DPos = new BlockPos(CurPos.X, i, CurPos.Z);
                            Block DBlock = bar.GetBlock(DPos);
                            if (IsReplaceable(DBlock, p)) {
                                Blocks.Add(DPos);
                            }
                            memory[x + brushsize, z + brushsize] = 1;
                        }
                    }
                }
            }
            return Blocks;
        }

        private static bool IsReplaceable(Block block, BrushDataPacket p) {
            if (block.IsLiquid()) {
                return false;
            }
            if (block.BlockId == 0) {
                return false;
            }
            if (p.OverlayPerformer == SniperData.OverlayPerformers.all) {
                return true;
            }
            if (p.OverlayPerformer == SniperData.OverlayPerformers.replace) {
                if (IsOnReplaceList(block, p.ReplaceMaterial)) {
                    return true;
                }
                else {
                    return false;
                }
            }
            if (!block.CanStep) {
                return false;
            }
            if (block.LightAbsorption < 32) {
                return false;
            }
            if (CheckForKeyWords(block.ToString())) {
                return false;
            }
            return true;
        }

        public static void OverlaySetBlocks(IBlockAccessorRevertable bar, List<BlockPos> positions, int[] blocks) {
            for (int i = 0; i < positions.Count; i++) {
                OverlaySetBlock(bar, blocks[RandomInt(blocks)], positions[i]);
            }
            bar.Commit();
        }

        private static void OverlaySetBlock(IBlockAccessorRevertable bar, int blockid, BlockPos blockpos) {
            bar.SetBlock(blockid, blockpos);
        }

        private static bool IsOnReplaceList(Block CurrentMaterial, int[] ReplaceList) {
            for (int i = 0; i < ReplaceList.Length; i++) {
                if (CurrentMaterial.BlockId == ReplaceList[i]) {
                    return true;
                }
            }
            return false;
        }
        private static bool CheckForKeyWords(string blockname) {
            for (int i = 0; i < ExclustionKeyWords.Length; i++) {
                if (blockname.ToLower().Contains(ExclustionKeyWords[i])) {
                    return true;
                }
            }
            return false;
        }
        private static int RandomInt(int[] arr) {
            Random rand = new Random();
            int r = rand.Next(0, arr.Length);
            return r;
        }
    }
}
