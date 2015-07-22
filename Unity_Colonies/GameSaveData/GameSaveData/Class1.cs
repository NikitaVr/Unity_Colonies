using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using ProtoBuf;

namespace GameSaveData
{
    [ProtoContract]
    public class terrain
    {
        [ProtoMember(1)]
        public int[] blocks;

        public terrain()
        {
            this.blocks = new int[0];
        }

        public terrain(int[] blockData)
        {
            this.blocks = blockData;
        }
    }
}
