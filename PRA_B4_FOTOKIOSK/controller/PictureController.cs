using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }

        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        // Start methode die wordt aangeroepen wanneer de foto pagina opent
        public void Start()
        {
            // Bepaal het dagnummer van vandaag (0 = Zondag, 1 = Maandag, ..., 6 = Zaterdag)
            int today = (int)DateTime.Now.DayOfWeek;

            // Leeg eerst de lijst, voor het geval deze opnieuw gevuld wordt
            PicturesToDisplay.Clear();

            // Loop door alle submappen in de fotos map
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Haal alleen de mapnaam op, bijvoorbeeld "2_Dinsdag"
                string folderName = new DirectoryInfo(dir).Name;

                // Split de naam op "_" en probeer het eerste deel om te zetten naar een int
                string[] parts = folderName.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay))
                {
                    // Als de dag overeenkomt met vandaag, laad dan de foto's
                    if (folderDay == today)
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                        }
                    }
                }
            }

            // Update de fotos in de UI
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {
            // Herlaad de foto's van vandaag
            Start();
        }
    }
}
