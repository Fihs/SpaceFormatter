using System;

namespace SpaceFormatter.Core.Utils
{
    // http://www.jasinskionline.com/technicalwiki/Estimated-Time-of-Completion-Calculation-C.ashx
    public class EtcCalculator
    {
        private readonly DateTime _startTime;

        /// <summary>
        ///
        /// </summary>
        /// <param name="maxIndex">Upper bound of the zero-based array being processed</param>
        public EtcCalculator(long maxIndex)
        {
            MaxIndex = maxIndex;
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Upper bound of the zero-based array being processed
        /// </summary>
        public long MaxIndex { get; set; }

        public TimeSpan GetEtc(long iterationsDone)
        {
            var iterationsTotal = MaxIndex + 1;
            var msElapsed = DateTime.Now.Subtract(_startTime).TotalMilliseconds;
            var unitTime = msElapsed / (double)iterationsDone;
            var etc = TimeSpan.FromMilliseconds(unitTime * (iterationsTotal - iterationsDone));
            return etc;
        }
    }
}