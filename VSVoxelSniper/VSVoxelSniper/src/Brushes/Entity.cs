using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;

namespace VSVoxelSniper.src.Brushes {
    internal class Entitys {
        public static void SpawnEntity(ICoreServerAPI sapi, string id) {
            EntityProperties entityProp = sapi.World.GetEntityType(new AssetLocation(id));
            Entity entity = sapi.World.GetEntityById(entityProp.Id);
            sapi.World.SpawnEntity(entity);
        }

    }
}
