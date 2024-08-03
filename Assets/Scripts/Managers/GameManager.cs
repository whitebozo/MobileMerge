using Merge.Objects.ScriptableObjects;
using UnityEngine;

namespace Merge.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GridManager gridManager;
        public ScoreManager scoreManager;
        public UIManager uiManager;

        public BlockDefinitions blockDefinitions;
        public Milestones milestones;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            gridManager.Initialize();
            scoreManager.Initialize();
            uiManager.Initialize();
        }
    }
}