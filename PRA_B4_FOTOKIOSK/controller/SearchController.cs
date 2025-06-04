using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class SearchController
    {
        // Maak Window nullable om fout te vermijden
        public static Home? Window { get; set; }

        public void Start()
        {
            // Hier kun je eventueel UI resetten als dat nodig is
        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            if (Window == null)
            {
                MessageBox.Show("Venster is niet beschikbaar.");
                return;
            }

            var selectedItem = Window.cbDagenVanDeWeek?.SelectedItem as ComboBoxItem;
            string? selectedDay = selectedItem?.Content as string;
            string time = SearchManager.GetSearchInput();
            int selectedFolder = -1;

            string[] timeParts = time.Split(':');
            if (selectedDay == null || timeParts.Length != 3 ||
                !int.TryParse(timeParts[0], out int iHour) ||
                !int.TryParse(timeParts[1], out int iMinute) ||
                !int.TryParse(timeParts[2], out int iSecond))
            {
                MessageBox.Show("Geen geldige invoer");
                return;
            }

            // Dag omzetten naar mapnummer
            selectedFolder = selectedDay.ToLower() switch
            {
                "zondag" => 0,
                "maandag" => 1,
                "dinsdag" => 2,
                "woensdag" => 3,
                "donderdag" => 4,
                "vrijdag" => 5,
                "zaterdag" => 6,
                _ => -1
            };

            if (selectedFolder == -1)
            {
                MessageBox.Show("Ongeldige dag geselecteerd");
                return;
            }

            bool isFound = false;
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                string folderName = Path.GetFileName(dir); // bijv. "0_Zondag"
                string[] parts = folderName.Split('_');

                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == selectedFolder)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string fileName = Path.GetFileName(file); // bijv. "10_05_30_id8824.jpg"
                        string[] fileParts = fileName.Split('_');

                        if (fileParts.Length >= 4 &&
                            int.TryParse(fileParts[0], out int hour) &&
                            int.TryParse(fileParts[1], out int minute) &&
                            int.TryParse(fileParts[2], out int second))
                        {
                            if (hour == iHour && minute == iMinute && second == iSecond)
                            {
                                SearchManager.SetPicture(file);
                                isFound = true;

                                string displayMinute = minute.ToString("D2");
                                string displaySecond = second.ToString("D2");
                                string id = fileParts[3].Replace("id", "").Replace(".jpg", "");

                                string text = $"Foto gevonden met tijd en datum: {selectedDay}-{hour}:{displayMinute}:{displaySecond} met id: {id}";
                                SearchManager.SetSearchImageInfo(text);
                                break;
                            }
                        }
                    }

                    if (isFound) break;
                }
            }

            if (!isFound)
            {
                MessageBox.Show("Geen foto gevonden met deze tijd");
            }
        }
    }
}
