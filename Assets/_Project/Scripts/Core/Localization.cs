using System;
using System.Collections.Generic;
using UnityEngine;
namespace Transmilleno
{
    public static class Localization
    {
        [Serializable]
        public class Entry
        {
            public string key; public string value;
        }
        [Serializable]
        public class Table
        {
            public Entry[] entries;
        }
        static readonly Dictionary<string, string> strings = new Dictionary<string, string>(); static string loaded;
        public static string Get(string key)
        {
            var language = Session.Save?.language ?? "es-CO";
            if (loaded != language)
            {
                strings.Clear();
                var source = Resources.Load<TextAsset>("Localization/" + language);
                if (source)
                {
                    var table = JsonUtility.FromJson<Table>(source.text);
                    if (table?.entries != null)
                        foreach (var e in table.entries)
                            strings[e.key] = e.value;
                }
                loaded = language;
            }
            return key != null && strings.TryGetValue(key, out var value) ? value : key;
        }
    }
}
