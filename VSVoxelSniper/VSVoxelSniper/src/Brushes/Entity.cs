using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;

namespace VSVoxelSniper.Brushes {
    internal class Entities {

        public int RemoveEntities(ICoreServerAPI sapi, BrushDataPacket p, int brushsize){
            Vec3d pos = new Vec3d(p.BlockPos.X, p.BlockPos.Y, p.BlockPos.Z);
            Entity[] ens = sapi.World.GetEntitiesAround(pos, (float)brushsize, (float)brushsize);
            int count = 0;
            foreach (Entity en in ens){
                //Console.WriteLine(en.GetName() + " " + en.Code + " " + en.GetType());
                if (en.GetType().ToString().Contains("EntityAgent") ||
                    en.GetType().ToString().Contains("EntityItem") ||
                    en.GetType().ToString().Contains("EntityDrifter") ||
                    en.GetType().ToString().Contains("EntityBowtorn") ||
                    en.GetType().ToString().Contains("EntityShiver")){
                    sapi.World.DespawnEntity(en, new EntityDespawnData());
                    count++;
                }
            }
            return count;
        }
    }
}
