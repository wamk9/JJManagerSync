using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace JJManagerSync.Classes
{
    internal class Json
    {
        public static dynamic Parse(string json)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<string, object>;

            // Regular expression to match key-value pairs including nulls, integers, and arrays
            var regex = new Regex("\"(.*?)\":\\s*(\"(.*?)\"|(null)|([0-9]+)|\\[(.*?)\\])");
            var matches = regex.Matches(json);

            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                if (match.Groups[3].Success) // String value
                {
                    expandoDict[key] = match.Groups[3].Value;
                }
                else if (match.Groups[4].Success) // Null value
                {
                    expandoDict[key] = null;
                }
                else if (match.Groups[5].Success) // Integer value
                {
                    expandoDict[key] = int.Parse(match.Groups[5].Value);
                }
                else if (match.Groups[6].Success) // Array value
                {
                    var list = new List<string>();
                    foreach (var item in match.Groups[6].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        list.Add(item.Trim(' ', '"'));
                    }
                    expandoDict[key] = list;
                }

                // Log the key-value pair added
                Console.WriteLine($"Added key: {key}, value: {expandoDict[key]}");
            }

            return expando;
        }

        public static string Serialize(dynamic obj)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            var expandoDict = obj as IDictionary<string, object>;
            foreach (var kvp in expandoDict)
            {
                sb.AppendFormat("\"{0}\":", kvp.Key);
                sb.Append(SerializeValue(kvp.Value));
                sb.Append(",");
            }

            if (sb.Length > 1)
            {
                sb.Length--; // Remove the last comma
            }

            sb.Append("}");
            return sb.ToString();
        }

        private static string SerializeValue(object value)
        {
            var sb = new StringBuilder();

            if (value is List<string> list)
            {
                sb.Append("[");
                foreach (var item in list)
                {
                    sb.AppendFormat("\"{0}\",", item);
                }
                if (list.Count > 0)
                {
                    sb.Length--; // Remove the last comma
                }
                sb.Append("]");
            }
            else if (value is int || value is long || value is float || value is double || value is decimal)
            {
                sb.Append(value.ToString());
            }
            else if (value == null)
            {
                sb.Append("null");
            }
            else if (value is string str)
            {
                sb.AppendFormat("\"{0}\"", str);
            }
            else if (value is IDictionary<string, object> dict)
            {
                sb.Append("{");
                foreach (var kvp in dict)
                {
                    sb.AppendFormat("\"{0}\":", kvp.Key);
                    sb.Append(SerializeValue(kvp.Value));
                    sb.Append(",");
                }
                if (sb.Length > 1)
                {
                    sb.Length--; // Remove the last comma
                }
                sb.Append("}");
            }
            else
            {
                // Handle other types if necessary
                sb.AppendFormat("\"{0}\"", value.ToString());
            }

            return sb.ToString();
        }


        public static bool HasKey(dynamic obj, string key)
        {
            var expandoDict = obj as IDictionary<string, object>;
            return expandoDict.ContainsKey(key);
        }
    }
}
