using NonoSharp.Exceptions;
using NonoSharp.Maui.Drawables;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Maui
{
    public partial class NonogramBuilderPage : ThemedPage
    {
        BuilderDrawable drawable;
        Grid menu;

        public NonogramBuilderPage(NonogramBuilder builder)
        {
            drawable = new(builder);

            menu = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,

                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(5, GridUnitType.Star)},
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                        ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            GraphicsView builderView = new GraphicsView() { Drawable = drawable };

            Button titleButton = new Button() { Text = "Set puzzle title" };
            titleButton.Clicked += async (s, e) =>
            {

            };

            Button saveButton = new Button() { Text = "Save to file" };
            saveButton.Clicked += async (s, e) =>
            {
                try
                {
                    saveButton.IsEnabled = false;

                    string path = Path.Combine(FileSystem.AppDataDirectory, builder.Title ?? "NonoSharp_puzzle", ".ns");
                    await builder.SaveAsFileAsync(path);
                    await DisplayAlertAsync("Successfully saved puzzle!", "The puzzle was successfully saved" +
                        $"to {path}", "OK");
                }
                catch (PuzzleNotSolvableException exc)
                {
                    await DisplayAlertAsync("Could not save puzzle!", exc.Message, "OK");
                }
                catch (Exception)
                {
                    await DisplayAlertAsync("Could not save puzzle!", "An error has occurred while trying to " +
                        "save to puzzle to disk. Please try again.", "OK");
                }
                finally
                { 
                    saveButton.IsEnabled = true; 
                }
                    

            };

            menu.Add(builderView, 0, 0);
            menu.Add(saveButton, 0, 2);
        }
    }
}
