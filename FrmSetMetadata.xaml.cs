using SrcChess2.Core;
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
using System.Windows.Shapes;

namespace SrcChess2
{
    /// <summary>
    /// Логика взаимодействия для SetMetadataDialog.xaml
    /// </summary>
    public partial class SetMetadataDialog : Window
    {
        public ChessBoard Board { get; private set; }

        public SetMetadataDialog(ChessBoard board)
        {
            InitializeComponent();
            Board = board;
            checkBoxBlackLeft.IsChecked = Board.CanBlackMakeLeftCastle;
            checkBoxBlackRight.IsChecked = Board.CanBlackMakeRightCastle;
            checkBoxWhiteRight.IsChecked = Board.CanWhiteMakeRightCastle;
            checkBoxWhiteLeft.IsChecked = Board.CanWhiteMakeLeftCastle;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Board.CanBlackMakeLeftCastle = checkBoxBlackLeft.IsChecked ?? true;
            Board.CanBlackMakeRightCastle = checkBoxBlackRight.IsChecked ?? true;
            Board.CanWhiteMakeRightCastle = checkBoxWhiteRight.IsChecked ?? true;
            Board.CanWhiteMakeLeftCastle = checkBoxWhiteLeft.IsChecked ?? true;
            Close();
        }
    }
}
