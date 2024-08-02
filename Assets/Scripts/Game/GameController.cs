using Merge.Managers;
using UnityEngine;

namespace Merge.Game
{
    public class GameController : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var column = Mathf.Clamp((int)mousePosition.x, 0, GameManager.Instance.gridManager.columns - 1);
                GameManager.Instance.gridManager.SpawnNewBlock(column);
            }
        }
    }
}