using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Oprogramowanie_Serwera_TCP
{
    public abstract class TcpSoftware
    {
        /// <summary>
        /// Fields of TCP server software
        /// </summary>
        /// 
        #region Fields

        TcpListener tcpListener { get; set; };
        TcpClient tcpClient { get; set; };
        IPAddress iPAddress { get; set; };
        string asciiString = ""; // used to convert from byte[] to string
        string input = ""; // used to receive proper input to the program
        char[] charToTrim = { '\0', '\r', '\n' }; // chars we don't want to be in buffer
        string prompt = "$";
        bool running;
        int port;
        int buffer_size = 1024;
        NetworkStream stream { get; set; };
        
        #endregion

        #region Properties

        /// <summary>
        /// This property gives access to the IP address of a server instance. Property can't be changed when the Server is running.
        /// </summary>

        public IPAddress IPAddress
        {
            get => iPAddress;
            set
            {
                if (!running)
                    iPAddress = value;
                else
                    throw new Exception("nie można zmienić adresu IP kiedy serwer jest uruchomiony");
            }
        }

        /// <summary>
        /// This property gives access to the port of a server instance. Property can't be changed when the Server is running. Setting invalid port numbers will cause an exception. 
        /// </summary>

        public int Port
        {
            get => port; set

            {

                int tmp = port;

                if (!running) port = value; else throw new Exception("nie można zmienić portu kiedy serwer jest uruchomiony");

                if (!checkPort())

                {

                    port = tmp;

                    throw new Exception("błędna wartość portu");

                }

            }

        }

        /// <summary>

        /// This property gives access to the buffer size of a server instance. Property can't be changed when the Server is running. Setting invalid size numbers will cause an exception. 

        /// </summary>

        public int Buffer_size
        {
            get => buffer_size; set

            {

                if (value < 0 || value > 1024 * 1024 * 64) throw new Exception("błędny rozmiar pakietu");

                if (!running) buffer_size = value; else throw new Exception("nie można zmienić rozmiaru pakietu kiedy serwer jest uruchomiony");

            }

        }



        protected TcpListener TcpListener
        {
            get => tcpListener;
            set => tcpListener = value;
        }

        protected TcpClient TcpClient
        {
            get => tcpClient;
            set => tcpClient = value;
        }

        protected NetworkStream Stream
        {
            get => stream;
            set => stream = value;
        }

        #endregion

        #region Constructors

        /// <summary>

        /// A default constructor. It doesn't start the server. Invalid port numbers will thrown an exception.

        /// </summary>

        /// <param name="IP">IP address of the server instance.</param>

        /// <param name="port">Port number of the server instance.</param>

        public TcpSoftware(IPAddress IP, int port)
        {
            running = false;

            IPAddress = IP;

            Port = port;

            if (!checkPort())

            {

                Port = 8000;

                throw new Exception("błędna wartość portu, ustawiam port na 8000");

            }

        }

        #endregion

        #region Functions

        /// <summary>

        /// This function will return false if Port is set to a value lower than 1024 or higher than 49151.

        /// </summary>

        /// <returns>An information wether the set Port value is valid.</returns>

        protected bool checkPort()
        {
            if (port < 1024 || port > 49151) return false;

            return true;

        }

        /// <summary>

        /// This function starts the listener.

        /// </summary>

        protected void StartListening()
        {
            TcpListener = new TcpListener(IPAddress, Port);

            TcpListener.Start();

        }

        /// <summary>

        /// This function waits for the Client connection.

        /// </summary>

        protected abstract void AcceptClient();

        /// <summary>

        /// This function implements Echo and transmits the data between server and client.

        /// </summary>

        protected abstract void BeginDataTransmission(NetworkStream stream);

        /// <summary>

        /// This function fires off the default server behaviour. It interrupts the program.

        /// </summary>

        public abstract void Start();

        #endregion

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

        void Start()
        {
            server.Start();
            while (true)
            {
                client = server.AcceptTcpClient();
                byte[] buffer = new byte[1024];

                while (client.Connected)
                {
                    /// <summary>
                    /// Wyswietlenie komunikatu startowego i przejscie do oprogramowania
                    /// </summary>
                    try
                    {
                        buffer = Encoding.ASCII.GetBytes("Password generator server by Dawid Bronszkiewicz.\r\nTo start please enter the code \"genpass\".\r\nEnter \"exit\" to close the connection.\r\n\r\n");
                        client.GetStream().Write(buffer, 0, buffer.Length);
                        tcpSoftware.UI(client, buffer);
                    }
                    /// <summary>
                    /// Wyłapanie wyjątku powoduje wyświetlenie komunikatu o błędzie i zamknięcie połączenia
                    /// </summary>
                    catch
                    {
                        buffer = Encoding.ASCII.GetBytes("ERROR! Closing connection.\r\n\r\n");
                        client.GetStream().Write(buffer, 0, buffer.Length);
                        client.Close();
                    }
                }
            }
        }
    }
}
