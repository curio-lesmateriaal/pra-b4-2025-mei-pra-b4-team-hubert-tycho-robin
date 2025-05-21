using PRA_B4_FOTOKIOSK.controller;
using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PRA_B4_FOTOKIOSK
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {

        public ShopController ShopController { get; set; }
        public PictureController PictureController { get; set; }
        public SearchController SearchController { get; set; }

        public Home()
        {
            // Bouw de UI
            InitializeComponent();

            // Stel de manager in
            PictureManager.Instance = this;
            ShopManager.Instance = this;
            ShopController.Window = this;
            PictureController.Window = this;
            SearchController.Window = this;

            // Maak de controllers
            ShopController = new ShopController();
            PictureController = new PictureController();
            SearchController = new SearchController();

            // Start de paginas
            PictureController.Start();
            ShopController.Start();
            SearchController.Start();
        }

        private void btnShopAdd_Click(object sender, RoutedEventArgs e)
        {
            bool invoerCorrect = true;

            // Valideer aantal
            if (!double.TryParse(tbAmount.Text, out double amount) || amount <= 0)
            {
                
                return;
            }

            // Haal geselecteerd product op
            string geselecteerdProduct = cbProducts.SelectedItem as string;
            if (string.IsNullOrEmpty(geselecteerdProduct))
            {
                
                return;
            }

            // Bepaal de prijs op basis van geselecteerd product
            double prijsPerStuk;
            switch (geselecteerdProduct)
            {
                case "Foto 10x15":
                    prijsPerStuk = 2.55;
                    break;
                case "Foto 15x20":
                    prijsPerStuk = 4.00;
                    break;
                default:
                    
                    return;
            }

            // Bereken eindbedrag
            double eindbedrag = amount * prijsPerStuk;

            // Toon eindbedrag in Label en TextBox
            lbReceipt.Content = $"Eindbedrag: {eindbedrag:C}";
            
        }



        private void btnShopReset_Click(object sender, RoutedEventArgs e)
        {
            ShopController.ResetButtonClick();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            PictureController.RefreshButtonClick();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ShopController.SaveButtonClick();
        }

        private void btnZoeken_Click(object sender, RoutedEventArgs e)
        {
            SearchController.SearchButtonClick();
        }
    }
}
