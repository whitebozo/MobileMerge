using UnityEngine;

namespace Merge.Objects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Milestones", menuName = "ScriptableObjects/Milestones", order = 1)]
    public class Milestones : ScriptableObject
    {
        public Milestone[] milestones;
    }
}