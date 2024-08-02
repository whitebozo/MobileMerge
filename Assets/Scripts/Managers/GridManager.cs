using System.Collections;
using System.Collections.Generic;
using Merge.Game;
using Merge.Objects;
using UnityEngine;

namespace Merge.Managers
{
    public class GridManager : MonoBehaviour
    {
        public int columns = 4;
        public int rows = 7;
        public GameObject blockPrefab;
        public NextBlockPreview nextBlockPreview;

        private List<BlockDefinition> _currentSpawningPool;
        private Block[,] _grid;

        public void Initialize()
        {
            _grid = new Block[columns, rows];
            _currentSpawningPool = new List<BlockDefinition>();
            InitializeSpawningPool();
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        void InitializeSpawningPool()
        {
            // Initial pool setup
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

        public void RemoveBlockFromSpawningPool(int number)
        {
            var def = GetBlockDefinitionByNumber(number);
            if (def != null && _currentSpawningPool.Contains(def))
            {
                _currentSpawningPool.Remove(def);
            }
        }

        public void SpawnNewBlock(int column)
        {
            var blockDefinition = nextBlockPreview.GetNextBlockDefinition();
            var blockObj = Instantiate(blockPrefab, new Vector2(column, rows - 1), Quaternion.identity);
            var block = blockObj.GetComponent<Block>();
            block.SetDefinition(blockDefinition);
            StartCoroutine(DropBlock(block, column));
            nextBlockPreview.UpdatePreviewBlock(GetRandomBlockDefinition());
        }

        IEnumerator DropBlock(Block block, int column)
        {
            var row = rows - 1;
            while (row >= 0 && _grid[column, row] == null)
            {
                block.transform.position = new Vector2(column, row);
                yield return new WaitForSeconds(0.1f);
                row--;
            }

            row++;
            if (row < rows)
            {
                _grid[column, row] = block;
                CheckMerge(column, row);
            }
        }

        void CheckMerge(int column, int row)
        {
            var sameBlocks = new List<Block>();
            var number = _grid[column, row].GetNumber();

            // Check vertically
            for (var r = row - 1; r <= row + 1; r++)
            {
                if (r >= 0 && r < rows && _grid[column, r] != null && _grid[column, r].GetNumber() == number)
                    sameBlocks.Add(_grid[column, r]);
            }

            // Check horizontally
            for (var c = column - 1; c <= column + 1; c++)
            {
                if (c >= 0 && c < columns && _grid[c, row] != null && _grid[c, row].GetNumber() == number)
                    sameBlocks.Add(_grid[c, row]);
            }

            if (sameBlocks.Count >= 2)
            {
                int newNumber = 0;

                // Calculate new number based on the number of blocks touching
                if (sameBlocks.Count == 2)
                {
                    newNumber = number + number; // n + n
                }
                else if (sameBlocks.Count >= 3)
                {
                    newNumber = number * 4; // n * 4
                }

                foreach (var b in sameBlocks)
                {
                    _grid[(int)b.transform.position.x, (int)b.transform.position.y] = null;
                    Destroy(b.gameObject);
                }

                _grid[column, row].SetDefinition(GetBlockDefinitionByNumber(newNumber));
                CheckMerge(column, row); // Check again for further merges

                // Update score
                GameManager.Instance.scoreManager.AddScore(newNumber);

                // Update highest block number
                GameManager.Instance.scoreManager.UpdateHighestBlockNumber(newNumber);
            }
        }

        BlockDefinition GetRandomBlockDefinition()
        {
            return _currentSpawningPool[Random.Range(0, _currentSpawningPool.Count)];
        }

        BlockDefinition GetBlockDefinitionByNumber(int number)
        {
            foreach (var def in GameManager.Instance.blockDefinitions.blocks)
            {
                if (def.number == number)
                    return def;
            }
            return null; // Handle the case where no definition is found
        }
    }
}
