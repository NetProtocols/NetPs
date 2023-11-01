namespace NetPs.Socket.Extras.Security
{
    using System;
    public struct RandomSource
    {
        internal System.Random random { get; set; }
        public static RandomSource New(int seed)
        {
            var random = new RandomSource();
            random.random = new System.Random(seed);
            return random;
        }
        public int Rand()
        {
            return random.Next();
        }
    }
}
