namespace Project_Info
{
    class Program
    {
        static void Main(string[] args)
        {
            QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            //var QRTest = new QRCode(1, 0, 1, 5, 1, "NIQUE TA RACE");
            var QRTest = new QRCode("hello world!", 2);
            Functions.WriteImage(QRTest, "../../../images/Test7.bmp");
        }
    }
}