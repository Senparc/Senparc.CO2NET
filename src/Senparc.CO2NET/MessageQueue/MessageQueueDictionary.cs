using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.MessageQueue
{
    public class MessageQueueDictionary : Dictionary<string, SenparcMessageQueueItem>
    {
        //public List<string> KeyList { get; set; } = new List<string>();
        public MessageQueueDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}
