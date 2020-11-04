using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Oprogramowanie_Serwera_TCP
{
    public class PassGenServer : TcpSoftware
    {
        #region Fields

        string asciiString = ""; // used to convert from byte[] to string
        string input = ""; // used to receive proper input to the program
        char[] charToTrim = { '\0', '\r', '\n' }; // chars we don't want to be in buffer
        string prompt = "$";
        byte[] buffer;

        public delegate void TransmissionDataDelegate(TcpClient tcpClient);

        #endregion

        public PassGenServer(IPAddress IP, int port) : base(IP, port)
        {

        }

        public override void Start()
        {
            StartListening();

            //transmission starts within the accept function

            AcceptClient();   
        }

        /// <summary>
        /// Funkcja obsługująca połączenie z klientem
        /// </summary>
        /// <param name="Stream"></param>
        private void BeginDataTransmission(TcpClient tcpClient)
        {
            NetworkStream Stream = tcpClient.GetStream();
            Stream.ReadTimeout = 50000;
            while (tcpClient.Connected)
            {
                /// <summary>
                /// Wyswietlenie komunikatu startowego i przejscie do oprogramowania
                /// </summary>
                try
                {
                    buffer = Encoding.ASCII.GetBytes("Password generator server by Dawid Bronszkiewicz.\r\nTo start please enter the code \"genpass\".\r\nEnter \"exit\" to close the connection.\r\n\r\n");
                    Stream.Write(buffer, 0, buffer.Length);
                    UI(tcpClient, buffer);
                }
                /// <summary>
                /// Wyłapanie wyjątku powoduje wyświetlenie komunikatu o błędzie i zamknięcie połączenia
                /// </summary>
                catch (IOException)
                {
                    //exception.Message;
                    buffer = Encoding.ASCII.GetBytes("ERROR! Closing connection.\r\n\r\n");
                    Stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        protected override void AcceptClient()
        {
            while (true)

            {

                TcpClient tcpClient = TcpListener.AcceptTcpClient();

                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);

                //callback style

                transmissionDelegate.BeginInvoke(tcpClient, TransmissionCallback, tcpClient);

                // async result style

                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);

                ////operacje......

                
                
                //while (!result.IsCompleted) ;

                ////sprzątanie

            }
        }

        private void TransmissionCallback(IAsyncResult ar)

        {
            TcpClient tcpClient = ar.AsyncState as TcpClient;

            // sprzątanie
            tcpClient.Close();
        }

        /// <summary>
        /// This function sends a message to client
        /// </summary>
        protected override void Send(NetworkStream stream, byte[] buffer, string message)
        {
            buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// This function receives a message from client
        /// </summary>
        protected override void Receive(NetworkStream stream, byte[] buffer)
        {
            Send(stream, buffer, prompt);
            buffer = new byte[1024];
            do
            {
                stream.Read(buffer, 0, 1024);
                asciiString = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                input = asciiString.TrimEnd(charToTrim);
            } while (input == "");

        }

        //OPERACJA LOCK do synchronizacji generatora haseł

        /// <summary>
        /// This is main program to generate passwords
        /// </summary>
        void GenPass(NetworkStream stream, byte[] buffer)
        {
            Random rnd = new Random();
            Send(stream, buffer, "How many passwords? ");
            Receive(stream, buffer);
            string[] passwd;
            int size = Int32.Parse(input);
            passwd = new string[size];
            for (int i = 0; i < size; i++)
            {
                Send(stream, buffer, "How many letters in " + (i + 1) + " password? ");
                Receive(stream, buffer);
                for (int j = 0; j < Int32.Parse(input); j++)
                {
                    passwd[i] += (char)rnd.Next(33, 126);
                }
                Send(stream, buffer, passwd[i]);
                Send(stream, buffer, "\r\n");
            }
            return;
        }

        /// <summary>
        /// This is main user interface
        /// </summary>
        public void UI(TcpClient client, byte[] buffer)
        {
            Receive(client.GetStream(), buffer);

            switch (input)
            {
                case "genpass":
                    GenPass(client.GetStream(), buffer);
                    break;
                case "exit":
                    client.Close();
                    return;
                case "":
                    break;
                default:
                    buffer = Encoding.ASCII.GetBytes("No such command.\r\nTo start please enter the code \"genpass\"\r\nEnter \"exit\" to close the connection\r\n\r\n");
                    client.GetStream().Write(buffer, 0, buffer.Length);
                    break;
            }
        }
    }
}
