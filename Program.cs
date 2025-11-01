using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

using System.IO;
namespace ZatcaProtection
{
   

    // مثال استخدام
    //class Program
    //{
    //    static void Main()
    //    {
    //        string number1 = "123456789012345"; // 15 رقم
    //        string number2 = "98765432101234567890"; // أي طول 10-30 رقم
    //        string password = "My$trongPassword!";

    //        string encrypted = SecureNumberEncryptor.EncryptNumbers(number1, number2, password);
    //        Console.WriteLine("Encrypted: " + encrypted);

    //        var (n1, n2) = SecureNumberEncryptor.DecryptNumbers(encrypted, password);
    //        Console.WriteLine("Decrypted Number1: " + n1);
    //        Console.WriteLine("Decrypted Number2: " + n2);
    //    }
    //}

    // ===== Program (ديمو سريع) =====
    static class Program
    {
        static void GenerateKey()
        {
            string number1 = "325789641265873";   // 15 رقم
            string number2 = "10106792154318795"; // 11+ رقم

            //string pass1 = "Password1!Strong";
            //string pass2 = "Password2!Stronger";
            //string pass3 = "Password3!Strongest";
            //string stegoKey = "HideItLikeAPro!";
            string pass1 = "My$trongPassword1!As0101105976@as1^Programmer_Comsys-Imcanat.";
            string pass2 = "My$trongPassword2!As0101105976@as1^ProgrammerZatca_Osus-Perfect.";
            string pass3 = "My$trongPassword3!As0101105976@as1^ProgrammerNSV_Silicon-BarTech.";
            string stegoKey = "HideItLikeAPro1!As0101105976@as1^Programmer_Vb6-Android^.Net.";

            // نقرأ بيانات JSON من ملف
            string jsonPath = "data.json";
            string jsonData = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "{ \"menu\": { " +
              "  \"id\": \"file\", " +
              "  \"value\": \"File\", " +
              "  \"popup\": { " +
              "    \"menuitem\": [ " +
              "      { \"value\": \"New\", \"onclick\": \"CreateNewDoc()\" }, " +
              "      { \"value\": \"Open\", \"onclick\": \"OpenDoc()\" }, " +
              "      { \"value\": \"Close\", \"onclick\": \"CloseDoc()\" } " +
              "    ] " +
              "  } " +
              "} " +
              "}";

            // 1) التشفير ثلاثي (مع JSON)
            string finalEncrypted = MultiLayerEncryptorForKey.EncryptTriple(number1, number2, jsonData, pass1, pass2, pass3);

            // 2) إخفاء
            string stego = StegoObfuscatorForKey.Hide(finalEncrypted, stegoKey, StegoObfuscatorForKey.DefaultCoverLength);
            Console.WriteLine("Stego Text Length = " + stego.Length);

            // 3) استخراج
            string extracted = StegoObfuscatorForKey.Extract(stego, stegoKey, StegoObfuscatorForKey.DefaultCoverLength);

            // 4) فك التشفير
            var (rec1, rec2, recJson) = MultiLayerEncryptorForKey.DecryptTriple(extracted, pass1, pass2, pass3);

            Console.WriteLine("Recovered Number1: " + rec1);
            Console.WriteLine("Recovered Number2: " + rec2);

            // حفظ JSON المسترجع
            File.WriteAllText("recovered.json", recJson);
            Console.WriteLine("Recovered JSON saved to recovered.json");
        }

        static void GenerateLicence()
        {
            string number1 = "325789641265873";
            string number2 = "10106792154318795";
            DateTime date = new DateTime(2025, 8, 11);

            string pass1 = "My$trongPassword1!As0101105976@as1^Programmer_Comsys-Imcanat.";
            string pass2 = "My$trongPassword2!As0101105976@as1^ProgrammerZatca_Osus-Perfect.";
            string pass3 = "My$trongPassword3!As0101105976@as1^ProgrammerNSV_Silicon-BarTech.";
            string stegoKey = "HideItLikeAPro1!As0101105976@as1^Programmer_Vb6-Android^.Net.";

            // 1) تشفير ثلاثي + التاريخ
            string finalEncrypted = MultiLayerEncryptorForLicence.EncryptTriple(number1, number2, date, pass1, pass2, pass3);

            // 2) الإخفاء
            string stego = StegoObfuscatorForLicence.Hide(finalEncrypted, stegoKey, StegoObfuscatorForLicence.DefaultCoverLength);
            Console.WriteLine("Stego Text (length = " + stego.Length + "):");
            Console.WriteLine(stego);

            // ===== في وقت لاحق =====
            // 3) استخراج
            string extractedEncrypted = StegoObfuscatorForLicence.Extract(stego, stegoKey, StegoObfuscatorForLicence.DefaultCoverLength);

            // 4) فك التشفير الثلاثي
            var (n1, n2, d) = MultiLayerEncryptorForLicence.DecryptTriple(extractedEncrypted, pass1, pass2, pass3);

            Console.WriteLine("\nRecovered Number1: " + n1);
            Console.WriteLine("Recovered Number2: " + n2);
            Console.WriteLine("Recovered Date   : " + d.ToString("yyyy-MM-dd"));

        }
        static void Main()
        {
            //GenerateKey();
            //GenerateLicence();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Introfrm());
        }
    }

}
