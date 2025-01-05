#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc
  
    FileName: PropertyChangeResult.cs
    File Function Description: Enum configuration file
    
    
    Creation Identifier: Senparc - 20240728

----------------------------------------------------------------*/

namespace Senparc.CO2NET.MagicObject
{
    /// <summary>
    /// Property retrieval result of <see cref="MO{T}"/> object
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class PropertyChangeResult<TValue>
    {
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }
        public TValue? SnapshotValue { get; set; }
        public bool IsChanged { get; set; }
        public bool HasShapshot { get; set; }


        public override string ToString()
        {
            return IsChanged ? $"{OldValue} -> {NewValue}" : OldValue?.ToString();
        }
    }

}
