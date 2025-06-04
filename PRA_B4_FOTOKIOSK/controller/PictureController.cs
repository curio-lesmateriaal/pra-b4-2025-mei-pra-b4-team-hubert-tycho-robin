using PRA_B4_FOTOKIOSK.magie;       // Namespace voor magie (vermoedelijk business logic)
using PRA_B4_FOTOKIOSK.models;      // Namespace voor modelklassen zoals KioskPhoto
using System;
using System.Collections.Generic;
using System.IO;                    // Voor directory- en bestandsmanipulatie
using System.Linq;                  // Voor LINQ functionaliteit zoals OrderBy

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // Lijst met foto's die getoond moeten worden in de kiosk
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        // Start-methode die de foto's verzamelt en koppelt
        public void Start()
        {
            // Bepaal de huidige dag van de week (0=Zondag, 1=Maandag, ...)
            int today = (int)DateTime.Now.DayOfWeek;

            // Maak de lijst met foto's leeg voor nieuwe data
            PicturesToDisplay.Clear();

            // Huidige tijd
            DateTime now = DateTime.Now;

            // Ondergrens: 30 minuten geleden
            DateTime lowerBound = now.AddMinutes(-30);

            // Bovengrens: 2 minuten geleden (om recente foto's niet te pakken)
            DateTime upperBound = now.AddMinutes(-2);

            // Lijst met alle gevonden foto's met hun tijd en pad
            List<(DateTime time, string path)> allPhotos = new List<(DateTime, string)>();

            // Loop door alle submappen in de fotos-map
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Haal de mapnaam op (bijv. "1_something")
                string folderName = new DirectoryInfo(dir).Name;

                // Split mapnaam op '_', we verwachten dat het eerste deel dagnummer is
                string[] parts = folderName.Split('_');

                // Controleer of het eerste deel een dagnummer is en gelijk is aan vandaag
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                {
                    // Loop door alle bestanden in deze map
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        // Haal bestandsnaam zonder extensie op (bijv. "12_30_15")
                        string filename = Path.GetFileNameWithoutExtension(file);

                        // Split op '_' om uur, minuut, seconde te krijgen
                        string[] fileParts = filename.Split('_');

                        // Controleer of we minimaal uur, minuut, seconde hebben
                        if (fileParts.Length >= 3 &&
                            int.TryParse(fileParts[0], out int hour) &&
                            int.TryParse(fileParts[1], out int minute) &&
                            int.TryParse(fileParts[2], out int second))
                        {
                            try
                            {
                                // Maak een DateTime object van vandaag met het uur, minuut, seconde van de foto
                                DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);

                                // Controleer of deze foto binnen de gewenste tijdspanne valt
                                if (photoTime >= lowerBound && photoTime <= upperBound)
                                {
                                    // Voeg foto toe aan de lijst met alle foto's
                                    allPhotos.Add((photoTime, file));
                                }
                            }
                            catch
                            {
                                // Negeer fouten bij ongeldige data
                            }
                        }
                    }
                }
            }

            // Sorteer alle foto's chronologisch op tijd
            var sortedPhotos = allPhotos.OrderBy(p => p.time).ToList();

            // Lijst om foto's te groeperen die dicht bij elkaar in tijd zijn (max 10 sec verschil)
            List<List<(DateTime time, string path)>> groupedPhotos = new List<List<(DateTime, string)>>();

            // Loop door alle gesorteerde foto's en groepeer ze
            foreach (var photo in sortedPhotos)
            {
                bool added = false;

                // Kijk of foto binnen 10 seconden past bij een bestaande groep
                foreach (var group in groupedPhotos)
                {
                    if (Math.Abs((photo.time - group.Last().time).TotalSeconds) <= 10)
                    {
                        // Voeg foto toe aan deze groep
                        group.Add(photo);
                        added = true;
                        break;
                    }
                }

                // Als foto niet toegevoegd is aan bestaande groep, maak een nieuwe groep
                if (!added)
                {
                    groupedPhotos.Add(new List<(DateTime, string)> { photo });
                }
            }

            // Lijsten voor foto's van camera 1 en camera 2
            List<List<(DateTime time, string path)>> cam1Groups = new();
            List<List<(DateTime time, string path)>> cam2Groups = new();

            // Splits de groepen op in camera1 of camera2 op basis van het tijdstip
            foreach (var group in groupedPhotos)
            {
                // Pak de tijd van de eerste foto in de groep
                var firstTime = group.First().time;

                // Bereken hoeveel seconden er verstreken zijn in het uur
                int totalSeconds = firstTime.Minute * 60 + firstTime.Second;

                // Modulo 120: als resultaat < 60 dan camera 1, anders camera 2
                if (totalSeconds % 120 < 60)
                    cam1Groups.Add(group); // Camera 1 groep
                else
                    cam2Groups.Add(group); // Camera 2 groep
            }

            // Indexen voor itereren door camera 1 en camera 2 groepen
            int cam1Index = 0;
            int cam2Index = 0;

            // Loop totdat we door alle groepen van camera 1 of 2 zijn
            while (cam1Index < cam1Groups.Count && cam2Index < cam2Groups.Count)
            {
                var group1 = cam1Groups[cam1Index]; // Huidige camera 1 groep
                var group2 = cam2Groups[cam2Index]; // Huidige camera 2 groep

                // Tijdstip van eerste foto in elke groep
                DateTime time1 = group1.First().time;
                DateTime time2 = group2.First().time;

                // Bereken het tijdsverschil in seconden tussen de twee groepen
                double diff = (time2 - time1).TotalSeconds;

                // Check of de twee foto's ongeveer 60 seconden verschil hebben (±2 seconden tolerantie)
                if (Math.Abs(diff - 60) <= 2)
                {
                    // Bepaal hoeveel foto's gekoppeld kunnen worden (kleinste aantal per groep)
                    int count = Math.Min(group1.Count, group2.Count);

                    // Voeg gekoppelde foto's toe aan de lijst voor weergave, steeds paar voor paar
                    for (int i = 0; i < count; i++)
                    {
                        PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group1[i].path });
                        PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group2[i].path });
                    }

                    // Log welke groepen gekoppeld zijn
                    Console.WriteLine($"✔️ Match: Cam1 {time1:HH:mm:ss} + Cam2 {time2:HH:mm:ss} ({count} foto's)");

                    // Ga naar volgende groep in beide lijsten
                    cam1Index++;
                    cam2Index++;
                }
                else if (time2 < time1.AddSeconds(59))
                {
                    // Camera 2 tijd ligt vóór de 59 seconden na camera 1 foto -> schuif cam2 index door
                    cam2Index++;
                }
                else
                {
                    // Camera 2 foto te laat, schuif cam1 index door
                    cam1Index++;
                }
            }

            // Toon het totaal aantal gekoppelde foto's
            Console.WriteLine($"Aantal gekoppelde foto's: {PicturesToDisplay.Count}");

            // Werk de foto's bij in de PictureManager (UI of andere weergave)
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Methode om de foto's handmatig te verversen (bijvoorbeeld via een knop)
        public void RefreshButtonClick()
        {
            Start();
        }
    }
}
