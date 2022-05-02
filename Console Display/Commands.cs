using System;
using System.Collections.Generic;

namespace Project_Info.Console_Display;

public static class Commands
{
    public static void CreateQRCommand(string message, string correction)
    {
        Console.WriteLine("Voulez vous également afficher le QR Code dans la console ?");
        var console = Console.ReadLine();
        var consoleWrite = OpenAI.AskForYesOrNo(console);
        
        var correctionIndex = correction switch
        {
            "L" => 1,
            "M" => 0,
            "Q" => 3,
            "H" => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(correction))
        };
        var qrTest = new QRCode.QRCode(message,correctionIndex );
        if(consoleWrite) ConsoleFunctions.DisplayBoolQRCodeMatrix(qrTest._masksMatrix, qrTest.MaskPattern);
        Functions.WriteImage(qrTest, "../../../images/QRCode.bmp");
    }
    public static void CreateQRCommandAdvanced(string message, string correction, string moduleWidth)
    {
        Console.WriteLine("Voulez vous également afficher le QR Code dans la console ?");
        var console = Console.ReadLine();
        var consoleWrite = OpenAI.AskForYesOrNo(console);
        var correctionIndex = correction switch
        {
            "L" => 1,
            "M" => 0,
            "Q" => 3,
            "H" => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(correction))
        };
        var moduleWidthIndex = Convert.ToInt32(moduleWidth);
        var qrTest = new QRCode.QRCode(message,correctionIndex,moduleWidthIndex );
        if(consoleWrite) ConsoleFunctions.DisplayBoolQRCodeMatrix(qrTest._masksMatrix, qrTest.MaskPattern);
        Functions.WriteImage(qrTest, "../../../images/QRCode.bmp");
    }

    public static void ReadQRCommand(string path)
    {
        Console.WriteLine("Voici le message contenu dans le QR code : ");
        var qr = new QRCode.QRReader(path);
        
    }
    public static void CreateFractaleCommand()
    {
        var im = new Image(500, 500);
        var fractale = Functions.Fractal(im);
        Functions.WriteImage(fractale, "../../../images/Fractale.bmp");
    }

    public static void CreateHistogrammeCommand(string path)
    {
        var im = Functions.ReadImage(path);
        var histo = Functions.Histograme(im);
        Functions.WriteImage(histo, "../../../images/Histogramme.bmp");
    }

    public static void MaximizeCommand(string path, string factor)
    {
        var im = Functions.ReadImage(path);
        im.Maximize(Convert.ToDouble(factor));
        Functions.WriteImage(im, "../../../images/Agrandi.bmp");
    }
    public static void MinimizeCommand(string path, string factor)
    {
        var im = Functions.ReadImage(path);
        im.Minimize(Convert.ToDouble(factor));
        Functions.WriteImage(im, "../../../images/Retreci.bmp");
    }

    public static void RotationCommand(string path, string degre)
    {
        var im = Functions.ReadImage(path);
        im.RotateAngle(Convert.ToInt32(degre) * Math.PI / 180);
        Functions.WriteImage(im, "../../../images/Rotation.bmp");
    }

    public static void MirrorCOmmand(string path)
    {
        var im = Functions.ReadImage(path);
        im.Mirror();
        Functions.WriteImage(im, "../../../images/Miroir.bmp");
    }

    public static void ConvertToGreyCommand(string path)
    {
        var im = Functions.ReadImage(path);
        im.ConvertToGrey();
        Functions.WriteImage(im, "../../../images/Gris.bmp");
    }
    public static void ConvolutionFilterCommand(string path, string type)
    {
        var im = Functions.ReadImage(path);
        if (type == "Sobel")
        {
            im.DoubleConvolutionFilter(Kernel.SobelX,Kernel.SobelY,1);
        }
        else
        {
            var kernel = type switch
            {
                "Flou" => Kernel.Flou,
                "Contour" => Kernel.Contour,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
            im.ConvolutionFilter(kernel, 1);
        }
        Functions.WriteImage(im, "../../../images/Miroir.bmp");
    }

    public static void HideCommand(string hide, string smol)
    {
        var bigImage = Functions.ReadImage(hide);
        var smallImage = Functions.ReadImage(smol);
        var hidden = Functions.Hide(bigImage, smallImage);
        Functions.WriteImage(hidden, "../../../images/Hidden.bmp");
    }

    public static void FoundCommand(string path)
    {
        var im = Functions.ReadImage(path);
        var found = Functions.Found(im);
        Functions.WriteImage(found, "../../../images/Found.bmp");
    }
    
    
}