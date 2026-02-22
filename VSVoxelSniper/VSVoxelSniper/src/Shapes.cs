using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSVoxelSniper {
    public static class Shapes {
        public static List<BlockPos> voxeldisk(BlockPos block, int radius) {
            List<BlockPos> selection = new List<BlockPos>();

            int bx = block.X;
            int bz = block.Z;

            for (int x = bx - radius; x <= bx + radius; x++) {
                for (int z = bz - radius; z <= bz + radius; z++) {
                    selection.Add(new BlockPos(x, block.Y, z));
                }
            }

            return selection;
        }

        public static List<BlockPos> disk(BlockPos block, int radius) {
            List<BlockPos> selection = new List<BlockPos>();

            double AdjustedRadius = radius + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            int bx = block.X;
            int bz = block.Z;

            for (int x = bx - radius; x <= bx + radius; x++) {
                for (int z = bz - radius; z <= bz + radius; z++) {
                    double distance = (bx - x) * (bx - x) + (bz - z) * (bz - z);
                    if (distance <= radiusSquared) {
                        selection.Add(new BlockPos(x, block.Y, z));
                    }
                }
            }

            return selection;
        }
        public static List<BlockPos> Cylinder(BlockPos block, int height, int radius) {
            List<BlockPos> selection = new List<BlockPos>();

            double AdjustedRadius = radius + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            int bx = block.X;
            int by = block.Y;
            int bz = block.Z;

            for (int x = bx - radius; x <= bx + radius; x++) {
                for (int y = by; y < by + height; y++) {
                    for (int z = bz - radius; z <= bz + radius; z++) {
                        double distance = (bx - x) * (bx - x) + (bz - z) * (bz - z);
                        if (distance <= radiusSquared) {
                            selection.Add(new BlockPos(x, y, z));
                        }
                    }
                }
            }

            return selection;
        }

        public static List<BlockPos> voxel(BlockPos block, int radius) {
            List<BlockPos> selection = new List<BlockPos>();

            for (int z = radius; z >= -radius; z--) {
                for (int x = radius; x >= -radius; x--) {
                    for (int y = radius; y >= -radius; y--) {
                        selection.Add(new BlockPos(block.X + x, block.Y + z, block.Z + y));
                    }
                }
            }
            return selection;
        }


        public static List<BlockPos> ball(BlockPos block, double radius) {
            List<BlockPos> selection = new List<BlockPos>();

            double AdjustedRadius = radius + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            int bx = block.X;
            int by = block.Y;
            int bz = block.Z;

            for (int x = (int)(bx - radius); x <= bx + radius; x++) {
                for (int y = (int)(by - radius); y <= by + radius; y++) {
                    for (int z = (int)(bz - radius); z <= bz + radius; z++) {
                        double distance = (bx - x) * (bx - x) + (bz - z) * (bz - z) + (by - y) * (by - y);
                        if (distance <= radiusSquared) {
                            selection.Add(new BlockPos(x, y, z));
                        }
                    }
                }
            }

            return selection;
        }
        public static List<BlockPos> face(BlockPos center, List<BlockPos> shape, SniperData.FaceDirection face) {
            if (face == SniperData.FaceDirection.Up || face == SniperData.FaceDirection.Down) { return shape; }

            List<BlockPos> NewPositions = new List<BlockPos>();
            for (int i = 0; i < shape.Count; i++) {
                Vec3i diff = center.ToVec3i() - shape[i].ToVec3i();
                Vec3i newpos = new Vec3i();
                if (face == SniperData.FaceDirection.North || face == SniperData.FaceDirection.South) {
                    newpos = new Vec3i(
                        center.X + diff.X,
                        center.Y + diff.Z,
                        center.Z
                        );
                }
                if (face == SniperData.FaceDirection.East || face == SniperData.FaceDirection.West) {
                    newpos = new Vec3i(
                        center.X,
                        center.Y + diff.X,
                        center.Z + diff.Z
                        );
                }
                NewPositions.Add(new BlockPos(newpos.X, newpos.Y, newpos.Z));
            }
            return NewPositions;
        }
        public static List<BlockPos> TwoPointVolume(Vec3i Pos1, Vec3i Pos2) {
            List<BlockPos> Positions = new List<BlockPos>();

            int xMin = Math.Min(Pos1.X, Pos2.X);
            int yMin = Math.Min(Pos1.Y, Pos2.Y);
            int zMin = Math.Min(Pos1.Z, Pos2.Z);

            int xMax = Math.Max(Pos1.X, Pos2.X);
            int yMax = Math.Max(Pos1.Y, Pos2.Y);
            int zMax = Math.Max(Pos1.Z, Pos2.Z);

            if (Math.Abs(xMax - xMin) * Math.Abs(yMax - yMin) * Math.Abs(zMax - zMin) > 10000000) {
                return null;
            }

            for (int x = xMin; x <= xMax; x++) {
                for (int y = yMin; y <= yMax; y++) {
                    for (int z = zMin; z <= zMax; z++) {
                        Positions.Add(new BlockPos(x, y, z));
                    }
                }
            }
            return Positions;
        }
        public static List<BlockPos> line(BlockPos pos1, BlockPos pos2) {
            List<BlockPos> selection = new List<BlockPos>();

            Vec3i vec1 = new Vec3i(pos1.ToVec3i().X, pos1.ToVec3i().Y, pos1.ToVec3i().Z);
            Vec3i vec2 = new Vec3i(pos2.ToVec3i().X, pos2.ToVec3i().Y, pos2.ToVec3i().Z);
            Vec3i dir = Vec3iSubtract(vec1, vec2);
            double len = Vec3iDistance(vec1, vec2);
            double step = .9 / len;

            if (len == 0) {
                selection.Add(pos1);
            }
            else {
                for (float i = 0; i <= 1; i += (float)step) {
                    float x = pos1.X + (pos2.X - pos1.X) * i;
                    float y = pos1.Y + (pos2.Y - pos1.Y) * i;
                    float z = pos1.Z + (pos2.Z - pos1.Z) * i;
                    BlockPos n = new BlockPos((int)x, (int)y, (int)z);
                    selection.Add(n);
                }
            }
            return selection;
        }
        public static List<BlockPos> Spike(BlockPos pos, Vec3f PlayerOrigin,int basesize, int height) {
            List<BlockPos> points = ball(pos, basesize);

            Console.WriteLine(PlayerOrigin);

            Vec3f heading = PlayerOrigin - pos.ToVec3f();
            float distance = (float)Math.Sqrt(heading.X * heading.X + heading.Y * heading.Y + heading.Z * heading.Z);
            Vec3f direction = heading / distance;

            Vec3f tippos = pos.ToVec3f() + (float)height * direction;
            BlockPos tip = new BlockPos(tippos.AsVec3i, pos.dimension);

            List<BlockPos> NewPoints = new List<BlockPos>();
            foreach (BlockPos point in points) {
                double dist = Vec3iDistance(tip.AsVec3i, point.AsVec3i);
                if (dist > (double)distance + 2d || dist < (double)distance - 2d) { continue; }
                List<BlockPos> templine = line(point, tip);
                foreach (BlockPos addition in templine) {
                    if (!NewPoints.Contains(addition)) {
                        NewPoints.Add(addition);
                    }
                }
            }

            return NewPoints;
        }
        public static List<BlockPos> forest(IBlockAccessorRevertable bar, BlockPos target, int radius, float density, BrushDataPacket packet) {
            List<BlockPos> points = new List<BlockPos>();

            //long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            int SeedLocationLikelyhood = 90;
            double Seperation = 1d / (double)density + 2d;

            Random random = new Random();

            double AdjustedRadius = radius + 0.5;
            double radiusSquared = Math.Pow(AdjustedRadius, 2);

            int bx = target.X;
            int by = Math.Clamp(target.Y - radius, 0, target.Y);
            int bz = target.Z;

            int height = radius * 2;

            for (int x = bx - radius; x <= bx + radius; x++) {
                for (int y = by; y < by + height; y++) {
                    for (int z = bz - radius; z <= bz + radius; z++) {
                        double distance = (bx - x) * (bx - x) + (bz - z) * (bz - z);
                        if (distance <= radiusSquared) {
                            BlockPos curpos = new BlockPos(x, y, z);
                            Block bls = bar.GetBlock(curpos, BlockLayersAccess.Solid);
                            Block bll = bar.GetBlock(curpos, BlockLayersAccess.Fluid);
                            if (bls.BlockId == 0 ||
                                bll.BlockId != 0 ||
                                bls.LightAbsorption < 32 ||
                                bls.ToString().Contains("log")) {
                                continue;
                            }
                            BlockPos abovepos = new BlockPos(x, y + 1, z);
                            Block abls = bar.GetBlock(abovepos, BlockLayersAccess.Solid);
                            Block abll = bar.GetBlock(abovepos, BlockLayersAccess.Fluid);
                            if (abll.BlockId != 0 || abls.BlockId != 0) {
                                continue;
                            }
                            int rand = random.Next(0, 100);
                            if (rand < SeedLocationLikelyhood) {
                                continue;
                            }
                            points.Add(curpos);
                        }
                    }
                }
            }

            //long first = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            //Console.WriteLine(first - start + " " + points.Count);

            List<BlockPos> removable = new List<BlockPos>();
            foreach (BlockPos pos in points) {
                bool isclear = true;
                for (int x = -2; x < 3; x++) {
                    for (int z = -2; z < 3; z++) {
                        for (int y = 2; y < 8; y++) {
                            BlockPos abovepos = new BlockPos(pos.X + x, pos.Y + y, pos.Z + z);
                            Block bls = bar.GetBlock(abovepos, BlockLayersAccess.Solid);
                            Block bll = bar.GetBlock(abovepos, BlockLayersAccess.Fluid);
                            if (bls.LightAbsorption > 20 || bll.BlockId != 0) {
                                removable.Add(pos);
                                isclear = false;
                                break;
                            }
                        }
                        if (!isclear) {
                            break;
                        }
                    }
                    if (!isclear) {
                        break;
                    }
                }
            }
            foreach (BlockPos pos in removable) {
                points.Remove(pos);
            }

            //long second = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            //Console.WriteLine(second - first + " " + points.Count);

            List<BlockPos> NewPoints = points.ToList();
            foreach (BlockPos pos1 in points) {
                BlockPos PosToRemove = null;
                foreach (BlockPos pos2 in NewPoints) {
                    if (pos1 != pos2 && Vec3iDistance(pos1.ToVec3i(), pos2.ToVec3i()) < Seperation) {
                        PosToRemove = pos2;
                        break;
                    }
                }
                if (PosToRemove != null) {
                    NewPoints.Remove(PosToRemove);
                }
            }

            //long third = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            //Console.WriteLine(third - second + " " + NewPoints.Count);

            return NewPoints;
        }



        public static Vec3i Vec3iSubtract(Vec3i one, Vec3i two) {
            Vec3i result = new Vec3i(
                one.X - two.X,
                one.Y - two.Y,
                one.Z - two.Z);
            return result;
        }
        public static double Vec3iDistance(Vec3i one, Vec3i two) {
            double dist = Math.Sqrt(
                Math.Pow(one.X - two.X, 2) +
                Math.Pow(one.Y - two.Y, 2) +
                Math.Pow(one.Z - two.Z, 2)
                );
            return dist;
        }
    }
}
