using System;

namespace MiningForge
{
    [Serializable]
    public class MiningConfig
    {
        public int[] centers = { 50, 60, 40 };
        public int halfWidth = 5, focus = 32, separationMargin = 15, fatalMargin = 20;
        public int criticalPercent = 10;
        public int[] minimum = { 10, 24, 4, 10 };
        public int[] maximum = { 14, 32, 6, 14 };
        public int[] costs = { 2, 4, 2, 5 };
    }

    public enum NodeState { Active, Collected, Destroyed, Abandoned }
    public struct Strike
    {
        public int part, amount;
        public bool critical, enteredBand, overBand;
    }

    // All gameplay randomness is owned by the node; opening UI never recreates it.
    public sealed class MiningSession
    {
        public readonly MiningConfig Config;
        public readonly string NodeId;
        private readonly int[] values = new int[3];
        private readonly Random random;
        public int Focus { get; private set; }
        public NodeState State { get; private set; }
        public bool Busy { get; private set; }
        public string RewardId { get; private set; }
        public int Actions { get; private set; }
        public MiningSession(MiningConfig config, int seed)
        {
            Config = config; Focus = config.focus; random = new Random(seed);
            NodeId = Guid.NewGuid().ToString("N");
        }
        public int Value(int part) => values[part];
        public int Lower(int part) => Config.centers[part] - Config.halfWidth;
        public int Upper(int part) => Config.centers[part] + Config.halfWidth;
        public int Separation(int part) => Lower(part) - Config.separationMargin;
        public int Fatal(int part) => Upper(part) + Config.fatalMargin;
        public bool InBand(int part) => values[part] >= Lower(part) && values[part] <= Upper(part);
        public static int PartQuality(int value, int lower, int upper) =>
            Math.Max(0, value < lower ? 100 - 2 * (lower - value) : value > upper ? 100 - 5 * (value - upper) : 100);
        public int Quality => (int)Math.Floor((PartQuality(values[0], Lower(0), Upper(0)) +
            PartQuality(values[1], Lower(1), Upper(1)) + PartQuality(values[2], Lower(2), Upper(2))) / 3.0 + 0.5);
        public static string Rank(int q) => q >= 80 ? "上質" : q >= 40 ? "通常" : "損傷品";
        public bool CanCollect => State == NodeState.Active && !Busy &&
            values[0] >= Separation(0) && values[1] >= Separation(1) && values[2] >= Separation(2);
        public bool CanStrike(int skill) => skill >= 0 && skill < 4 && State == NodeState.Active && !Busy && Focus >= Config.costs[skill];
        public static int ResolveAmount(int current, int center, int roll, bool critical) =>
            critical && current < center ? Math.Min(roll * 2, center - current) : roll;

        public Strike[] TryStrike(int skill, int selected)
        {
            if (!CanStrike(skill) || selected < 0 || selected > 2) return Array.Empty<Strike>();
            Busy = true; Focus -= Config.costs[skill]; Actions++;
            var hits = new Strike[skill == 3 ? 3 : 1];
            for (int i = 0; i < hits.Length; i++)
            {
                int p = skill == 3 ? i : selected;
                int roll = random.Next(Config.minimum[skill], Config.maximum[skill] + 1);
                bool critical = skill < 2 && values[p] < Config.centers[p] && random.Next(100) < Config.criticalPercent;
                bool wasInBand = InBand(p);
                int amount = ResolveAmount(values[p], Config.centers[p], roll, critical);
                values[p] += amount;
                hits[i] = new Strike { part = p, amount = amount, critical = critical,
                    enteredBand = !wasInBand && InBand(p), overBand = values[p] > Upper(p) };
                if (values[p] >= Fatal(p)) State = NodeState.Destroyed;
            }
            return hits;
        }
        public void FinishAnimation() { Busy = false; }
        public bool TryCollect()
        {
            if (!CanCollect) return false;
            State = NodeState.Collected; RewardId = NodeId + ":crystal"; return true;
        }
        public bool TryAbandon()
        {
            if (State != NodeState.Active || Busy) return false;
            State = NodeState.Abandoned; return true;
        }
    }
}
