using System;

namespace DistributedPrimeCalculatorApp
{
    [Serializable]
    public class Batch
    {
        private readonly long _minimum;
        private readonly long _maximum;

        public Batch(long minimum, long maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        public long Maximum => _maximum;
        public long Minimum => _minimum;
    }
}