public static class EnemyBrainFactory
{
    public static IEnemyBrain Create(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Ghost: return new GhostChargeBrain();
            case MonsterType.Skeleton: return new DefaultForwardBrain();
            case MonsterType.Bat: return new DefaultForwardBrain();
            case MonsterType.Slime: return new DefaultForwardBrain();
            case MonsterType.Crab: return new CrabBrain();
            case MonsterType.Worm: return new WormBrain();
            default: return new DefaultForwardBrain();
        }
    }
}
