namespace Merge.Objects
{
    [System.Serializable]
    public class Milestone
    {
        public int targetScore;
        public int[] blocksToAdd;
        public int[] blocksToRemove;
    }
}