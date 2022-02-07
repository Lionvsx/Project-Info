namespace Project_Info
{
    public static class Kernel
    {
        public static double[,] SobelX
        {
            get
            {
                return new double[,]
                {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };
            }
        }
        
        public static double[,] SobelY
        {
            get
            {
                return new double[,]
                {
                    {  1,  2,  1 },
                    {  0,  0,  0 },
                    { -1, -2, -1 }
                };
            }
        }
        public static double[,] Contour
        {
            get
            {
                return new double[,]
                {
                    {  0,  1,  0 },
                    {  1, -4,  1 },
                    {  0,  1,  0 }
                };
            }
        }
        
        public static double[,] Flou
        {
            get
            {
                return new double[,]
                {
                    {  1,  1,  1 },
                    {  1,  1,  1 },
                    {  1,  1,  1 }
                };
            }
        }
    }
}