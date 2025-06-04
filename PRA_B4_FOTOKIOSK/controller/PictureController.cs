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
        public static Home Window { get; set; }

        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        public void Start()
        {
            int today = (int)DateTime.Now.DayOfWeek;
            PicturesToDisplay.Clear();
            DateTime now = DateTime.Now;

            // Tijdsvenster: 30 tot 2 minuten geleden
            DateTime lowerBound = now.AddMinutes(-30);
            DateTime upperBound = now.AddMinutes(-2);

            List<(DateTime time, string path)> allPhotos = new List<(DateTime, string)>();

            // Doorloop elke map in de fotos-directory
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                string folderName = new DirectoryInfo(dir).Name;
                string[] parts = folderName.Split('_');

                // Controleer of folder bij juiste dag hoort
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        string[] fileParts = filename.Split('_');

                        // Probeer tijd uit bestandsnaam te halen
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
                            catch
                            {
                                // Ongeldige tijd, overslaan
                            }
                        }
                    }
                }
            }

            // Sorteer alle foto's op tijd
            var sortedPhotos = allPhotos.OrderBy(p => p.time).ToList();

            // Groepeer foto's die binnen 10 seconden van elkaar vallen
            List<List<(DateTime time, string path)>> groupedPhotos = new List<List<(DateTime, string)>>();

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

            // Zoek paren van groepen die 60 seconden uit elkaar liggen
            for (int i = 0; i < groupedPhotos.Count; i++)
            {
                var group1 = groupedPhotos[i];
                DateTime time1 = group1.First().time;

                for (int j = i + 1; j < groupedPhotos.Count; j++)
                {
                    var group2 = groupedPhotos[j];
                    DateTime time2 = group2.First().time;

                    if (Math.Abs((time2 - time1).TotalSeconds - 60) <= 1)
                    {
                        int count = Math.Min(group1.Count, group2.Count);
                        for (int k = 0; k < count; k++)
                        {
                            PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group1[k].path });
                            PicturesToDisplay.Add(new KioskPhoto { Id = 0, Source = group2[k].path });
                        }
                        break; // Stop na eerste match met 60 seconden verschil
                    }
                }
            }

            // Update de foto-weergave
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        public void RefreshButtonClick()
        {
            Start();
        }
    }
}
