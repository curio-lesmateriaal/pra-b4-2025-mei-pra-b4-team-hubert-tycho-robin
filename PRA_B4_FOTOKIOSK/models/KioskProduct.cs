using System;
using System.Collections.Generic;

namespace PRA_B4_FOTOKIOSK.models
{
    public class KioskProduct
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public static class ShopManager
    {
        public static List<KioskProduct> Products = new List<KioskProduct>
        {
            new KioskProduct
            {
                Name = "15 bij 20",
                Price = 4.00m,
                Description = "Fotoafdruk 15x20 cm"
            },
            new KioskProduct
            {
                Name = "Sleutelhanger",
                Price = 7.00m,
                Description = "Sleutelhanger met foto"
            },
            new KioskProduct
            {
                Name = "Mok",
                Price = 9.33m,
                Description = "Mok met persoonlijke foto"
            },
            new KioskProduct
            {
                Name = "T-shirt",
                Price = 12.69m,
                Description = "T-shirt met opdruk"
            }
        };

        private static string priceList = "";

        public static void SetShopPriceList(string text)
        {
            priceList = text;
        }

        public static void AddShopPriceList(string text)
        {
            priceList += text;
        }

        public static string GetShopPriceList()
        {
            return priceList;
        }

        public static void UpdatePriceList()
        {
            SetShopPriceList("");

            foreach (KioskProduct product in Products)
            {
                AddShopPriceList($"{product.Name} - €{product.Price:F2} : {product.Description}\n");
            }
        }
    }
}
