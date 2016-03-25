using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System.Windows;

namespace Gomoku
{
    public class NetworkProcess
    {
        Socket socket;

        public delegate void MessageComing(object sender, string message, string name);
        public event MessageComing messageComing;
        public delegate void PerformMove(object sender, int player, int row, int col);
        public event PerformMove performMove;
        public delegate void FirstAIMove(object sender);
        public event FirstAIMove firstAIMove;

        public void Init()
        {

            string url = System.Configuration.ConfigurationManager.ConnectionStrings["serverAddress"].ToString();
            socket = IO.Socket(url);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, "Connected", "Server"); })); 
            });
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString(), "Server"); })); 

            });
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString(), "Server"); })); 
            });
            socket.On("ChatMessage", (data) =>
            {
                string message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                string playerName;
                JToken player = ((Newtonsoft.Json.Linq.JObject)data)["from"];
                if (player == null)
                {
                    playerName = "Server";
                }
                else
                {
                    playerName = player.ToString();
                }
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, message, playerName); })); 
                if (message == "Welcome!")
                {
                    setNameToServer("Player1");
                    socket.Emit("ConnectToOtherPlayer");
                }

                if (message.Contains("first player"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { firstAIMove(this); })); 

                }
            });

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString(), "Server"); })); 
            });

            socket.On("NextStepIs", (data) =>
            {
                int player, row, col;
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["player"].ToString(), out player);
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString(), out row);
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString(), out col);
                Application.Current.Dispatcher.Invoke(new Action(() => { performMove(this, player, row, col); })); 
            });

            socket.On("EndGame", (data) =>
            {
                string message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                Application.Current.Dispatcher.Invoke(new Action(() => { messageComing(this, message, "Server"); }));
                MessageBox.Show(message, "Game over");
            });
        }

        public void setNameToServer(string myName)
        {
            socket.Emit("MyNameIs", myName);   
        }

        public void ChatToServer(string mes)
        {
            socket.Emit("ChatMessage", mes);
        }

        public void PlayAt(int x, int y)
        {
            socket.Emit("MyStepIs", JObject.FromObject(new { row = x, col = y }));
        }

        public void TurnOffOnline()
        {
            socket.Off();
            socket.Disconnect();
        }
    }
}
