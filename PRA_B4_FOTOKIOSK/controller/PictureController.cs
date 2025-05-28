using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }

        // De lijst met foto's die we op het scherm willen tonen
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        // Deze methode wordt uitgevoerd wanneer de foto-pagina opent
        public void Start()
        {
            // Bepaal welk dagnummer het vandaag is (0 = Zondag, 1 = Maandag, ..., 6 = Zaterdag)
            int today = (int)DateTime.Now.DayOfWeek;

            // Maak de lijst leeg zodat we opnieuw kunnen vullen
            PicturesToDisplay.Clear();

            // Haal het huidige tijdstip op
            DateTime now = DateTime.Now;

            // Bereken de onder- en bovengrens voor geldige foto's
            // Alleen foto's tussen 2 en 30 minuten geleden mogen worden getoond
            DateTime lowerBound = now.AddMinutes(-30); // 30 minuten geleden
            DateTime upperBound = now.AddMinutes(-2);  // 2 minuten geleden

            // Loop door alle dagmappen in de fotos-map
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Bijvoorbeeld: mapnaam "0_Zondag"
                string folderName = new DirectoryInfo(dir).Name;

                // Splits de mapnaam op "_" om het dagnummer eruit te halen
                string[] parts = folderName.Split('_');

                // Controleer of het eerste deel van de mapnaam een geldig getal is
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay))
                {
                    // Alleen doorgaan als het dagnummer van de map overeenkomt met vandaag
                    if (folderDay == today)
                    {
                        // Loop door alle bestanden in deze dagmap
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            // Haal de bestandsnaam zonder extensie op
                            // Bijvoorbeeld: "10_06_32_id2125"
                            string filename = Path.GetFileNameWithoutExtension(file);
                            string[] fileParts = filename.Split('_');

                            // Verwacht dat de bestandsnaam begint met uur_minuut_seconde_...
                            if (fileParts.Length >= 3 &&
                                int.TryParse(fileParts[0], out int hour) &&
                                int.TryParse(fileParts[1], out int minute) &&
                                int.TryParse(fileParts[2], out int second))
                            {
                                try
                                {
                                    // Maak een DateTime object van de tijd in de bestandsnaam
                                    DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);

                                    // Controleer of de foto binnen het tijdsvenster valt
                                    if (photoTime >= lowerBound && photoTime <= upperBound)
                                    {
                                        // Voeg geldige foto toe aan de lijst
                                        PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                                    }
                                }
                                catch
                                {
                                    // Als er een fout optreedt (bijv. ongeldige tijd), sla de foto over
                                }
                            }
                        }
                    }
                }
            }

            // Update de foto's in de gebruikersinterface
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Deze methode wordt uitgevoerd als er op de "Refresh"-knop is geklikt
        public void RefreshButtonClick()
        {
            // Herlaad de foto's van vandaag
            Start();
        }
    }
}
