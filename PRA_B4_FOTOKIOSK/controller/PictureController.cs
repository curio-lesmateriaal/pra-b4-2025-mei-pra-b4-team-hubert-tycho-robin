using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // 👇 Deze property maakt het mogelijk om de Window te koppelen vanuit de UI (zoals Home.xaml.cs)
        public Window Window { get; set; }

        // Lijst van foto's die getoond zullen worden op het scherm
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        // Methode die alle logica uitvoert om foto's op te halen en te koppelen
        public void Start()
        {
            int today = (int)DateTime.Now.DayOfWeek; // 0 = zondag, 1 = maandag, ...
            PicturesToDisplay.Clear();

            DateTime now = DateTime.Now;
            DateTime lowerBound = now.AddMinutes(-30); // Ondergrens voor fototijd
            DateTime upperBound = now.AddMinutes(-2);  // Bovengrens (laatste 2 minuten negeren)

            List<(DateTime time, string path)> allPhotos = new();

            // Doorloop alle mappen in de fotos-map
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                string folderName = new DirectoryInfo(dir).Name;
                string[] parts = folderName.Split('_');

                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        string[] fileParts = filename.Split('_');

                        if (fileParts.Length >= 3 &&
                            int.TryParse(fileParts[0], out int hour) &&
                            int.TryParse(fileParts[1], out int minute) &&
                            int.TryParse(fileParts[2], out int second))
                        {
                            try
                            {
                                DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
                                if (photoTime >= lowerBound && photoTime <= upperBound)
                                {
                                    allPhotos.Add((photoTime, file));
                                }
                            }
                            catch { /* Ongeldige datum, negeren */ }
                        }
                    }
                }
            }

            // Sorteer foto's op tijd
            var sortedPhotos = allPhotos.OrderBy(p => p.time).ToList();

            // Groepeer foto's die binnen 10 seconden van elkaar gemaakt zijn
            List<List<(DateTime time, string path)>> groupedPhotos = new();

            foreach (var photo in sortedPhotos)
            {
                bool added = false;
                foreach (var group in groupedPhotos)
                {
                    if (Math.Abs((photo.time - group.Last().time).TotalSeconds) <= 10)
                    {
                        group.Add(photo);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    groupedPhotos.Add(new List<(DateTime, string)> { photo });
                }
            }

            // Splits de groepen in camera 1 en camera 2, op basis van of de seconden binnen 0–59 of 60–119 vallen
            List<List<(DateTime time, string path)>> cam1Groups = new();
            List<List<(DateTime time, string path)>> cam2Groups = new();

            foreach (var group in groupedPhotos)
            {
                var firstTime = group.First().time;
                int totalSeconds = firstTime.Minute * 60 + firstTime.Second;

                if (totalSeconds % 120 < 60)
                    cam1Groups.Add(group);
                else
                    cam2Groups.Add(group);
            }

            // Match groepen van cam1 en cam2 die exact 60 seconden uit elkaar liggen
            int cam1Index = 0;
            int cam2Index = 0;

            while (cam1Index < cam1Groups.Count && cam2Index < cam2Groups.Count)
            {
                var group1 = cam1Groups[cam1Index];
                var group2 = cam2Groups[cam2Index];

                DateTime time1 = group1.First().time;
                DateTime time2 = group2.First().time;

                double diff = (time2 - time1).TotalSeconds;

                if (Math.Abs(diff - 60) <= 2) // 60 seconden verschil, met ±2 sec tolerantie
                {
                    int count = Math.Min(group1.Count, group2.Count);
                    for (int i = 0; i < count; i++)
                    {
                        PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group1[i].path });
                        PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group2[i].path });
                    }

                    Console.WriteLine($"✔️ Match: Cam1 {time1:HH:mm:ss} + Cam2 {time2:HH:mm:ss} ({count} foto's)");
                    cam1Index++;
                    cam2Index++;
                }
                else if (time2 < time1.AddSeconds(59))
                {
                    cam2Index++;
                }
                else
                {
                    cam1Index++;
                }
            }

            // Log het totaal
            Console.WriteLine($"Aantal gekoppelde foto's: {PicturesToDisplay.Count}");

            // Update de beelden via de PictureManager (UI)
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Methode aangeroepen bij refresh-knop
        public void RefreshButtonClick()
        {
            Start();
        }
    }
}
