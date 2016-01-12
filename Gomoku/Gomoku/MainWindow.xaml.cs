using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string myName;
        public MainWindow()
        {
            InitializeComponent();
            //Button b = (Button)boardGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 0);
            
        }

        private void messageSendButton_Click(object sender, RoutedEventArgs e)
        {
            ChatBlock newChatBlock = new ChatBlock();
            newChatBlock.nameLabel.Content = myName;
            newChatBlock.textLabel.Content = messageTextbox.Text;
            newChatBlock.timeLabel.Content = DateTime.Now.ToShortTimeString();
            chatPanel.Children.Add(newChatBlock);
            messageTextbox.Text = "";
        }

        private void nameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            myName = nameTextbox.Text;
        }

        private void messageTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                messageSendButton_Click(this, new RoutedEventArgs());
            }
        }

        private void nameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                nameChangeButton_Click(this, new RoutedEventArgs());
            }
        }

        private void someButton_Click(object sender, RoutedEvent e)
        {

        }

        private void someButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int row = Grid.GetRow(clickedButton);
            int col = Grid.GetColumn(clickedButton);
            string message = string.Format("Clicked at:\n  + Row: {0}\n  + Column: {1}", row.ToString(), col.ToString());
            MessageBox.Show(message);
        }
    }
}
