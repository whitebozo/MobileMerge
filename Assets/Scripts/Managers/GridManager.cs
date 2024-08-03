using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Merge.Game;
using Merge.Objects;
using UnityEngine;

namespace Merge.Managers
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private NextBlockPreview nextBlockPreview;
        [SerializeField] private GameObject[] columns; // Array to hold references to column GameObjects

        private List<BlockDefinition> _currentSpawningPool;
        private BlockDefinition[,] _grid;

        public void Initialize()
        {
            const int rows = 7;
            var cols = columns.Length;
            _grid = new BlockDefinition[cols, rows];
            _currentSpawningPool = new List<BlockDefinition>();

            // Initial pool setup
            InitializeSpawningPool();
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        private void InitializeSpawningPool()
        {
            // Initial pool setup with numbers
            AddBlocksToSpawningPool(new[] { 2, 4, 8, 16, 32, 64 });
        }

        public void AddBlocksToSpawningPool(IEnumerable<int> numbers)
        {
            foreach (var number in numbers)
            {
                var def = GetBlockDefinitionByNumber(number);
                if (def != null && !_currentSpawningPool.Contains(def))
                {
                    _currentSpawningPool.Add(def);
                }
            }
        }

        public void RemoveBlockFromSpawningPool(IEnumerable<int> numbers)
        {
            foreach (var number in numbers)
            {
                var def = GetBlockDefinitionByNumber(number);
                if (def != null && _currentSpawningPool.Contains(def))
                {
                    _currentSpawningPool.Remove(def);
                }
            }
        }
        
        public void HandleMouseClick(Vector2 localPosition, RectTransform canvasRectTransform)
        {
            // Determine which column was clicked based on the local position
            var columnWidth = canvasRectTransform.rect.width / columns.Length;
            var column = Mathf.Clamp((int)((localPosition.x + canvasRectTransform.rect.width / 2) / columnWidth), 0, columns.Length - 1);

            SpawnNewBlock(column);
        }

        public void SpawnNewBlock(int column)
        {
            var blockDefinition = nextBlockPreview.GetNextBlockDefinition();
            var blockObj = Instantiate(blockPrefab);
            var block = blockObj.GetComponent<Block>();
            block.SetDefinition(blockDefinition);

            StartCoroutine(DropBlock(block, column));
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        private IEnumerator DropBlock(Block block, int column)
        {
            var row = columns[column].transform.childCount - 1;
            while (row >= 0 && _grid[column, row] == null)
            {
                block.transform.SetParent(columns[column].transform.GetChild(row), false); // Set the parent to the row
                yield return new WaitForSeconds(0.1f);
                row--;
            }

            row++;
            if (row < columns[column].transform.childCount)
            {
                _grid[column, row] = block.definition;
                CheckMerge(column, row);
            }
        }

        private void CheckMerge(int column, int row)
        {
            var sameBlocks = new List<Block>();
            var number = _grid[column, row].number;

            // Check vertically
            for (var r = row - 1; r <= row + 1; r++)
            {
                if (r >= 0 && r < _grid.GetLength(1) && _grid[column, r] != null && _grid[column, r].number == number && !(r == row))
                {
                    sameBlocks.Add(GetBlockAtPosition(column, r));
                }
            }

            // Check horizontally
            for (var c = column - 1; c <= column + 1; c++)
            {
                if (c >= 0 && c < _grid.GetLength(0) && _grid[c, row] != null && _grid[c, row].number == number && !(c == column))
                {
                    sameBlocks.Add(GetBlockAtPosition(c, row));
                }
            }

            if (sameBlocks.Count >= 1)
            {
                var newNumber = 0;

                // Calculate new number based on the number of blocks touching
                if (sameBlocks.Count == 1)
                {
                    newNumber = number + number; // n + n
                }
                else if (sameBlocks.Count >= 2)
                {
                    newNumber = number * 4; // n * 4
                }

                foreach (var b in sameBlocks)
                {
                    var bColumn = b.transform.parent.parent.GetSiblingIndex();
                    var bRow = b.transform.parent.GetSiblingIndex();
                    _grid[bColumn, bRow] = null;
                    Destroy(b.gameObject);
                }

                // Update the original block with the new number
                _grid[column, row] = GetBlockDefinitionByNumber(newNumber);
                var newBlock = GetBlockAtPosition(column, row);
                newBlock.SetDefinition(GetBlockDefinitionByNumber(newNumber));

                // Ensure the new block is properly positioned
                newBlock.transform.SetParent(columns[column].transform.GetChild(row), false);
                newBlock.transform.localPosition = Vector3.zero;

                // Check surrounding blocks for potential new merges
                CheckSurroundingBlocksForMerge(column, row);
                
                // Drop floating blocks
                DropFloatingBlocks();

                // Update score
                GameManager.Instance.scoreManager.AddScore(newNumber);

                // Update highest block number
                GameManager.Instance.scoreManager.UpdateHighestBlockNumber(newNumber);
            }
        }

        private void CheckSurroundingBlocksForMerge(int column, int row)
        {
            // Check surrounding blocks for potential merges
            for (var r = row - 1; r <= row + 1; r++)
            {
                if (r >= 0 && r < _grid.GetLength(1) && _grid[column, r] != null && !(r == row))
                {
                    CheckMerge(column, r);
                }
            }

            for (var c = column - 1; c <= column + 1; c++)
            {
                if (c >= 0 && c < _grid.GetLength(0) && _grid[c, row] != null && !(c == column))
                {
                    CheckMerge(c, row);
                }
            }
        }
        
        private void DropFloatingBlocks()
        {
            for (var column = 0; column < _grid.GetLength(0); column++)
            {
                for (var row = 1; row < _grid.GetLength(1); row++)
                {
                    if (_grid[column, row] != null && _grid[column, row - 1] == null)
                    {
                        var block = GetBlockAtPosition(column, row);
                        var targetRow = row - 1;

                        // Find the lowest available position in the column
                        while (targetRow > 0 && _grid[column, targetRow - 1] == null)
                        {
                            targetRow--;
                        }

                        // Move the block to the new position
                        _grid[column, targetRow] = _grid[column, row];
                        _grid[column, row] = null;

                        block.transform.SetParent(columns[column].transform.GetChild(targetRow), false);
                        block.transform.localPosition = Vector3.zero;

                        Debug.Log($"Moved block from column: {column}, row: {row} to column: {column}, row: {targetRow}");
                        CheckMerge(column, targetRow);
                    }
                }
            }
        }


        private BlockDefinition GetRandomBlockDefinition()
        {
            return _currentSpawningPool[Random.Range(0, _currentSpawningPool.Count)];
        }
        
        private Block GetBlockAtPosition(int column, int row)
        {
            return columns[column].transform.GetChild(row).GetComponentInChildren<Block>();
        }

        private static BlockDefinition GetBlockDefinitionByNumber(int number)
        {
            return GameManager.Instance.blockDefinitions.blocks.FirstOrDefault(def => def.number == number);
        }
    }
}
