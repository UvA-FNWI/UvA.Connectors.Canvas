using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace UvA.DataNose.Connectors.Canvas
{
    public abstract class CanvasObject
    {
        [JsonProperty("id")]
        public int? ID { get; set; }

        internal bool isRetrieved;
        [JsonIgnore]
        public CanvasApiConnector Connector { get; set; }
        internal string EntityName => CanvasObjectID ?? this.GetType().Name;

        internal virtual string GetUrl => this.GetType().Name.ToLower() + "s/" + ID;
        internal virtual string SaveUrl => null;
        internal virtual string CanvasObjectID => null;
        internal virtual bool SendWrapped => true;

        public void Save()
        {
            if (ID != null)
                Connector.UpdateData(this);
            else
                Connector.Create(this);

        }

        public static string ToCanvasString(object o)
        {
            if (o == null)
                return "";
            else if (o is bool?)
                return (o as bool?).Value ? "true" : "false";
            else if (o is DateTime?)
                return (o as DateTime?)?.ToString("s", System.Globalization.CultureInfo.InvariantCulture) ?? "";
            else if (o is Enum)
                return o.GetType().GetField(o.ToString()).GetCustomAttribute<EnumMemberAttribute>()?.Value ?? o.ToString();
            else
                return o.ToString();
        }

        internal IEnumerable<(string key, string value)> GetValues(string prefix = null)
        {
            return this.GetType().GetProperties().SelectMany(p => MapProperty(p, prefix));
        }

        IEnumerable<(string, string)> MapProperty(PropertyInfo prop, string prefix)
        {
            var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() as JsonPropertyAttribute;
            var propName = attr?.PropertyName;
            if (attr == null)
            {
                var dmattr = prop.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
                if (dmattr == null)
                    return new(string, string)[0];
                propName = dmattr.Name;
            }
            if (prefix == null && attr != null)
                prefix = CanvasObjectID;
            var value = prop.GetValue(this);
            prefix = prefix == null ? propName : $"{prefix}[{propName}]";
            if (value is object[] arr)
            {
                var list = new List<(string, string)>();
                for (int i = 0; i < arr.Length; i++)
                {
                    string entryPrefix = $"{prefix}[{i}]";
                    if (arr.GetValue(i) is CanvasObject co)
                        list.AddRange(co.GetValues(entryPrefix));
                    else
                        list.Add((entryPrefix, ToCanvasString(arr.GetValue(i))));
                }
                return list;
            }
            else if (value is Array enumArr)
            {
                var list = new List<(string, string)>();
                for (int i = 0; i < enumArr.Length; i++)
                {
                    string entryPrefix = $"{prefix}[]";
                    list.Add((entryPrefix, ToCanvasString(enumArr.GetValue(i))));
                }
                return list;
            }
            else if (value != null)
                return new[] { (prefix, ToCanvasString(value)) };
            else
                return new(string, string)[0];
        }
    }
}
