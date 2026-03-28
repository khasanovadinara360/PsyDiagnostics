using System;

namespace PsyDiagnostics.Services
{
    public class AiService
    {
        public int NormalizeScore(int rawScore, int maxScore)
        {
            if (maxScore <= 0)
                return 0;

            var normalized = (int)((rawScore / (double)maxScore) * 100);

            if (normalized < 0) normalized = 0;
            if (normalized > 100) normalized = 100;

            return normalized;
        }

        public int PredictRecidivism(int score)
        {
            return score >= 60 ? 1 : 0;
        }
    }
}