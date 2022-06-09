﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace démimin
{
    public class Asynch_client
    {
        /* La classe Asynch_client, permet  la création d'objets associés à des clients
        * La classe contient toute les méthodes permettant la communication*/

        public int index_client = 0;

        #region EventHandlers
        /* Création des gestionnaires d'évements */
        public delegate void ClientConnectedHandler(Asynch_client client);
        public delegate void DataSendHandler(Asynch_client client);
        public delegate void DataReceiveHandler(Asynch_client client, object data);
        public delegate void ClientDisconnectedHandler(Asynch_client client, string message);
        public delegate void ConnectionRefusedHandler(Asynch_client client, string message);
        #endregion

        #region Events
        /* Création des events propres à  chaque client */
        public event ConnectionRefusedHandler ConnectionRefused;
        public event DataSendHandler DataSent;
        public event DataReceiveHandler DataReceived;
        public event ClientConnectedHandler ClientConnected;
        public event ClientDisconnectedHandler ClientDisconnected;
        #endregion

        private Socket clientSock;

        #region Constructeurs
        /* On définit deux construteurs
         * Ils intancient un objet client de deux manières différentes
         * Soit à partir d'un nouveau socket
         * Soit à partir d'un socket existant
         */
        public Asynch_client()
        {
            clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public Asynch_client(Socket clientSocket)
        {
            this.clientSock = clientSocket;
            Seceive_data();
        }
        #endregion

        public void Connect(string address, int port)
        {
/* Afin de communiquer, le client doit se connecter à  un serveur
 * Il faut alrs préciser l'IP et le port de connection du serveur
 * L'étape finalement de connection du client étant la requête de connection envoyée au serveur.
 */

            if (!clientSock.Connected)
            {
                IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(address), port);
                clientSock.BeginConnect(EndPoint, Connection_Callback, null);
            }
        }
        public void Disconnect()
        {
/* Déconnection du client au serveur  */
            if (clientSock.Connected)
            {
                clientSock.Close();
            }
        }

        public void Send_data(object data)
        {
/* Méthode propre à l'envoi de données
 * Les données sont envoyées sous forme de tableau de bits
 * Il faut donc "sérialiser" les données, entendons par là, convertir la donnée à envoyer en tableau de bits
 */
            if (clientSock.Connected)
            {
                byte[] dataBuffer = serialize(data);

                try
                {
                    clientSock.BeginSend(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, dataSendCallback, null);
                }
                catch (Exception ex)
                {
                    onClientDisconnected(ex.Message);
                }
            }
        }

        private void Seceive_data()
        {
/* Méthode propre à la réception de données
 * On stock les données reçues dans un buffer temporaire 
 */
            if (clientSock.Connected)
            {
                //R_buff receiveBuffer = new R_buff();

                const int BufferSize = 4096;
                byte[] tempBuffer = new byte[BufferSize];
                MemoryStream memStream = new MemoryStream();

                clientSock.BeginReceive(tempBuffer, 0, BufferSize, SocketFlags.None, receiveCallback, tempBuffer);
            }
        }
        private byte[] serialize(object data)
        {
/* La méthode permet la conversion d'un paramètre reçu en argument en un tableau de bits
 * La coversion est réalisée par l'objet "binaryformatter", qui implémente les méthode de conversion
 * L'objet "MemoryStream" permet lui de traiter des données directement dans la mémoire. 
 * Une fois le contenu de data formatté dans un flux temporaire on fige ce flux dans un buffer temporaire
 * Le flux à pour intérêt l'accès direct à la mémoire, puisque la fonction de formtage sort les bits un à un
 * Et plutôt que d'ajouter chaque bit au buffer, on crée un flux qu'on fige une fois le formatge terminé
*/
            BinaryFormatter bin = new BinaryFormatter();
            MemoryStream mem = new MemoryStream();
            bin.Serialize(mem, data);
            byte[] buffer = mem.GetBuffer();
            mem.Close();
            return buffer;
        }

        private void Connection_Callback(IAsyncResult ar)
        {
/* On tente de se connecter au serveur. Si la connexion est établie, on démarre la réception de 
* données et on déclenche l'event ClientConnected. 
*/
            try
            {
                clientSock.EndConnect(ar);
            }
            catch (SocketException ex)
            {
                onConnectionRefused(ex.Message);
            }

            if (clientSock.Connected)
            {
                Seceive_data();
                onClientConnected(this);
            }
        }

        private void dataSendCallback(IAsyncResult ar)
        {
/* Méthode appelée à la fin de l'envoi 
 * Elle permet l'arrêt de l'envoi et déclenche la routine "onDataSent"
 */
            if (clientSock.Connected)
            {
                clientSock.EndSend(ar);
                onDataSent(this);
            }
            else
            {
                onClientDisconnected("unable to send data : client disconnected");
            }
        }

        private void receiveCallback(IAsyncResult ar)
        {
/* La fonction de callback est appelée lors de la réception de données
 * On récupère la taille de la donnée transmise - nb de bytes
 * On continue d'exécuter la fonction de réception de données tant que le nmbre de bytes envoyés n'est pas atteint
 * 
*/
            int dataReceivedSize = 0;
            try
            {
                dataReceivedSize = clientSock.EndReceive(ar);
            }
            catch (Exception ex)
            {
                if (!clientSock.Connected)
                {
                    onClientDisconnected(ex.Message);
                }
            }
            //R_buff receiveBuffer = (R_buff)ar.AsyncState;

            const int BufferSize = 4096;
            byte[] tempBuffer = (byte[])ar.AsyncState;
            MemoryStream memStream = new MemoryStream();

            if (dataReceivedSize > 0)
            {
                /* Le compteur est uncrémenté après -- ce qui implique que la fonction s'exécute encore une fois après avoir reçu la dernière donnée
                 * Lorsqu'on arrive au dernier byte, le socket client devient indisiponible 
                 * On décompose alors la donnée reçue et on instancie une variable data dont le type "objet" s'ignifie qu'on ne précise pas le type de donnée
                 * Ce dernier est propre à la donnée
                 * Enfin il faut relancer l'écoute
                 */
                //receiveBuffer.Append(dataReceivedSize);
                memStream.Write(tempBuffer, 0, dataReceivedSize);
                if (clientSock.Available > 0)
                    clientSock.BeginReceive(tempBuffer, 0, BufferSize, SocketFlags.None, receiveCallback, tempBuffer);
                else
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    object data;
                    memStream.Seek(0, SeekOrigin.Begin);

                    data = formatter.Deserialize(memStream);

                    memStream.Close();

                    //return data;
                    //object data = receiveBuffer.Deserialize();
                    onDataReceived(data);
                    Seceive_data();
                }
            }
        }

        #region routines de connexion
        private void onClientDisconnected(string message)
        {
            if (ClientDisconnected != null)
            {
                if (ClientDisconnected.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)ClientDisconnected.Target).Invoke(ClientDisconnected, this, message);
                }
                else
                {
                    ClientDisconnected(this, message);
                }
            }
        }

        private void onConnectionRefused(string message)
        {
            if (ConnectionRefused.Target is System.Windows.Forms.Control)
            {
                ((System.Windows.Forms.Control)ConnectionRefused.Target).Invoke(ConnectionRefused, this, message);
            }
            else
            {
                ConnectionRefused(this, message);
            }
        }

        private void onClientConnected(Asynch_client asyncClient)
        {
            if (ClientConnected != null)
            {
                if (ClientConnected.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)ClientConnected.Target).Invoke(ClientConnected, this);
                }
                else
                {
                    ClientConnected(this);
                }
            }
        }
        #endregion

        #region Routines de réception/envoi de dnnées
/* Si on souhaite exécuter des tâches après chaque envoi il faut déclencher l'évenement associé
 */
        private void onDataReceived(object data)
        {
            if (DataReceived != null)
            {
                if (DataReceived.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)DataReceived.Target).Invoke(DataReceived, this, data);
                }
                else
                {
                    DataReceived(this, data);
                }
            }
        }

        private void onDataSent(Asynch_client asyncClient)
        {
            if (DataSent != null)
            {
                if (DataSent.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)DataSent.Target).Invoke(DataSent, this);
                }
                else
                {
                    DataSent(this);
                }
            }
        }
        #endregion
    }
}
