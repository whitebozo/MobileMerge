using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge.Objects
{
    public class Block : MonoBehaviour
    {
        public BlockDefinition Definition { get; private set; }
        [SerializeField] private Image blockImage;
        [SerializeField] private TMP_Text blockValue;

        public void SetDefinition(BlockDefinition newDefinition)
        {
            Definition = newDefinition;
            blockImage.sprite = Definition.image;
            blockValue.text = Definition.number.ToString();
        }
    }
}