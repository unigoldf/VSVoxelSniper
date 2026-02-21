using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSVoxelSniper.src.Brushes {
    static class Splatter {

        public static int SeedMin = 1;
        public static int SeedMax = 9999;

        public static int GrowthMin = 1;
        public static int GrowthMax = 9999;

        public static int RecustionMin = 0;
        public static int RecustionMax = 15;

        public static List<BlockPos> Splatter3d(IBlockAccessorRevertable bar, BrushDataPacket p, BlockPos pos, IPlayer player, bool sphere = false) {

            int[,,] splat = new int[2 * p.brushsize + 1, 2 * p.brushsize + 1, 2 * p.brushsize + 1];

            Random random = new Random();

            for (int x = 0; x < splat.GetLength(0); x++) {
                for (int y = 0; y < splat.GetLength(1); y++) {
                    for (int z = 0; z < splat.GetLength(2); z++) {
                        if (random.Next(SeedMax + 1) <= p.SplatterSeed) {
                            splat[x, y, z] = 1;
                        }
                    }
                }
            }

            int gref = p.SplatterGrowth;
            int[,,] tempSplat = new int[2 * p.brushsize + 1, 2 * p.brushsize + 1, 2 * p.brushsize + 1];
            int growcheck = 0;

            for (int r = 0; r < Math.Clamp(p.SplatterRecursions, RecustionMin, RecustionMax); r++) {
                int growth = gref - ((gref / p.SplatterRecursions) * (r));
                for (int x = 0; x < splat.GetLength(0); x++) {
                    for (int y = 0; y < splat.GetLength(1); y++) {
                        for (int z = 0; z < splat.GetLength(2); z++) {

                            tempSplat[x, y, z] = splat[x, y, z];
                            growcheck = 0;

                            if (splat[x, y, z] == 0) {
                                if (x != 0 && splat[x - 1, y, z] == 1) {
                                    growcheck++;
                                }
                                if (y != 0 && splat[x, y - 1, z] == 1) {
                                    growcheck++;
                                }
                                if (z != 0 && splat[x, y, z - 1] == 1) {
                                    growcheck++;
                                }
                                if (x != splat.GetLength(0) - 1 && splat[x + 1, y, z] == 1) {
                                    growcheck++;
                                }
                                if (y != splat.GetLength(1) - 1 && splat[x, y + 1, z] == 1) {
                                    growcheck++;
                                }
                                if (z != splat.GetLength(2) - 1 && splat[x, y, z + 1] == 1) {
                                    growcheck++;
                                }
                            }
                            if (growcheck >= GrowthMin && random.Next(GrowthMax + 1) <= growth) {
                                tempSplat[x, y, z] = 1;
                            }
                        }
                    }
                }

                for (int x = 0; x < splat.GetLength(0); x++) {
                    for (int y = 0; y < splat.GetLength(1); y++) {
                        for (int z = 0; z < splat.GetLength(2); z++) {
                            if (tempSplat[x, y, z] != 0) {
                                splat[x, y, z] = tempSplat[x, y, z];
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < splat.GetLength(0); x++) {
                for (int y = 0; y < splat.GetLength(1); y++) {
                    for (int z = 0; z < splat.GetLength(2); z++) {
                        if (splat[Math.Max(x - 1, 0), y, z] == 1 &&
                            splat[Math.Min(x + 1, p.brushsize * 2), y, z] == 1 &&
                            splat[x, Math.Max(0, y - 1), z] == 1 &&
                            splat[x, Math.Min(2 * p.brushsize, y + 1), z] == 1) {

                            splat[x, y, z] = 1;

                        }
                    }
                }
            }

            double AdjustedRadius = p.brushsize + 0.5;

            List<BlockPos> positions = new List<BlockPos>();
            int offset = p.brushsize + 1;
            for (int x = 0; x < 2 * p.brushsize + 1; x++) {
                for (int y = 0; y < 2 * p.brushsize + 1; y++) {
                    for (int z = 0; z < 2 * p.brushsize + 1; z++) {
                        Vec3i CurBlockPos = new Vec3i(x + pos.X - offset, y + pos.Y - offset, z + pos.Z - offset);
                        double distance = Shapes.Vec3iDistance(CurBlockPos, pos.AsVec3i);

                        if (distance >= AdjustedRadius && sphere) { continue; }

                        if (splat[x, y, z] == 1) {
                            positions.Add(new BlockPos(CurBlockPos, player.Entity.Pos.Dimension));
                        }
                    }
                }
            }
            return positions;
        }
        public static List<BlockPos> Splatter2d(IBlockAccessorRevertable bar, BrushDataPacket p, BlockPos pos, IPlayer player, bool sphere = false) {

            int[,] splat = new int[2 * p.brushsize + 1, 2 * p.brushsize + 1];

            Random random = new Random();

            int y = 0;

            for (int x = 0; x < splat.GetLength(0); x++) {
                //for (int y = 0; y < splat.GetLength(1); y++) {
                for (int z = 0; z < splat.GetLength(1); z++) {
                    if (random.Next(SeedMax + 1) <= p.SplatterSeed) {
                        splat[x, z] = 1;
                    }
                }
                //}
            }

            int gref = p.SplatterGrowth;
            int[,] tempSplat = new int[2 * p.brushsize + 1, 2 * p.brushsize + 1];
            int growcheck = 0;

            for (int r = 0; r < Math.Clamp(p.SplatterRecursions, RecustionMin, RecustionMax); r++) {
                int growth = gref - ((gref / p.SplatterRecursions) * (r));
                for (int x = 0; x < splat.GetLength(0); x++) {
                    //for (int y = 0; y < splat.GetLength(1); y++) {
                    for (int z = 0; z < splat.GetLength(1); z++) {

                        tempSplat[x, z] = splat[x, z];
                        growcheck = 0;

                        if (splat[x, z] == 0) {
                            if (x != 0 && splat[x - 1, z] == 1) {
                                growcheck++;
                            }
                            if (z != 0 && splat[x, z - 1] == 1) {
                                growcheck++;
                            }
                            if (x != splat.GetLength(0) - 1 && splat[x + 1, z] == 1) {
                                growcheck++;
                            }
                            if (z != splat.GetLength(1) - 1 && splat[x, z + 1] == 1) {
                                growcheck++;
                            }
                        }
                        if (growcheck >= GrowthMin && random.Next(GrowthMax + 1) <= growth) {
                            tempSplat[x, z] = 1;
                        }
                    }
                    // }
                }

                for (int x = 0; x < splat.GetLength(0); x++) {
                    //for (int y = 0; y < splat.GetLength(1); y++) {
                    for (int z = 0; z < splat.GetLength(1); z++) {
                        if (tempSplat[x, z] != 0) {
                            splat[x, z] = tempSplat[x, z];
                        }
                    }
                    //}
                }
            }
            for (int x = 0; x < splat.GetLength(0); x++) {
                //for (int y = 0; y < splat.GetLength(1); y++) {
                for (int z = 0; z < splat.GetLength(1); z++) {
                    if (splat[Math.Max(x - 1, 0), z] == 1 &&
                        splat[Math.Min(x + 1, p.brushsize * 2), z] == 1) //&&
                                                                            //splat[x, Math.Max(0, y - 1), z] == 1 &&
                                                                            //splat[x, Math.Min(2 * p.brushsize, y + 1), z] == 1) 
                        {

                        splat[x, z] = 1;

                    }
                }
                //}
            }

            double AdjustedRadius = p.brushsize + 0.5;

            List<BlockPos> positions = new List<BlockPos>();
            int offset = p.brushsize + 1;
            for (int x = 0; x < splat.GetLength(0); x++) {
                //for (int y = 0; y < 2 * p.brushsize + 1; y++) {
                for (int z = 0; z < splat.GetLength(1); z++) {
                    Vec3i CurBlockPos = new Vec3i(x + pos.X - offset, pos.Y, z + pos.Z - offset);
                    double distance = Shapes.Vec3iDistance(CurBlockPos, pos.AsVec3i);
                    if (distance >= AdjustedRadius && sphere) { continue; }

                    if (splat[x, z] == 1) {
                        positions.Add(new BlockPos(CurBlockPos, player.Entity.Pos.Dimension));
                    }
                }
                //}
            }
            return positions;
        }
    }
}
