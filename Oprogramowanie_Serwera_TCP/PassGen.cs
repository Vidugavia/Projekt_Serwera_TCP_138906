﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Oprogramowanie_Serwera_TCP
{
    public class PassGen : TcpSoftware
    {
        #region Fields

        string asciiString = ""; // used to convert from byte[] to string
        string input = ""; // used to receive proper input to the program
        char[] charToTrim = { '\0', '\r', '\n' }; // chars we don't want to be in buffer
        string prompt = "$";
        byte[] buffer;

        public delegate void TransmissionDataDelegate(NetworkStream stream);

        #endregion

        public PassGen(IPAddress IP, int port) : base(IP, port)
        {

        }

        public override void Start()
        {
            StartListening();

            //transmission starts within the accept function

            AcceptClient();   
        }

        protected override void AcceptClient()
        {
            while (true)

            {

                TcpClient tcpClient = TcpListener.AcceptTcpClient();

                Stream = tcpClient.GetStream();

                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);

                //callback style

                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);

                // async result style

                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);

                ////operacje......
                
                buffer = new byte[Buffer_size];

                while (tcpClient.Connected)
                {
                    /// <summary>
                    /// Wyswietlenie komunikatu startowego i przejscie do oprogramowania
                    /// </summary>
                    try
                    {
                        buffer = Encoding.ASCII.GetBytes("Password generator server by Dawid Bronszkiewicz.\r\nTo start please enter the code \"genpass\".\r\nEnter \"exit\" to close the connection.\r\n\r\n");
                        tcpClient.GetStream().Write(buffer, 0, buffer.Length);
                        UI(tcpClient, buffer);
                    }
                    /// <summary>
                    /// Wyłapanie wyjątku powoduje wyświetlenie komunikatu o błędzie i zamknięcie połączenia
                    /// </summary>
                    catch
                    {
                        buffer = Encoding.ASCII.GetBytes("ERROR! Closing connection.\r\n\r\n");
                        tcpClient.GetStream().Write(buffer, 0, buffer.Length);
                        tcpClient.Close();
                    }
                }
                
                //while (!result.IsCompleted) ;

                ////sprzątanie

            }
        }

        private void TransmissionCallback(IAsyncResult ar)

        {

            // sprzątanie

        }

        protected override void BeginDataTransmission(NetworkStream stream)

        {

            byte[] buffer = new byte[Buffer_size];

            while (true)

            {

                try

                {

                    int message_size = stream.Read(buffer, 0, Buffer_size);

                    stream.Write(buffer, 0, message_size);

                }

                catch (IOException e)

                {

                    break;

                }

            }

        }

        public override void Start()

        {

            

        }

        /// <summary>
        /// This function sends a message to client
        /// </summary>
        void Send(TcpClient client, byte[] buffer, string message)
        {
            buffer = Encoding.ASCII.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// This function receives a message from client
        /// </summary>
        void Receive(TcpClient client, byte[] buffer)
        {
            Send(client, buffer, prompt);
            buffer = new byte[1024];
            do
            {
                client.GetStream().Read(buffer, 0, 1024);
                asciiString = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                input = asciiString.TrimEnd(charToTrim);
            } while (input == "");

        }

        /// <summary>
        /// This is main program to generate passwords
        /// </summary>
        void GenPass(TcpClient client, byte[] buffer)
        {
            Random rnd = new Random();
            Send(client, buffer, "How many passwords? ");
            Receive(client, buffer);
            string[] passwd;
            int size = Int32.Parse(input);
            passwd = new string[size];
            for (int i = 0; i < size; i++)
            {
                Send(client, buffer, "How many letters in " + (i + 1) + " password? ");
                Receive(client, buffer);
                for (int j = 0; j < Int32.Parse(input); j++)
                {
                    passwd[i] += (char)rnd.Next(33, 126);
                }
                Send(client, buffer, passwd[i]);
                Send(client, buffer, "\r\n");
            }
            return;
        }

        /// <summary>
        /// This is main user interface
        /// </summary>
        public void UI(TcpClient client, byte[] buffer)
        {
            Receive(client, buffer);

            switch (input)
            {
                case "genpass":
                    GenPass(client, buffer);
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