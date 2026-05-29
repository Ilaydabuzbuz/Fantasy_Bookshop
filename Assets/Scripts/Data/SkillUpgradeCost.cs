public static class SkillUpgradeCost
{
    // 0 -> 1, 1 -> 2, 2 -> 3, 3 -> 4, 4 -> 5
    private static readonly int[] costs = { 100, 200, 350, 550, 800 };

    public static int GetCostForLevel(int currentLevel)
    {
        if (currentLevel < 0 || currentLevel >= costs.Length)
            return 0;

        return costs[currentLevel];
    }
}