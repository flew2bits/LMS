using System;
using IronBarCode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();

app.MapRazorPages();
app.Run();

namespace IronBarCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var barcode = BarcodeWriter.CreateBarcode("JCPS barcode", BarcodeEncoding.Code128);
            barcode.AddBarcodeValueTextBelowBarcode();
            barcode.SaveAsPng(("Barcode.png"));
        }
    }
}