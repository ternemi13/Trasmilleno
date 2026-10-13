using UnityEngine;
namespace Transmilleno
{
    public class GameProgressionConfig : ScriptableObject
    {
        public LevelDefinition[] levels; public WeaponDefinition fists, knife, throwable; public EnemyDefinition guard, police; public DifficultySettings[] difficulties; public AssetCatalog catalog; public int MaxLevels => levels.Length;
    }
}
