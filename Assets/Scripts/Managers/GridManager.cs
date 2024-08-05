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

        private const int Rows = 7;
        private int Columns => columns.Length;
        
        public void Initialize()
        {
            _grid = new BlockDefinition[Columns, Rows];
            _currentSpawningPool = new List<BlockDefinition>();

            // Initial pool setup
            InitializeSpawningPool();
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        private void InitializeSpawningPool()
        {
            // Initial pool setup with numbers
            AddBlocksToSpawningPool(new[] {2, 4, 8, 16, 32, 64});
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
            var rect = canvasRectTransform.rect;
            var columnWidth = rect.width / columns.Length;
            var column = Mathf.Clamp((int) ((localPosition.x + rect.width / 2) / columnWidth), 0, columns.Length - 1);

            SpawnNewBlock(column);
        }

        private void SpawnNewBlock(int column)
        {
            var blockDefinition = nextBlockPreview.GetNextBlockDefinition();
            var blockObj = Instantiate(blockPrefab);
            var block = blockObj.GetComponent<Block>();
            block.SetDefinition(blockDefinition);

            StartCoroutine(DropBlock(block, column, 0));
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        private IEnumerator DropBlock(Block block, int column, int startingRow)
        {
            var row = startingRow;
            while (row <= Rows - 1 && _grid[column, row] == null)
            {
                block.transform.SetParent(columns[column].transform.GetChild(row), false); // Set the parent to the row
                yield return new WaitForSeconds(0.1f);
                row++;
            }

            row--;
            if (row < Rows)
            {
                _grid[column, row] = block.Definition;
                ProcessGrid((column, row));
            }
        }
        
        void ProcessGrid((int,int) lastUpdatePosition)
        {
            ProcessMerges(lastUpdatePosition);
            ProcessFloatingBlocks();
        }

        void ProcessMerges((int, int) position)
        {
            var column = position.Item1;
            var row = position.Item2;

            var block = GetBlockAtPosition(column, row);
            if (block == null)
            {
                Debug.LogError($"No block found at position [{column},{row}] during merge check.");
                return;
            }

            var number = block.Definition.number;
            bool mergeUp = false, mergeDown = false, mergeLeft = false, mergeRight = false;

            var sameCount = 0;

            // Check adjacent cells
            if (row < Rows - 1 && _grid[column, row + 1]?.number == number)
            {
                mergeDown = true;
                sameCount++;
            }

            if (row > 0 && _grid[column, row - 1]?.number == number)
            {
                mergeUp = true;
                sameCount++;
            }

            if (column < Columns - 1 && _grid[column + 1, row]?.number == number)
            {
                mergeLeft = true;
                sameCount++;
            }

            if (column > 0 && _grid[column - 1, row]?.number == number)
            {
                mergeRight = true;
                sameCount++;
            }

            if (sameCount < 1) return;
            
            var newAmount = sameCount switch
            {
                1 => number * 2,
                2 => number * 4,
                3 => number * 8,
                _ => 0
            };
            
            if(newAmount == 0) return;
                
            var newBlock = GetBlockDefinitionByNumber(newAmount);
            
            if (mergeDown)
            {
                var mergedBlock = GetBlockAtPosition(column, row + 1);
                if (mergedBlock != null)
                {
                    _grid[column, row + 1] = null;
                    Destroy(mergedBlock.gameObject);
                }
            }

            if (mergeUp)
            {
                Debug.LogWarning("Merged with a block above it??? (shouldn't happen)");
                var mergedBlock = GetBlockAtPosition(column, row - 1);
                if (mergedBlock != null)
                {
                    _grid[column, row - 1] = null;
                    Destroy(mergedBlock.gameObject);
                }
            }

            if (mergeLeft)
            {
                var mergedBlock = GetBlockAtPosition(column + 1, row);
                if (mergedBlock != null)
                {
                    _grid[column + 1, row] = null;
                    Destroy(mergedBlock.gameObject);
                }
            }

            if (mergeRight)
            {
                var mergedBlock = GetBlockAtPosition(column - 1, row);
                if (mergedBlock != null)
                {
                    _grid[column - 1, row] = null;
                    Destroy(mergedBlock.gameObject);
                }
            }
            _grid[column, row] = newBlock;
            block.SetDefinition(newBlock);
            ProcessGrid((column, row)); // On merge reprocess Grid
        }

        private void ProcessFloatingBlocks()
        {
            var floatingBlocks = CheckForFloatingBlocks(); // Check for all floating blocks
            if (floatingBlocks.Count <= 0) return; // Return if no blocks are floating
            
            foreach (var block in floatingBlocks) // Drop them
            {
                _grid[block.Item1, block.Item2] = null; // Remove block from grid
                var floatingBlock = GetBlockAtPosition(block.Item1, block.Item2);
                if (floatingBlock != null)
                {
                    StartCoroutine(DropBlock(floatingBlock, block.Item1, block.Item2));
                }
                else
                {
                    Debug.LogError($"No block found at position [{block.Item1},{block.Item2}] during floating block drop.");
                }
            }
        }

        private List<(int, int)> CheckForFloatingBlocks()
        {
            var floatingBlocks = new List<(int, int)>();
            for (var column = 0; column < Columns; column++)
            {
                for (var row = 0; row < Rows-1; row++)
                {
                    if (_grid[column, row] != null && _grid[column, row + 1] == null)
                    {
                        floatingBlocks.Add((column, row));
                    }
                }
            }
    
            return floatingBlocks;
        }

        private BlockDefinition GetRandomBlockDefinition()
        {
            return _currentSpawningPool[Random.Range(0, _currentSpawningPool.Count)];
        }
        
        private Block GetBlockAtPosition(int column, int row)
        {
            if (column < 0 || column >= _grid.GetLength(0) || row < 0 || row >= _grid.GetLength(1))
            {
                Debug.LogError($"Invalid position: column {column}, row {row}");
                return null;
            }

            var parentTransform = columns[column].transform.GetChild(row);
            if (parentTransform.childCount > 0)
            {
                return parentTransform.GetComponentInChildren<Block>();
            }
            Debug.LogWarning($"No block found at column: {column}, row: {row}");
            return null;
        }

        private static BlockDefinition GetBlockDefinitionByNumber(int number)
        {
            return GameManager.Instance.blockDefinitions.blocks.FirstOrDefault(def => def.number == number);
        }
    }
}
