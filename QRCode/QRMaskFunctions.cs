namespace Project_Info.QRCode
{
    public static class QRMaskFunctions
    {
        public static int[] Evaluation1(bool[,,] matrix)
        {
            var resultVector = new int[matrix.GetLength(2) - 1];
            for (int k = 0; k < matrix.GetLength(2) - 1; k++)
            {
                for (int index = 0; index < matrix.GetLength(0); index++)
                {
                    var horizontalPenalty = 0;
                    var verticalPenalty = 0;
                    var blackCountH = 0;
                    var blackCountV = 0;
                    
                }
            }
            return resultVector;
        }
    }
}