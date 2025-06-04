using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home? Window { get; set; }
        private List<OrderedProduct> orderedProducts = new List<OrderedProduct>();

        public void Start()
        {
            ShopManager.SetShopReceipt("Eindbedrag\n€");

            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.55, Description = "Kleurenfoto op glanspapier" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 15x20", Price = 4.00, Description = "Grotere foto met extra details" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Sleutelhanger", Price = 7.00, Description = "Sleutelhanger met jouw foto" });
            ShopManager.Products.Add(new KioskProduct() { Name = "mok", Price = 9.33, Description = "Witte mok met bedrukking" });
            ShopManager.Products.Add(new KioskProduct() { Name = "T-shirt", Price = 12.69, Description = "T-shirt met opdruk" });

            foreach (KioskProduct item in ShopManager.Products)
            {
                ShopManager.AddShopPriceList($"{item.Name} - {item.Description}: €{item.Price:0.00}\n");
            }

            ShopManager.UpdateDropDownProducts();
        }

        public void AddButtonClick()
        {
            KioskProduct? selectedProduct = ShopManager.GetSelectedProduct();
            int? fotoId = ShopManager.GetFotoId();
            int? amount = ShopManager.GetAmount();

            if (selectedProduct == null)
            {
                MessageBox.Show("Selecteer een product");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedProduct.Name))
            {
                MessageBox.Show("Productnaam is leeg");
                return;
            }

            if (fotoId == null)
            {
                MessageBox.Show("Vul een fotoId in");
                return;
            }

            if (amount == null || amount <= 0)
            {
                MessageBox.Show("Vul een geldig aantal in");
                return;
            }

            double final = selectedProduct.Price * amount.Value;

            orderedProducts.Add(new OrderedProduct(fotoId.Value, selectedProduct.Name, amount.Value, final));

            StringBuilder bon = new StringBuilder();
            double totaal = 0;
            bon.AppendLine("Bon:");
            bon.AppendLine("-------------------------");

            foreach (var product in orderedProducts)
            {
                if (product.ProductName == null)
                {
                    product.ProductName = "(Onbekend product)";
                }

                bon.AppendLine(
                    $"FotoId: {product.PhotoId}\nProduct: {product.ProductName}\nAantal: {product.Amount}\nTotaal Prijs: €{product.TotalPrice:0.00}\n"
                );
                totaal += product.TotalPrice;
            }

            bon.AppendLine("-------------------------");
            bon.AppendLine($"Eindbedrag: €{totaal:0.00}");

            ShopManager.SetShopReceipt(bon.ToString());

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName ?? baseDir;
                string filePath = Path.Combine(projectRoot, "Bon.txt");
                File.WriteAllText(filePath, bon.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan bon: {ex.Message}");
            }
        }

        public void ResetButtonClick()
        {
            orderedProducts.Clear();
            ShopManager.SetShopReceipt("Eindbedrag\n€");
        }

        public void SaveButtonClick()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bon_Save.txt");
                File.WriteAllText(filePath, ShopManager.GetShopReceipt());
                MessageBox.Show("Bon opgeslagen.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan bon: {ex.Message}");
            }
        }
    }
}
