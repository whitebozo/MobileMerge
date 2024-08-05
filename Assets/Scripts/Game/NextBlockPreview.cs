using Merge.Objects;
using UnityEngine;

namespace Merge.Game
{
    public class NextBlockPreview : MonoBehaviour
    {
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private Transform previewHolder;
        private Block _nextBlock;

        public void UpdatePreviewBlock(BlockDefinition definition)
        {
            if (_nextBlock != null)
            {
                Destroy(_nextBlock.gameObject);
            }

            var blockObj = Instantiate(blockPrefab, previewHolder.position, Quaternion.identity);
            _nextBlock = blockObj.GetComponent<Block>();
            _nextBlock.transform.SetParent(previewHolder, false);
            _nextBlock.transform.localPosition = Vector3.zero;
            _nextBlock.transform.localScale = Vector3.one;
            _nextBlock.SetDefinition(definition);
        }

        public BlockDefinition GetNextBlockDefinition()
        {
            return _nextBlock.Definition;
        }
    }
}