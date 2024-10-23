/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：XmlDocument_XxeFixed.cs
    File Function Description：Resolve XXE vulnerability, inherit from XmlDocument object, automatically set XmlResolver to null


    Creation Identifier：Senparc - 20180704

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Senparc.CO2NET.ExtensionEntities
{
    /// <summary>
    /// Resolve XXE vulnerability, automatically set XmlResolver to null
    /// </summary>
    public class XmlDocument_XxeFixed : XmlDocument
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public XmlDocument_XxeFixed(XmlResolver xmlResolver = null)
        {
            XmlResolver = null;
        }
    }
}
