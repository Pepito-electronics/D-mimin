using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace démimin
{
/* L'application serveur a pour objectif de fournir au client un identifiant et de récupèrer les meilleures scores */
    public partial class Démineur_serveur : Form
    {
        As_Server server;
        List<As_Client> remoteClients = new List<As_Client>();
        List<List<string>> player_score = new List<List<string>>();
        int playerCount = 0;

        #region Constructors
        /* Initialise le forms et démarre le serveur */
        public Démineur_serveur()
        {
            InitializeComponent();
        }

        public Démineur_serveur(string ip, int port) : this()
        {
        /* Création et démarrage du serveur + abonnement aux évents */
            server = new As_Server(ip, port);
            server.ServerSarted += Server_ServerStarted;
            server.ClientAccepted += Server_ClientAccepted;
            server.Start();
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            /* Création des forms client
             * A ce stade on instancie simplement un deuxième forms et on rend visible
             */ 
            Démineur_client client = new Démineur_client("127.0.0.1", 1234);
            client.Show();            
        }
        private void Server_ClientAccepted(As_Client client)
        {
            /* Routine déclenchée à chaque nouvelle connection d'un client au serveur
             * On récupère les données du client qui sont placée dans un objet client analogue "RemoteClient"
             * Qui est ensuite placé dans une liste contenant tous les clients
             * Finalement on crée une nouvelle ligne dans le tableau des scores pour chaque nouveau client
             */
            As_Client remoteClient = client;
            remoteClient.DataReceived += RemoteClient_DataReceived;
            remoteClient.ClientDisconnected += RemoteClient_ClientDisconnected;
            remoteClients.Add(client);
            
            remoteClient.index_client = playerCount;        // On récupère l'index du client et on lui attribue
            /* Attribution du nom du joueur au client */
            string nomDuJoeur = textBox1.Text;              
            var player = new List<string>();
            player.Add(nomDuJoeur);
            player_score.Add(player);
            client.Send_data(nomDuJoeur);
            /*-----------------------------------------*/
            playerCount++;
        }

        private void RemoteClient_DataReceived(As_Client client, object data)
        {
/* Routine déclenchée à la réception de données
 * Grâce aux méthodes du client analogue on peut utiliser les méthodes d'envoi et de réception propre aux clients
 * 
 * On récupère les scores qui sont comparés aux anciens, et on met à jour le tableau des scores si le résultat est meilleur
 */
            string score = data.ToString();
            int oldscore;
            /* Lors de la première partie il n'y a pas de score l'idex [x][1] n'existe pas
             * On essaye donc de récupèrer l'ancien score
             * Si échec alors il s'agit de la première partie... Debug dans la console pour controler l'erreur
             */
            try
            {
                oldscore = int.Parse(player_score[client.index_client][1]);
                int newscore = int.Parse(score);
                if (oldscore <= newscore)
                {
                    player_score[client.index_client][1] = score;
                }
            }
            catch(Exception ex) 
            {
                oldscore = 0;
                player_score[client.index_client].Add(score);
                Console.WriteLine(ex.ToString());
            }
        }

        private void RemoteClient_ClientDisconnected(As_Client client, string message)
        {
        /* Routine de déconnexion - il s'agît de supprimer le client des clients connectés*/
            remoteClients.Remove(client);
        }

        private void Server_ServerStarted()
        {
        /* Routine de démarrage serveur */
            Console.WriteLine("Serveur has succefully started");    // Confirmation de démarrage en ligne de commande
            //MessageBox.Show("Server has sarted");
        }

        private void scores_Click(object sender, EventArgs e)
        {
         /* A l'appui du bouton des scores, on formate une chaine de caractères et on affiche une fenêtre */   
            string scores = "";
            foreach(var client in remoteClients)
            {
                string score = player_score[client.index_client][0] + " : " + player_score[client.index_client][1]+"\n\r";
                scores += score;
            }
            MessageBox.Show(scores);

        }
    }
}
