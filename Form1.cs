using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//Dépendance pour la connexion BD
using MySql.Data.MySqlClient;
using CameraDetect;


namespace CameraDetect
{
    public partial class Form1 : Form
    {
        private Capture capture;  //Variable permettant de prendre des images de la caméra
        private bool captureInProgress; // Vérifie si la capture est en cours d'éxécution
        private CascadeClassifier faceDetect;  //Déclaration de l'objet CascadeClassifier
        private Bitmap VISAGE = new Bitmap(350, 350);
        private ConnexionBD connectBD = new ConnexionBD();
        private Image<Bgr, byte> SansDetect;
        private Rectangle face;
        private MySqlConnection connection;
        //private string date_heure = DateTime.Now.ToString("dd/mm/yyyy hh:mm");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            //Chemin du fichier de détection EmguCV .xml au chargement du programme
            faceDetect = new CascadeClassifier("haarcascade_frontalface_default.xml");

            if (connectBD.OpenConnection() == true)
            {
                LblBDD.BackColor = Color.Green; //Si la connexion est OK : VERT
            }
            else
            {
                LblBDD.BackColor = Color.Red; //Si la connexion est KO : ROUGE
            }
            connectBD.CloseConnection();
        }
        private void ProcessFrame(object sender, EventArgs arg)
        {

            Image<Bgr, Byte> ImageFrame = capture.QueryFrame(); //Création d'un objet EmguCv type image nommé ImageFrame puis enregistrement
            SansDetect = ImageFrame.Copy();
            if (ImageFrame != null)
            {
                Image<Gray, byte> GrayFrame = ImageFrame.Convert<Gray, byte>(); //Conversion de l'image en nuance de gris (plus facile pour la détection)
                var faces = faceDetect.DetectMultiScale(
                        GrayFrame,           //image détéctée 
                        1.1,                //scaleFactor
                        50,                 //minNeighbors - Nb minimum de visage à détécter
                        new Size(10, 10),   //minSize
                        Size.Empty);


                if (faces.Length != 0)
                {
                    face = faces[0];
                    face.Inflate(0, 70);
                    ImageFrame.Draw(face, new Bgr(Color.White), 8);
                }
                imgBox.Image = ImageFrame;
            }



        }

        private void BtnStart_Click_1(object sender, EventArgs e)
        {
            #region if capture is not created, create it now
            if (capture == null)
            {
                try
                {
                    capture = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            #endregion

            if (capture != null)
            {
                if (BtnStart.Text == "Pause")
                {
                    BtnStart.Text = "Reprendre";
                    Application.Idle -= ProcessFrame;
                }
                else
                {
                    BtnStart.Text = "Pause";
                    Application.Idle += ProcessFrame;
                }
                captureInProgress = !captureInProgress;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                if (captureInProgress)
                {

                    if (!face.IsEmpty)
                    {
                        try
                        {
                            VISAGE = SansDetect.Bitmap.Clone(face, System.Drawing.Imaging.PixelFormat.DontCare);
                        }
                        catch
                        {
                            MessageBox.Show("Il n'y a pas de visage !");
                        }

                        imgBoxSave.Image = VISAGE;
                    }

                }
            }

        }

        private void BtnQuitter_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnAjout_Click(object sender, EventArgs e)
        {
            
            MySqlConnection connection = new MySqlConnection();
            ImageConverter imageC = new ImageConverter();

            connectBD.OpenConnection();
            //create command and assign the query and connection from the constructor
            MySqlCommand commande = new MySqlCommand("INSERT INTO Camera (Nom, Prenom, Cam) VALUES(@nom, @prenom, @cam)", connectBD.connection);
            commande.Parameters.AddWithValue("@nom", TxtBoxNom.Text);
            commande.Parameters.AddWithValue("@prenom", TxtBoxPrenom.Text);
            commande.Parameters.AddWithValue("@cam", (byte[])imageC.ConvertTo(VISAGE, typeof(byte[])));
            
           
            
            commande.ExecuteNonQuery();
            commande.Connection.Close();
        }


    }






    public partial class ConnexionBD
    {
        public MySqlConnection connection;
        private string server;
        private string database;
        private string uid;    
         private string password;
    


        public ConnexionBD()
        {
            Initialize();
        }

        //Initialize values
        public void Initialize()
        {
            server = "localhost";
            database = "test";
            uid = "root";
            password = "";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection= new MySqlConnection(connectionString);
        }
        //Ouvrir une connection à la BDD
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Impossible de se connecter au serveur.");
                        break;

                    case 1045:
                        MessageBox.Show("Mot de passe ou identifiant invalide, veuillez réessayer!");
                        break;
                }
                return false;
            }
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

       
    }
}


