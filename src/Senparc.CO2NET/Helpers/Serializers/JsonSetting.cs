#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc
    
    FileName：JsonSetting.cs
    File Function Description：JSON string definition
    
    
    Creation Identifier：Senparc - 20150930
    
    Modification Identifier：Senparc - 20160722
    Modification Description：Added features to control the output content of json format, such as outputting enum type strings, not outputting default values, and exception properties, such as CodeType in membership cards
             Modified content in foreach in IDictionary

    Modification Identifier：Senparc - 20160722
    Modification Description：v4.11.5 Fixed error in WeixinJsonConventer.Serialize. Thanks to @jiehanlin
    
    Modification Identifier：Senparc - 20180526
    Modification Description：v4.22.0-rc1 JsonSetting inherits JsonSerializerSettings, using Newtonsoft.Json for serialization
    

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/Conventers/WeixinJsonConventer.cs.cs  ----

    Modification Identifier：Senparc - 20180602
    Modification Description：v0.1.0 1. Ported JsonSetting
                     2. Renamed WeixinJsonContractResolver to JsonContractResolver
                     3. Renamed WeiXinJsonSetting to JsonSettingWrap

    Modification Identifier：Senparc - 20180721
    Modification Description：v0.2.1 Optimized serialization feature recognition

----------------------------------------------------------------*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
//#if NET462
//using System.Web.Script.Serialization;
//#endif

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Senparc.CO2NET.Helpers.Serializers
{
    /// <summary>
    /// JSON output settings
    /// </summary>
    public class JsonSetting : JsonSerializerSettings
    {
        /// <summary>
        /// Whether to ignore properties of the current type and those with the IJsonIgnoreNull interface that are null. If true, such properties will not appear in the Json string
        /// </summary>
        public bool IgnoreNulls { get; set; }
        /// <summary>
        /// Properties that need special null value ignoring
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// Null properties under the specified type (Class, not Interface) will not be generated in Json
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }

        #region Add


        public class IgnoreValueAttribute : System.ComponentModel.DefaultValueAttribute
        {
            public IgnoreValueAttribute(object value) : base(value)
            {
                //Value = value;
            }
        }
        public class IgnoreNullAttribute : Attribute
        {

        }
        /// <summary>
        /// Exception properties, i.e., properties that are not excluded
        /// </summary>
        public class ExcludedAttribute : Attribute
        {

        }

        /// <summary>
        /// Enum type displays as string
        /// </summary>
        public class EnumStringAttribute : Attribute
        {

        }

        #endregion
        /// <summary>
        /// JSON output settings constructor
        /// </summary>
        /// <param name="ignoreNulls">Whether to ignore properties of the current type and those with the IJsonIgnoreNull interface that are null. If true, such properties will not appear in the Json string</param>
        /// <param name="propertiesToIgnoreNull">Properties that need special null value ignoring</param>
        /// <param name="typesToIgnoreNull">Null properties under the specified type (Class, not Interface) will not be generated in Json</param>
        public JsonSetting(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull ?? new List<string>();
            TypesToIgnoreNull = typesToIgnoreNull ?? new List<Type>();
        }
    }

    //#if NET462

    //    /// <summary>
    //    /// WeChat JSON converter
    //    /// </summary>
    //    public class WeixinJsonConventer : JavaScriptConverter
    //    {
    //        private readonly JsonSetting _jsonSetting;
    //        private readonly Type _type;

    //        public WeixinJsonConventer(Type type, JsonSetting jsonSetting = null)
    //        {
    //            this._jsonSetting = jsonSetting ?? new JsonSetting();
    //            this._type = type;
    //        }

    //        public override IEnumerable<Type> SupportedTypes
    //        {
    //            get
    //            {
    //                var typeList = new List<Type>(new[] { typeof(IJsonIgnoreNull), typeof(IJsonEnumString)/*,typeof(JsonIgnoreNull)*/ });

    //                if (_jsonSetting.TypesToIgnoreNull.Count > 0)
    //                {
    //                    typeList.AddRange(_jsonSetting.TypesToIgnoreNull);
    //                }

    //                if (_jsonSetting.IgnoreNulls)
    //                {
    //                    typeList.Add(_type);
    //                }

    //                return new ReadOnlyCollection<Type>(typeList);
    //            }
    //        }

    //        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    //        {
    //            var result = new Dictionary<string, object>();
    //            if (obj == null)
    //            {
    //                return result;
    //            }

    //            var properties = obj.GetType().GetProperties();
    //            foreach (var propertyInfo in properties)
    //            {
    //                //continue;
    //                //Excluded properties
    //                bool excludedProp = propertyInfo.IsDefined(typeof(JsonSetting.ExcludedAttribute), true);
    //                if (excludedProp)
    //                {
    //                    result.Add(propertyInfo.Name, propertyInfo.GetValue(obj, null));
    //                }
    //                else
    //                {
    //                    if (!this._jsonSetting.PropertiesToIgnoreNull.Contains(propertyInfo.Name))
    //                    {
    //                        bool ignoreProp = propertyInfo.IsDefined(typeof(ScriptIgnoreAttribute), true);
    //                        if ((this._jsonSetting.IgnoreNulls || ignoreProp) && propertyInfo.GetValue(obj, null) == null)
    //                        {
    //                            continue;
    //                        }


    //                        //Properties to ignore when value matches

    //#if NET35 || NET40
    //                        JsonSetting.IgnoreValueAttribute attri = propertyInfo.GetCustomAttributes(typeof(JsonSetting.IgnoreValueAttribute), false).FirstOrDefault() as JsonSetting.IgnoreValueAttribute;
    //                        if (attri != null && attri.Value.Equals(propertyInfo.GetValue(obj, null)))
    //                        {
    //                            continue;
    //                        }

    //                        JsonSetting.EnumStringAttribute enumStringAttri = propertyInfo.GetCustomAttributes(typeof(JsonSetting.EnumStringAttribute), false).FirstOrDefault() as JsonSetting.EnumStringAttribute;
    //                        if (enumStringAttri != null)
    //                        {
    //                            //Enum type displays as string
    //                            result.Add(propertyInfo.Name, propertyInfo.GetValue(obj, null).ToString());
    //                        }
    //                        else
    //                        {
    //                            result.Add(propertyInfo.Name, propertyInfo.GetValue(obj, null));
    //                        }
    //#else
    //                        JsonSetting.IgnoreValueAttribute attri = propertyInfo.GetCustomAttribute<JsonSetting.IgnoreValueAttribute>();
    //                        if (attri != null && attri.Value.Equals(propertyInfo.GetValue(obj)))
    //                        {
    //                            continue;
    //                        }

    //                        JsonSetting.EnumStringAttribute enumStringAttri = propertyInfo.GetCustomAttribute<JsonSetting.EnumStringAttribute>();
    //                        if (enumStringAttri != null)
    //                        {
    //                            //Enum type displays as string
    //                            result.Add(propertyInfo.Name, propertyInfo.GetValue(obj).ToString());
    //                        }
    //                        else
    //                        {
    //                            result.Add(propertyInfo.Name, propertyInfo.GetValue(obj, null));
    //                        }
    //#endif
    //                    }
    //                }
    //            }
    //            return result;
    //        }

    //        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    //        {
    //            throw new NotImplementedException(); //Converter is currently only used for ignoring properties on serialization
    //        }
    //    }

    public class JsonSettingWrap : JsonSerializerSettings
    {
        public JsonSettingWrap() : this(null)
        {

        }

        public JsonSettingWrap(JsonSetting jsonSetting)
        {
            if (jsonSetting != null)
            {
                //If null, no special handling
                ContractResolver = new JsonContractResolver(jsonSetting.IgnoreNulls, jsonSetting.PropertiesToIgnoreNull, jsonSetting.TypesToIgnoreNull);
            }
            //else
            //{
            //    jsonSetting = new JsonSetting();
            //}
        }

        /// <summary>
        /// JSON output settings constructor Priority: ignoreNulls < propertiesToIgnoreNull < typesToIgnoreNull
        /// </summary>
        /// <param name="ignoreNulls">Whether to ignore properties with the IJsonIgnoreNull interface that are null. If true, such properties will not appear in the Json string</param>
        /// <param name="propertiesToIgnoreNull">Properties that need special null value ignoring</param>
        /// <param name="typesToIgnoreNull">Null properties under the specified type (Class, not Interface) will not be generated in Json</param>
        public JsonSettingWrap(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            ContractResolver = new JsonContractResolver(ignoreNulls, propertiesToIgnoreNull, typesToIgnoreNull);
        }

    }
    public class JsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Whether to ignore properties of the current type and those with the IJsonIgnoreNull interface that are null. If true, such properties will not appear in the Json string
        /// </summary>
        bool IgnoreNulls;
        /// <summary>
        /// Properties that need special null value ignoring
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// Null properties under the specified type (Class, not Interface) will not be generated in Json
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }
        /// <summary>
        /// JSON output settings constructor Priority: ignoreNulls < propertiesToIgnoreNull < typesToIgnoreNull
        /// </summary>
        /// <param name="ignoreNulls">Whether to ignore properties of the current type and those with the IJsonIgnoreNull interface that are null. If true, such properties will not appear in the Json string</param>
        /// <param name="propertiesToIgnoreNull">Properties that need special null value ignoring</param>
        /// <param name="typesToIgnoreNull">Null properties under the specified type (Class, not Interface) will not be generated in Json</param>
        public JsonContractResolver(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull;
            TypesToIgnoreNull = typesToIgnoreNull;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            //TypesToIgnoreNull null properties under the specified type (Class, not Interface) will not be generated in Json
            if (TypesToIgnoreNull.Contains(type))
            {
                type.IsDefined(typeof(JsonSetting.IgnoreNullAttribute), false);
            }
            return base.CreateProperties(type, memberSerialization);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

#if NET462
            //IgnoreNull fields marked with IgnoreNulls are serialized based on the IgnoreNulls setting
            var ignoreNull = member.GetCustomAttribute<JsonSetting.IgnoreNullAttribute>();
            if (ignoreNull != null || IgnoreNulls)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }
            else
            {
                property.NullValueHandling = NullValueHandling.Include;
            }

            //propertiesToIgnoreNull fields specified as null are not serialized
            if (PropertiesToIgnoreNull.Contains(member.Name))
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            ////Fields that match IgnoreValue marked values are not serialized
            //var ignoreValue = member.GetCustomAttribute<JsonSetting.IgnoreValueAttribute>();
            //if (ignoreValue != null)
            //{
            //    property.DefaultValueHandling = DefaultValueHandling.Ignore;
            //    var t = member.DeclaringType;
            //    property.ShouldSerialize = instance =>
            //    {
            //        var obj = Convert.ChangeType(instance, t);
            //        var value = (member as PropertyInfo).GetValue(obj, null);
            //        return value != ignoreValue.Value;
            //    };
            //}

            //Enum serialization
            var enumString = member.GetCustomAttribute<JsonSetting.EnumStringAttribute>();
            if (enumString != null)
            {
                property.Converter = new StringEnumConverter();
                //property = base.CreateProperty(member, memberSerialization);
            }
#else
            var customAttributes = member.GetCustomAttributes(false);
            var ignoreNullAttribute = typeof(JsonSetting.IgnoreNullAttribute);
            //IgnoreNull fields marked with IgnoreNulls are serialized based on the IgnoreNulls setting
            if (IgnoreNulls || customAttributes.Count(o => o.GetType() == ignoreNullAttribute) == 1)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }
            else
            {
                property.NullValueHandling = NullValueHandling.Include;
            }

            //TODO: Once IgnoreNulls is executed, some special judgments may no longer be needed

            //PropertiesToIgnoreNull fields specified as null are not serialized
            if (PropertiesToIgnoreNull.Contains(member.Name))
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            //TypesToIgnoreNull specific type fields specified as null are not serialized
            if (TypesToIgnoreNull.Contains(property.PropertyType))
            {
                //Console.WriteLine("Ignore null values: " + property.PropertyType);
                property.NullValueHandling = NullValueHandling.Ignore;//This setting is invalid

                var t = member.DeclaringType;

                property.ShouldSerialize = instance =>
                {
                    try
                    {
                        //var obj = Convert.ChangeType(instance, t);
                        var value = (member as PropertyInfo).GetValue(instance, null);

                        //Tracking test
                        //Console.WriteLine("Object Value:" + value);
                        //Console.WriteLine("Setting Value:" + (ignoreValue as JsonSetting.IgnoreValueAttribute).Value);
                        //Console.WriteLine("ShouldSerialize Result:" + (!value.Equals((ignoreValue as JsonSetting.IgnoreValueAttribute).Value)));

                        //return value != (ignoreValue as JsonSetting.IgnoreValueAttribute).Value;

                        //Console.WriteLine("TypesToIgnoreNull Value: " + value);
                        //Console.WriteLine("TypesToIgnoreNull Value is null: " + (value == null));

                        return value != null;
                    }
                    catch (Exception ex)
                    {
                        Trace.SenparcTrace.BaseExceptionLog(new Exceptions.BaseException(ex.Message, ex));
                        return true;
                    }

                };
            }


            //Fields that match IgnoreValue marked values are not serialized
            var ignoreValueAttribute = typeof(JsonSetting.IgnoreValueAttribute);
            var ignoreValue = customAttributes.FirstOrDefault(o => o.GetType() == ignoreValueAttribute);
            if (ignoreValue != null)
            {
                //property.DefaultValueHandling = DefaultValueHandling.Ignore;
                var t = member.DeclaringType;

                property.ShouldSerialize = instance =>
                {
                    //var obj = Convert.ChangeType(instance, t);
                    var value = (member as PropertyInfo).GetValue(instance, null);

                    //Tracking test
                    //Console.WriteLine("Object Value:" + value);
                    //Console.WriteLine("Setting Value:" + (ignoreValue as JsonSetting.IgnoreValueAttribute).Value);
                    //Console.WriteLine("ShouldSerialize Result:" + (!value.Equals((ignoreValue as JsonSetting.IgnoreValueAttribute).Value)));

                    //return value != (ignoreValue as JsonSetting.IgnoreValueAttribute).Value;
                    return !value.Equals((ignoreValue as JsonSetting.IgnoreValueAttribute).Value);
                };
            }

            //Enum serialization
            var enumStringAttribute = typeof(JsonSetting.EnumStringAttribute);
            if (customAttributes.Count(o => o.GetType() == enumStringAttribute) == 1)
            {
                property.Converter = new StringEnumConverter();
            }
#endif

            //var defaultIgnore = member.GetCustomAttribute<DefaultIgnoreAttribute>();
            //if (defaultIgnore != null)
            //{
            //    //defaultIgnore.Value == member.
            //}
            return property;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            return base.CreateContract(objectType);
        }
    }

    //#endif
}
