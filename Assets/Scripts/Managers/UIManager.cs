using TMPro;
using UnityEngine;

namespace Merge.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        public void Initialize()
        {
            UpdateScoreText(0);
        }

        public void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score.ToString();
            }
        }
    }
}