using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHarvest
{
    class Config
    {
        public bool Enabled { get; set; } = true;
        public byte TicksInterval { get; set; } = 30;
        public bool CheckUpdate { get; set; } = true;
        public bool AutoHarvestCrops { get; set; } = true;
        public bool AutoDestroyDeadCrops { get; set; } = true;
        public bool AutoCollectForages { get; set; } = true;
    }
}
