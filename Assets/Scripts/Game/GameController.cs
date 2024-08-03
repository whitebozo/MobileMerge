using Merge.Managers;
using UnityEngine;

namespace Merge.Game
{
    public class GameController : MonoBehaviour
    {
        public RectTransform canvasRectTransform;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Convert mouse position to a position relative to the canvas
                Vector2 mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, null, out mousePosition);

                GameManager.Instance.gridManager.HandleMouseClick(mousePosition, canvasRectTransform);
            }
        }
    }
}