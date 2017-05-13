using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTI780_TP1.Contracts.Entities
{
    public class StereoscopicHeader : Header
    {
        public StereoscopicHeader(string H)
            :base(HeaderType.Stereoscopic, H)
        {
        }

        public override void SaveToBitMap()
        {

        }
    }
}
