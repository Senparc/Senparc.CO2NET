/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName：SenparcNullReferenceException.cs
    File Function Description：Exception for null object


    Creation Identifier：Senparc - 20200506

----------------------------------------------------------------*/


using System;

namespace Senparc.CO2NET.Exceptions
{
    public class SenparcNullReferenceException : BaseException
    {
        /// <summary>
        /// The parent object that is not null (or an object important for debugging).
        /// If multiple objects need to be debugged, an array can be passed, such as: new {obj1, obj2}
        /// </summary>
        public object ParentObject { get; set; }
        public SenparcNullReferenceException(string message)
            : this(message, null, null)
        {
        }

        public SenparcNullReferenceException(string message, object parentObject)
            : this(message, parentObject, null)
        {
            ParentObject = parentObject;
        }

        public SenparcNullReferenceException(string message, object parentObject, Exception inner)
            : base(message, inner)
        {
            ParentObject = parentObject;
        }
    }
}
