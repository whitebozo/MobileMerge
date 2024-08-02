using Merge.Objects;
using Merge.Objects.ScriptableObjects;
using UnityEngine;

namespace Merge.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        private int _score;
        private int _highestBlockNumber;
        private int _currentMilestoneIndex = 0;

        public void Initialize()
        {
            _score = 0;
            _highestBlockNumber = 0;
            UpdateScoreUI();
        }

        public void AddScore(int points)
        {
            _score += points;
            UpdateScoreUI();
        }

        public int GetScore()
        {
            return _score;
        }

        public void UpdateHighestBlockNumber(int newNumber)
        {
            if (newNumber > _highestBlockNumber)
            {
                _highestBlockNumber = newNumber;
                CheckMilestones();
            }
        }

        void CheckMilestones()
        {
            var milestones = GameManager.Instance.milestones;

            while (_currentMilestoneIndex < milestones.milestones.Length &&
                   _highestBlockNumber >= milestones.milestones[_currentMilestoneIndex].targetScore)
            {
                var milestone = milestones.milestones[_currentMilestoneIndex];
                GameManager.Instance.gridManager.AddBlocksToSpawningPool(milestone.blocksToAdd);
                foreach (var blockToRemove in milestone.blocksToRemove)
                {
                    GameManager.Instance.gridManager.RemoveBlockFromSpawningPool(blockToRemove);
                }
                _currentMilestoneIndex++;
            }
        }

        void UpdateScoreUI()
        {
            GameManager.Instance.uiManager.UpdateScoreText(_score);
        }
    }
}