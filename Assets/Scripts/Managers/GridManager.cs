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
            var rect = canvasRectTransform.rect;
            var columnWidth = rect.width / columns.Length;
            var column = Mathf.Clamp((int)((localPosition.x + rect.width / 2) / columnWidth), 0, columns.Length - 1);

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
            Debug.Log($"Block dropped from [{column},{startingRow}] => [{column},{row}]");
            if (row < Rows)
            {
                _grid[column, row] = block.definition;
                ProcessGrid((column, row));
            }
        }
        
        void ProcessGrid((int,int) lastDrop)
        {
            if (CheckForMerges(lastDrop)) //If any merge happened on drop
            {
                var floatingBlocks = CheckForFloatingBlocks(); //Check for all floating blocks
                if (floatingBlocks.Count > 0) //If there is any...
                {
                    Debug.Log($"After merge there was ({floatingBlocks.Count}) floating block/s that needed to drop");
                    foreach (var block in floatingBlocks) //Drop them and have them recall ProcessGrid
                    {
                        StartCoroutine(DropBlock(GetBlockAtPosition(block.Item1, block.Item2), block.Item1, block.Item2));
                    }
                }
            }
        }

        bool CheckForMerges((int,int) lastDrop)
        {
            var column = lastDrop.Item1;
            var row = lastDrop.Item2;
            
            var merged = false;
            var block = GetBlockAtPosition(column, row);
            var number = block.definition.number;
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

            if (sameCount >= 1)
            {
                var newAmount = sameCount == 1 ? number * 2 : number * 4;
                var newBlock = GetBlockDefinitionByNumber(newAmount);
                
                Debug.Log($"Block({number}) - Merged with {sameCount} block/s make a new Block({newAmount})");

                if (mergeDown)
                {
                    Debug.Log("Merged with a block below it");
                    var mergedBlock = GetBlockAtPosition(column, row + 1);
                    _grid[column, row + 1] = null;
                    Destroy(mergedBlock.gameObject);
                }

                if (mergeUp)
                {
                    Debug.LogWarning("Merged with a block above it??? (shouldn't happen)");
                    var mergedBlock = GetBlockAtPosition(column, row - 1);
                    _grid[column, row - 1] = null;
                    Destroy(mergedBlock.gameObject);
                }

                if (mergeLeft)
                {
                    Debug.Log("Merged with a block to the left");
                    var mergedBlock = GetBlockAtPosition(column + 1, row);
                    _grid[column + 1, row] = null;
                    Destroy(mergedBlock.gameObject);
                }

                if (mergeRight)
                {
                    Debug.Log("Merged with a block to the right");
                    var mergedBlock = GetBlockAtPosition(column - 1, row);
                    _grid[column - 1, row] = null;
                    Destroy(mergedBlock.gameObject);
                }
                _grid[column, row] = newBlock;
                block.SetDefinition(newBlock);
                merged = true;
            }

            return merged;
        }

        private List<(int, int)> CheckForFloatingBlocks()
        {
            var floatingBlocks = new List<(int, int)>();
            for (var column = 0; column < Columns; column++)
            {
                for (var row = 0; row < Rows; row++)
                {
                    if (_grid[column, row] != null)
                    {
                        if (row > 0 && _grid[column, row - 1] == null)
                        {
                            _grid[column, row] = null;
                            floatingBlocks.Add((column,row));
                        }
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
