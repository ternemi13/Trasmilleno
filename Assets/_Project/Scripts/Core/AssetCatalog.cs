using UnityEngine;
namespace Transmilleno
{
    public class AssetCatalog : ScriptableObject
    {
        public CatalogEntry[] entries;
        public CatalogEntry Get(string key) { foreach (var e in entries) if (e.key == key) return e; return null; }
    }
}
