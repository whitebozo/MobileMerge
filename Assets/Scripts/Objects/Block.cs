using UnityEngine;
using UnityEngine.UI;

namespace Merge.Objects
{
    public class Block : MonoBehaviour
    {
        public BlockDefinition definition;
        public Image blockImage;

        public void SetDefinition(BlockDefinition newDefinition)
        {
            definition = newDefinition;
            blockImage.color = definition.color;
            blockImage.sprite = definition.image;
        }

        public int GetNumber()
        {
            return definition.number;
        }
    }
}