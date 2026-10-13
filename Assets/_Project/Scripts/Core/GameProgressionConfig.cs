using UnityEngine;
namespace Transmilleno
{
    public class GameProgressionConfig : ScriptableObject { public LevelDefinition[] levels; public AssetCatalog catalog; public int MaxLevels => levels.Length; }
}
