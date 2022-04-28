namespace Project_Info
{
    class Program
    {
        static void Main()
        {
            QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            //var QRTest = new QRCode(1, 0, 1, 5, 1, "NIQUE TA RACE");
            var QRTest = new QRCode("https://www.notion.so/fr-fr", 3);
            Functions.WriteImage(QRTest, "../../../images/Test7.bmp");
        }
    }
}