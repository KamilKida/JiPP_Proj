open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.IO

open Answare_Functions
open Show_Results_Functions

[<STAThread>]
[<EntryPoint>]
let main(_) = 
    
    //Główne okno aplikacji
    let window = new Window()
    window.Title <- "Ankieta"
    window.WindowState <- WindowState.Maximized

    let currentDirectory = Directory.GetCurrentDirectory()
    let icon_Path = Path.Combine(currentDirectory, "Ankieta_Ikona.png")

    window.Icon <- new System.Windows.Media.Imaging.BitmapImage(new Uri(icon_Path))
    window.Background <-  new SolidColorBrush(Color.FromRgb(162uy, 110uy, 184uy))


    let stack_Panel = new StackPanel()
    stack_Panel.Orientation <- Orientation.Vertical 
    stack_Panel.VerticalAlignment <- VerticalAlignment.Center 
    stack_Panel.HorizontalAlignment <- HorizontalAlignment.Center

    let btn_Answare = new Button()
    btn_Answare.Content <- "Odpowiedz na ankiete"
    btn_Answare.Width <- 260.0
    btn_Answare.Height <- 75.0
    btn_Answare.FontSize <- 25.0

    btn_Answare.Click.Add(fun _ -> create_Answare_Panel stack_Panel window)

    let btn_Results = new Button()
    btn_Results.Content <- "Wyniki ankiety"
    btn_Results.Width <- 260.0
    btn_Results.Height <- 75.0
    btn_Results.FontSize <- 25.0
    btn_Results.Margin <- new System.Windows.Thickness(10.0)

    btn_Results.Click.Add(fun _ -> show_Results stack_Panel -1 window)

    stack_Panel.Children.Add(btn_Answare) |> ignore
    stack_Panel.Children.Add(btn_Results) |> ignore

    let scroll_View = new ScrollViewer()
    scroll_View.Content <- stack_Panel

    window.Content <- scroll_View

    let app = new Application()
    app.Run(window) |> ignore

    0
