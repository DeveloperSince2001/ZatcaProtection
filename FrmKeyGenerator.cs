using System;
using System.IO;
using System.Windows.Forms;

using System.Diagnostics;
namespace ZatcaProtection
{
    public partial class FrmKeyGenerator : Form
    {
        DateTime DateStart = new DateTime(2000, 9, 11, 14, 30, 0); // 11-Sep-2025 14:30:00
        DateTime DateEnd = new DateTime(2000, 9, 11, 14, 45, 30); // 11-Sep-2025 14:45:30
        string _mProccess_Name = "";
        public FrmKeyGenerator()
        {
            InitializeComponent();
            InitializeDefaults();
        }

        private void PrintDifferenceInSeconds()
        {
            DateEnd = DateTime.Now;


            TimeSpan difference = DateEnd - DateStart;

            double seconds = difference.TotalSeconds; // total seconds including fractions
            int wholeSeconds = (int)difference.TotalSeconds; // rounded down to whole seconds

            Debug.WriteLine(_mProccess_Name + " ===> " + $"Difference in seconds (double): {seconds}");
            Debug.WriteLine(_mProccess_Name + " ===> " + $"Difference in seconds (int): {wholeSeconds}");
        }
        private void InitializeDefaults()
        {
            txtPassword1.Text = "My$trongPassword1!As0101105976@as1^Programmer_Comsys-Imcanat.";
            txtPassword2.Text = "My$trongPassword2!As0101105976@as1^ProgrammerZatca_Osus-Perfect.";
            txtPassword3.Text = "My$trongPassword3!As0101105976@as1^ProgrammerNSV_Silicon-BarTech.";
            txtStegoKey.Text = "HideItLikeAPro1!As0101105976@as1^Programmer_Vb6-Android^.Net.";

        }

        private void CleanDefaults()
        {
            txtNumber1.Clear();
            txtNumber2.Clear();
            txtPassword1.Clear();
            txtPassword2.Clear();
            txtPassword3.Clear();
            txtStegoKey.Clear();
            txtJson.Clear();

            // Outputs
            txtEncrypted.Clear();
            txtNumber1Out.Clear();
            txtNumber2Out.Clear();
            txtJsonOut.Clear();

        }
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                DateStart = DateTime.Now;

                string number1 = txtNumber1.Text.Trim();
                string number2 = txtNumber2.Text.Trim();
                string pass1 = txtPassword1.Text.Trim();
                string pass2 = txtPassword2.Text.Trim();
                string pass3 = txtPassword3.Text.Trim();
                string stegoKey = txtStegoKey.Text.Trim();
                string json = "{\"partyIdentification\":{\"ID\":null,\"schemeID\":null},\"postalAddress\":{\"country\":{\"IdentificationCode\":\"SA\"},\"StreetName\":\"street name\",\"AdditionalStreetName\":null,\"BuildingNumber\":\"3724\",\"PlotIdentification\":null,\"CityName\":\"Jeddah\",\"PostalZone\":\"15385\",\"CountrySubentity\":null,\"CitySubdivisionName\":\"Alfalah\"},\"partyLegalEntity\":{\"RegistrationName\":\"مؤسسة المشترى\"},\"partyTaxScheme\":{\"taxScheme\":{\"ID\":\"VAT\"},\"CompanyID\":\"310424415000003\"},\"contact\":{\"ID\":null,\"Name\":null,\"Telephone\":null,\"ElectronicMail\":null,\"Note\":null}}";

                // نقرأ JSON من ملف أو من TextBox
                string jsonData = txtJson.Text.Trim();
                if (string.IsNullOrEmpty(jsonData))
                {
                    jsonData = File.Exists("data.json")
                        ? File.ReadAllText("data.json")
                        : "{\"partyIdentification\":{\"ID\":null,\"schemeID\":null},\"postalAddress\":{\"country\":{\"IdentificationCode\":\"SA\"},\"StreetName\":\"street name\",\"AdditionalStreetName\":null,\"BuildingNumber\":\"3724\",\"PlotIdentification\":null,\"CityName\":\"Jeddah\",\"PostalZone\":\"15385\",\"CountrySubentity\":null,\"CitySubdivisionName\":\"Alfalah\"},\"partyLegalEntity\":{\"RegistrationName\":\"مؤسسة المشترى\"},\"partyTaxScheme\":{\"taxScheme\":{\"ID\":\"VAT\"},\"CompanyID\":\"310424415000003\"},\"contact\":{\"ID\":null,\"Name\":null,\"Telephone\":null,\"ElectronicMail\":null,\"Note\":null}}";
                }

                // تشفير
                string finalEncrypted = MultiLayerEncryptorForKey.EncryptTriple(number1, number2, jsonData, pass1, pass2, pass3);

                // إخفاء
                string stego = StegoObfuscatorForKey.Hide(finalEncrypted, stegoKey, StegoObfuscatorForKey.DefaultCoverLength);

                txtEncrypted.Text = stego;

                _mProccess_Name = "Encrypt";
                PrintDifferenceInSeconds();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Encrypting: " + ex.Message);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                DateStart = DateTime.Now;

                string encryptedStego = txtEncrypted.Text.Trim();
                string pass1 = txtPassword1.Text.Trim();
                string pass2 = txtPassword2.Text.Trim();
                string pass3 = txtPassword3.Text.Trim();
                string stegoKey = txtStegoKey.Text.Trim();

                // استخراج النص الأصلي
                string extracted = StegoObfuscatorForKey.Extract(encryptedStego, stegoKey, StegoObfuscatorForKey.DefaultCoverLength);

                // فك التشفير
                var (n1, n2, recJson) = MultiLayerEncryptorForKey.DecryptTriple(extracted, pass1, pass2, pass3);

                txtNumber1Out.Text = n1;
                txtNumber2Out.Text = n2;
                txtJsonOut.Text = recJson;

                // حفظ نسخة من JSON المسترجع
                File.WriteAllText("recovered.json", recJson);
                _mProccess_Name = "Decrypt";
                PrintDifferenceInSeconds();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Decrypting: " + ex.Message);
            }
        }

        private void BtnCleanAll_Click(object sender, EventArgs e)
        {
            CleanDefaults();

        }

        private void btnReEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                DateStart = DateTime.Now;

                string n1 = txtNumber1Out.Text.Trim();
                string n2 = txtNumber2Out.Text.Trim();
                string inputdate = txtDateInput.Text.Trim();


                int year = int.Parse(inputdate.Substring(0, 4));  // "2025"
                int month = int.Parse(inputdate.Substring(5, 2)); // "09"
                int day = int.Parse(inputdate.Substring(8, 2));   // "11"

                DateTime date = new DateTime(year, month, day);

                string pass1 = txtPassword1.Text.Trim();
                string pass2 = txtPassword2.Text.Trim();
                string pass3 = txtPassword3.Text.Trim();
                string stegoKey = txtStegoKey.Text.Trim();

                // استدعاء التشفير الجديد
                string newEncrypted = MultiLayerEncryptorForLicence.EncryptTriple(n1, n2, date, pass1, pass2, pass3);

                // تخزين النتيجة
                txtReEncrypted.Text = newEncrypted;

                // إخفاء مع Stego
                string stego = StegoObfuscatorForLicence.Hide(newEncrypted, stegoKey, StegoObfuscatorForLicence.DefaultCoverLength);
                txtStegoReEncrypted.Text = stego;

                try
                {
                    File.WriteAllText("License" + n2 + ".txt", stego);
                    //_mClassToJsonStr = JsonConvert.SerializeObject(responsePost);
                    //_mProtectionKey_File_Name = SettingsParams.PihFileTxtPath + "License" + inv.SupplierParty.partyIdentification.ID + ".txt";
                    //using (var tw = new StreamWriter(_mProtectionKey_File_Name, true, Encoding.UTF8))
                    //{
                    //    tw.WriteLine(recJson);
                    //    tw.Close();
                    //}

                }
                catch (Exception ExErrorWrite)
                {
                    //File.AppendAllText(_mReturnResponsedir.Replace(_mVAT_REGISTERATION + "_" + _mISSUE_DATE.Replace("-", "") + "_", "") + "WriteFileErrorLog.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Error writing to file: ***\n" + _mGeneralFileName + "***\n###" + ExErrorWrite.ToString() + "###\n\n");

                }
                _mProccess_Name = "ReEncypt";
                PrintDifferenceInSeconds();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Re-Encrypt: " + ex.Message);
            }
        }

        private void btnDecryptNew_Click(object sender, EventArgs e)
        {
            try
            {
                DateStart = DateTime.Now;

                string stegoKey = txtStegoKey.Text.Trim();
                string pass1 = txtPassword1.Text.Trim();
                string pass2 = txtPassword2.Text.Trim();
                string pass3 = txtPassword3.Text.Trim();

                string stegoEnc = txtStegoReEncrypted.Text.Trim();

                // استخراج النص
                string extracted = StegoObfuscatorForLicence.Extract(stegoEnc, stegoKey, StegoObfuscatorForLicence.DefaultCoverLength);

                // فك التشفير
                var (num1, num2, dateOut) = MultiLayerEncryptorForLicence.DecryptTriple(extracted, pass1, pass2, pass3);

                txtNumber1NewOut.Text = num1;
                txtNumber2NewOut.Text = num2;
                txtDateOut.Text = dateOut.ToString();
                _mProccess_Name = "DecryptNew";
                PrintDifferenceInSeconds();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Decrypt New: " + ex.Message);
            }
        }

    }
}
