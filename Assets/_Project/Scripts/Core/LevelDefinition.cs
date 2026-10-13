using UnityEngine;
namespace Transmilleno
{
    public class LevelDefinition : ScriptableObject
    {
        public int index; public string station; public string subtitle; public int guards = 1, police, pedestrians = 8;
        public float busInterval = 28, stopDuration = 14, perception = 12; public bool night, rain, knife;
        public int layout; public Vector3 spawn = new Vector3(-9, 0.1f, -16); public Vector3[] routes;
    }
}
