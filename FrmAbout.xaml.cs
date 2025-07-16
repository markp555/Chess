using System;
using System.Windows;

namespace SrcChess2 {
    /// <summary>
    /// Interaction logic for frmAbout.xaml
    /// </summary>
    public partial class FrmAbout : Window {

        /// <summary>
        /// Class CTor
        /// </summary>
        public FrmAbout() {
            InitializeComponent();
            VersionInfo.Content = Version + NetVersion;
            YearInfo.Content = Years;
        }

        /// <summary>
        /// Called when the Ok button is closed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event argument</param>
        private void ButOk_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// Program version
        /// </summary>
        public static string Version => "Version 3.34.0";
        
        /// <summary>
        /// .Net Version
        /// </summary>
        public static string NetVersion => $" (.Net {Environment.Version})";

        /// <summary>
        /// Development times
        /// </summary>
        public static string Years => "(2007-2024)";
    }
}
