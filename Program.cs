namespace Project_Info
{
    class Program
    {
        static void Main(string[] args)
        {
            var coco = Functions.ReadImage("../../../images/coco.bmp");
            var hist = Functions.Histograme(coco);
            Functions.WriteImage(hist,"../../../images/Hist.bmp");
            
            QRCode.InitializeAlphaNumericTable();
            //var test = Functions.ReadImage("../../../images/Test.bmp");
            var bjr = Functions.ConvertIntToBinaryArray(1);
            var QRTest = new QRCode(2, 5, 1);
            Functions.WriteImage(QRTest, "../../../images/Test7.bmp");
            
        }
    }
}