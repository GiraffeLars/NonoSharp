using Picross.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Maui
{
    internal partial class SelectionPage : ThemedPage
    {
        int availablePuzzles;
        Grid menu;

        internal SelectionPage()
        {
            //var puzzleDirStream = FileSystem.OpenAppPackageFileAsync("Puzzles").Result;
            //var a = new StreamReader(puzzleDirStream);
            //var b = a.

            //var puzzleDir = Path.Combine(appDir, "Puzzles");
            //availablePuzzles = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).Length;

            menu = new()
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,

                ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
            },

                RowDefinitions =
            {
                new RowDefinition(),
                new RowDefinition(),
                new RowDefinition(),
                new RowDefinition(),
                new RowDefinition()
            }
            };

            for (int i = 0; i < 1; i++)
            {
                Button button = new()
                {
                    Text = $"{i + 1}",
                    CommandParameter = i
                };
                button.Clicked += async (s, e) =>
                {
                    var bytes = await LoadPuzzleAsync((int) button.CommandParameter);

                    await Navigation.PushAsync(new GamePage(await GameAPI.LoadFromSerializedAsync(bytes)));
                };

                menu.Add(button, i % 5 + 1, i / 5);
            }

            Content = menu;
        }

        private async Task<byte[]> LoadPuzzleAsync(int i)
        {
            var puzzleStream = await FileSystem.OpenAppPackageFileAsync($"Puzzles/puzzle{i}.ns");
            using var ms = new MemoryStream();
            await puzzleStream.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();
            return bytes;
        }
    }
}
