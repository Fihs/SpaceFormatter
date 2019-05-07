using System;

namespace SpaceFormatter
{
    // http://www.jasinskionline.com/technicalwiki/Estimated-Time-of-Completion-Calculation-C.ashx
    public class EtcCalculator
    {
        private readonly DateTime _startTime;
        private readonly int _imax; // upper bound of the zero-based array being processed

        /// <summary>
        ///
        /// </summary>
        /// <param name="maxIndex">upper bound of the zero-based array being processed</param>
        public EtcCalculator(int maxIndex)
        {
            _imax = maxIndex;
            _startTime = DateTime.Now;
        }

        public TimeSpan GetEtc(int iterationsDone)
        {
            var iterationsTotal = _imax + 1;
            var msElapsed = DateTime.Now.Subtract(_startTime).TotalMilliseconds;
            var unitTime = msElapsed / (double)iterationsDone;
            var etc = TimeSpan.FromMilliseconds(unitTime * (iterationsTotal - iterationsDone));
            return etc;
        }
    }
}