using UnityEngine;

namespace Merge.Objects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BlockDefinitions", menuName = "ScriptableObjects/BlockDefinitions", order = 1)]
    public class BlockDefinitions : ScriptableObject
    {
        public BlockDefinition[] blocks;
    }
}