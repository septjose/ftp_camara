using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WebCamSave
{
    public partial class Form1 : Form
    {
        private string ruta = @"C:\Users\José Alberto\Pictures\";
        private bool HayDispositivos;
        private FilterInfoCollection MisDispositivios;
        private VideoCaptureDevice MiWebCam;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargaDispositivos();
        }

        public void CargaDispositivos()
        {
            MisDispositivios = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (MisDispositivios.Count > 0)
            {
                HayDispositivos = true;
                for (int i = 0; i < MisDispositivios.Count; i++)
                    comboBox1.Items.Add(MisDispositivios[i].Name.ToString());
                comboBox1.Text = MisDispositivios[0].Name.ToString();
            }
            else
                HayDispositivos = false;

        }

        private void CerrarWebCam()
        {
            if (MiWebCam != null && MiWebCam.IsRunning)
            {
                MiWebCam.SignalToStop();
                MiWebCam = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CerrarWebCam();
            int i = comboBox1.SelectedIndex;
            string NombreVideo = MisDispositivios[i].MonikerString;
            MiWebCam = new VideoCaptureDevice(NombreVideo);
            MiWebCam.NewFrame += new NewFrameEventHandler(Capturando);
            MiWebCam.Start();
        }

        private void Capturando(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap Imagen = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = Imagen;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarWebCam();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MiWebCam != null && MiWebCam.IsRunning)
            {
                pictureBox2.Image = pictureBox1.Image;
                pictureBox2.Image.Save(ruta + "yugi.jpg", ImageFormat.Jpeg);
            }
        }

        public bool SubirFicheroStockFTP()
        {
            bool verdad;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(
                        ConfigurationManager.AppSettings["FTP_SERVER"].ToString() +
                        ConfigurationManager.AppSettings["FTP_PATH"].ToString() +
                        ConfigurationManager.AppSettings["NOMBRE_FICHERO"].ToString());

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        ConfigurationManager.AppSettings["RUTA_FICHERO"],
                        ConfigurationManager.AppSettings["NOMBRE_FICHERO"]);

                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["FTP_USER"].ToString(),
                        ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString());

                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (var fileStream = File.OpenRead(filePath))
                {
                    using (var requestStream = request.GetRequestStream())
                    {
                        fileStream.CopyTo(requestStream);
                        requestStream.Close();
                    }
                }

                var response = (FtpWebResponse)request.GetResponse();

                response.Close();
                verdad = true;
            }
            catch (Exception ex)
            {
                //logger.Error("Error " + ex.Message + " " + ex.StackTrace);
                verdad = false;
                MessageBox.Show("Error " + ex.Message + " " + ex.StackTrace);
                
               
            }
            return verdad;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool subir = SubirFicheroStockFTP();
            if(subir)
            {
                MessageBox.Show("SI se pudo");
            }
            else
            {
                MessageBox.Show("No se Pudo");
            }
        }

        public bool downloadFile(string servidor, string usuario, string password, string archivoOrigen, string carpetaDestino, int bufferdes)
        {
            bool descargar;
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(servidor + archivoOrigen);
                reqFTP.Credentials = new NetworkCredential(usuario, password);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream writeStream = new FileStream(@carpetaDestino, FileMode.Create);
                int Length = bufferdes;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                writeStream.Close();
                response.Close();
                descargar = true;
            }
            catch (WebException wEx)
            {
                descargar = false;
                throw wEx;
                
            }
            catch (Exception ex)
            {
                descargar = false;
                throw ex;
                

            }
            return descargar;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool descargo = downloadFile("ftp://jjdeveloperswdm.com/", "bonita_smile@jjdeveloperswdm.com", "bonita_smile", "yugi.jpg", @"C:\Users\José Alberto\Pictures\yugiohh\yugi.jpg", 10);
            if(descargo)
            {
                MessageBox.Show(":)");
            }
            else
            {
                MessageBox.Show(":(");
            }
        }
    }
}