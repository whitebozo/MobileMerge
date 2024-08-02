using Merge.Objects;
using UnityEngine;

namespace Merge.Game
{
    public class NextBlockPreview : MonoBehaviour
    {
        public GameObject blockPrefab;
        private Block _nextBlock;

        public void UpdatePreviewBlock(BlockDefinition definition)
        {
            if (_nextBlock != null)
            {
                Destroy(_nextBlock.gameObject);
            }

            var blockObj = Instantiate(blockPrefab, transform.position, Quaternion.identity);
            _nextBlock = blockObj.GetComponent<Block>();
            _nextBlock.transform.SetParent(transform);
            _nextBlock.transform.localScale = Vector3.one; // Ensure it scales correctly within the UI
            _nextBlock.SetDefinition(definition);
        }

        public BlockDefinition GetNextBlockDefinition()
        {
            return _nextBlock.definition;
        }
    }
}