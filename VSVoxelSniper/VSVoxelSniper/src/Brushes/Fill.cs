using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using static VSVoxelSniper.SniperData;

namespace VSVoxelSniper.src.Brushes {

    internal static class Fill {

        private static int WaterFloorCutoff = 1;



        internal static List<BlockPos> FillDown(IBlockAccessorRevertable bar, BrushDataPacket p) {
            List<BlockPos> selection = new List<BlockPos>();

            double AdjustedRadius = p.brushsize + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                    double distance = (p.BlockPos.X - x) * (p.BlockPos.X - x) + (p.BlockPos.Z - z) * (p.BlockPos.Z - z);
                    if (distance <= radiusSquared) {
                        for (int y = p.BlockPos.Y; y > 0; y--) {
                            BlockPos pos = new BlockPos(x, y, z);
                            Block block = bar.GetBlock(pos, BlockLayersAccess.Solid);
                            if (block.BlockId == 0) {
                                selection.Add(pos);
                                continue;
                            }
                            break;
                        }
                    }
                }
            }
            return selection;
        }

        internal static List<BlockPos> FillLiquid(IBlockAccessorRevertable bar, BrushDataPacket p) {
            List<BlockPos> selection = new List<BlockPos>();

            double AdjustedRadius = p.brushsize + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            for (int x = p.BlockPos.X - p.brushsize; x <= p.BlockPos.X + p.brushsize; x++) {
                for (int z = p.BlockPos.Z - p.brushsize; z <= p.BlockPos.Z + p.brushsize; z++) {
                    double distance = (p.BlockPos.X - x) * (p.BlockPos.X - x) + (p.BlockPos.Z - z) * (p.BlockPos.Z - z);
                    if (distance <= radiusSquared) {
                        int counter = 0;
                        for (int y = p.BlockPos.Y; y > 0; y--) {
                            BlockPos pos = new BlockPos(x, y, z);
                            Block block = bar.GetBlock(pos, BlockLayersAccess.Solid);
                            if (block.LightAbsorption < 32) {
                                selection.Add(pos);
                                counter = 0;
                                continue;
                            }
                            counter++;
                            if (counter > WaterFloorCutoff) {
                                break;
                            }
                        }
                    }
                }
            }
            return selection;
        }
        internal static void PlaceLiquid(IBlockAccessorRevertable bar, List<BlockPos> blocks, BrushDataPacket p) {
            SetBlocks(blocks, bar, p.Material);
        }
        private static void SetBlocks(List<BlockPos> blocks, IBlockAccessorRevertable bar, int[] blockid) {
            Block block = bar.GetBlock(blockid[RandomInt(blockid)]);

            for (int i = 0; i < blocks.Count; i++) {
                SetBlock(bar, blocks[i], block);
            }
            bar.Commit();
        }
        private static void SetBlock(IBlockAccessorRevertable bar, BlockPos pos, Block block) {
            bar.SetBlock(block.BlockId, pos, BlockLayersAccess.Fluid);
        }
    }
}
