using System;
using System.Threading;

namespace RescueScoreManager.Helpers
{
    public static class IdGenerator
    {
        private static int _lastRaceFormatConfigurationId = 0;
        private static int _lastRaceFormatDetailId = 0;
        private static readonly object _lock = new object();

        public static int GenerateRaceFormatConfigurationId()
        {
            lock (_lock)
            {
                return Interlocked.Increment(ref _lastRaceFormatConfigurationId);
            }
        }

        public static int GenerateRaceFormatDetailId()
        {
            lock (_lock)
            {
                return Interlocked.Increment(ref _lastRaceFormatDetailId);
            }
        }

        public static void InitializeFrom(IEnumerable<Data.RaceFormatConfiguration> existingConfigurations)
        {
            lock (_lock)
            {
                if (existingConfigurations?.Any() == true)
                {
                    _lastRaceFormatConfigurationId = existingConfigurations.Max(c => c.Id);
                    
                    var allDetails = existingConfigurations
                        .Where(c => c.RaceFormatDetails?.Any() == true)
                        .SelectMany(c => c.RaceFormatDetails);
                        
                    if (allDetails.Any())
                    {
                        _lastRaceFormatDetailId = allDetails.Max(d => d.Id);
                    }
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _lastRaceFormatConfigurationId = 0;
                _lastRaceFormatDetailId = 0;
            }
        }
    }
}