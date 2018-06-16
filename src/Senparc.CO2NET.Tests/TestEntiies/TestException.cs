using Senparc.CO2NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntiies
{
    public class TestException : BaseException
    {
        public TestException(string message, System.Exception inner, bool logged = false) 
            : base(message, inner, logged)
        {
        }
    }
}
