using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.MagicObject
{
    public class PropertyChangeResult<TValue>
    {
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }
        public TValue? SnapshotValue { get; set; }
        public bool IsChanged { get; set; }
        public bool HasShapshot { get; set; }


        public override string ToString()
        {
            return IsChanged ? $"{OldValue} -> {NewValue}" : NewValue?.ToString();
        }
    }

}
