﻿#region LICENSE
/*
 *   Copyright 2014 Angelo Simone Scotto <scotto.a@gmail.com>
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 * 
 * */
#endregion

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName：Lock.cs
    File Function Description：Redis Lock


    Creation Identifier：Senparc - 20170402

----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redlock.CSharp
{
    public class Lock
    {

        public Lock(string resource, byte[] val, TimeSpan validity)
        {
            this.resource = resource;
            this.val = val;
            this.validity_time = validity;
        }

        private string resource;

        private byte[] val;

        private TimeSpan validity_time;

        public string Resource { get { return resource; } }

        public byte[] Value { get { return val; } }

        public TimeSpan Validity { get { return validity_time; } }
    }
}
