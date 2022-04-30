using System;

namespace Project_Info.QRCode
{
    public static class QRMaskFunctions
    {

        public static int[] PerformMaskEvaluations(bool[,,] masksMatrix)
        {
            var resultVector = new int[masksMatrix.GetLength(2)];
            var vector1 = Evaluation1(masksMatrix);
            var vector2 = Evaluation2(masksMatrix);
            var vector3 = Evaluation3(masksMatrix);
            var vector4 = Evaluation4(masksMatrix);

            for (var i = 0; i < resultVector.Length; i++)
            {
                resultVector[i] = vector1[i] + vector2[i] + vector3[i] + vector4[i];
            }
            return resultVector;
        }
        public static int[] Evaluation1(bool[,,] masksMatrix)
        {
            var resultVector = new int[masksMatrix.GetLength(2)];
            for (int k = 0; k < masksMatrix.GetLength(2); k++)
            {
                var horizontalPenalty = 0;
                var verticalPenalty = 0;

                for (int i = 0; i < masksMatrix.GetLength(1); i++)
                {
                    var blackCountH = 0;
                    var blackCountV = 0;
                    for (int index = 0; index < masksMatrix.GetLength(0); index++)
                    {
                        if (masksMatrix[index, i, k]) blackCountV++;
                        else
                        {
                            verticalPenalty = blackCountV >= 5 ? 3 + (blackCountV - 5): verticalPenalty;
                            blackCountV = 0;
                        }
                        if (masksMatrix[i, index, k]) blackCountH++;
                        else
                        {
                            horizontalPenalty = blackCountH >= 5 ? 3 + (blackCountH - 5): horizontalPenalty;
                            blackCountH = 0;
                        }

                    }
                }
                resultVector[k] = horizontalPenalty + verticalPenalty;
            }
            return resultVector;
        }

        public static int[] Evaluation2(bool[,,] masksMatrix)
        {
            var resultVector = new int[masksMatrix.GetLength(2)];

            for (int k = 0; k < resultVector.Length; k++)
            {
                var numberOf2X2Blocks = 0;
                for (int line = 0; line < masksMatrix.GetLength(0) - 2; line++)
                {
                    for (int col = 0; col < masksMatrix.GetLength(1) - 2; col++)
                    {
                        if (Discover2X2Block(masksMatrix, line, col, k)) numberOf2X2Blocks++;
                    }
                }
                resultVector[k] = numberOf2X2Blocks * 3;
            }
            
            return resultVector;
        }

        public static int[] Evaluation3(bool[,,] masksMatrix)
        {
            var resultVector = new int[masksMatrix.GetLength(2)];
            for (int k = 0; k < resultVector.Length; k++)
            {
                var penaltyScore = 0;
                for (int line = 0; line < masksMatrix.GetLength(0) - 2; line++)
                {
                    for (int col = 0; col < masksMatrix.GetLength(1) - 2; col++)
                    {
                        var nbPatterns = DiscoverPattern(masksMatrix, line, col, k);
                        penaltyScore += nbPatterns > 0 ? 40 * nbPatterns : 0;

                    }
                }
                resultVector[k] = penaltyScore;
            }
            
            return resultVector;
        }

        public static int[] Evaluation4(bool[,,] masksMatrix)
        {
            var resultVector = new int[masksMatrix.GetLength(2)];
            
            for (int k = 0; k < resultVector.Length; k++)
            {
                var darkModulePercent = CalculateDarkModulePercent(masksMatrix, k);
                
                var lowNumber = Math.Abs((darkModulePercent / 5) - 50) / 5;
                var highNumber = Math.Abs(((darkModulePercent + 4) / 5) - 50) / 5;

                resultVector[k] = Math.Min(lowNumber, highNumber) * 10;
            }
            
            return resultVector;
        }

        private static int CalculateDarkModulePercent(bool[,,] masksMatrix, int k)
        {
            var darkModuleCount = 0;
            for (var line = 0; line < masksMatrix.GetLength(0); line++)
            {
                for (var col = 0; col < masksMatrix.GetLength(1); col++)
                {
                    if (masksMatrix[line, col, k]) darkModuleCount++;
                }
            }
            return darkModuleCount * 100 / (masksMatrix.GetLength(0) * masksMatrix.GetLength(1));
        }


        private static int DiscoverPattern(bool[,,] masksMatrix, int line, int col, int k)
        {
            var pattern1 = new[] {true, false, true, true, true, false, true, false, false, false, false};
            var pattern2 = new[] {false, false, false, false, true, false, true, true, true, false, true};
            
            var isVerticalPattern = true;
            var isHorizontalPattern = true;
            
            // DISCOVERY PATTERN 1
            if (masksMatrix[line, col, k])
            {
                for (var index = 0; index < pattern1.Length; index++)
                {
                    if (index + line >= masksMatrix.GetLength(0) || masksMatrix[line + index, col, k] != pattern1[index]) isVerticalPattern = false;
                    if (index + col >= masksMatrix.GetLength(1) || masksMatrix[line, col + index, k] != pattern1[index]) isHorizontalPattern = false;
                    if (!isVerticalPattern && !isHorizontalPattern) return 0;
                }
            }
            // DISCOVERY PATTERN 2
            else
            {
                for (var index = 0; index < pattern2.Length; index++)
                {
                    if (index + line >= masksMatrix.GetLength(0) || masksMatrix[line + index, col, k] != pattern2[index]) isVerticalPattern = false;
                    if (index + col >= masksMatrix.GetLength(1) || masksMatrix[line, col + index, k] != pattern2[index]) isHorizontalPattern = false;
                    if (!isVerticalPattern && !isHorizontalPattern) return 0;
                }
            }

            return isVerticalPattern && isHorizontalPattern ? 2 : 1;
        }

        private static bool Discover2X2Block(bool[,,] masksMatrix, int x, int y, int k)
        {
            var count = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (masksMatrix[x + i, y + j, k]) count++;
                }
            }
            return count == 4;
        }
    }
}