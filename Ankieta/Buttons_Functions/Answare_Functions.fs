module Answare_Functions

open Sql_Functions
open Show_Results_Functions
open System.Windows
open System.Windows.Controls
open System.Windows.Media


let save_Button = new Button()
let error_Label = new Label()

let check_Score (score_TextBox : TextBox)  (save_Button : Button)=
    try
        let score_As_Int = int score_TextBox.Text

        if score_As_Int > 10 || score_As_Int < 0 then
            score_TextBox.BorderBrush <- System.Windows.Media.Brushes.Red
            save_Button.IsEnabled <- false
        else
            score_TextBox.BorderBrush <- Brushes.LightGray
            save_Button.IsEnabled <- true
    with
    | :? System.FormatException ->
        score_TextBox.BorderBrush <- System.Windows.Media.Brushes.Red
        save_Button.IsEnabled <- false


let is_TextBox_Red (text_Box : TextBox) =
    match text_Box.BorderBrush with
    | :? SolidColorBrush as brush when brush.Color = System.Windows.Media.Colors.Red -> true
    | _ -> false

let all_TextBoxes_Have_Value (score_Panels : StackPanel list) =
    let any_Empty_Score =
        score_Panels
        |> List.exists (fun panel ->
            match panel with
            | :? StackPanel as sp ->
                sp.Children
                |> Seq.cast<UIElement>
                |> Seq.exists (fun child ->
                    match child with
                    | :? TextBox as te when te.Tag = "Score_TextBox" ->
                        let text = te.Text.Trim() 
                        if System.String.IsNullOrWhiteSpace(text) then
                            true 
                        else
                            false 
                    | _ -> false)
            | _ -> false)

    any_Empty_Score




let save_Scores (result_Panel : StackPanel) (respondent_Id : int) = 

    let score_Panels =
        result_Panel.Children
        |> Seq.cast<UIElement>
        |> Seq.filter (fun child -> child :? StackPanel)
        |> Seq.toList
    
    
    for panel in score_Panels do
        match panel with
        | :? StackPanel as stackPanel -> 
            let employee_Id =
                stackPanel.Children
                |> Seq.cast<UIElement>
                |> Seq.tryFind (fun child -> 
                    match child with
                    | :? FrameworkElement as element when element.Tag = "Employee_Id" -> true
                    | _ -> false 
                )

            let employee_Score =
                stackPanel.Children
                |> Seq.cast<UIElement>
                |> Seq.tryFind (fun child -> 
                    match child with
                    | :? FrameworkElement as element when element.Tag = "Score_TextBox" -> true
                    | _ -> false
                )

            match employee_Id, employee_Score with
            | Some(eId), Some(eScore) -> 
                let employee_Id = (eId :?> Label).Content.ToString()
                let score = (eScore :?> TextBox).Text.ToString()
            
                let insert_Score_Query = 
                    sprintf "INSERT INTO Employee_Evaluation VALUES (%s, %d, %s)"
                        employee_Id respondent_Id score

                sql_Result_Data_Insert insert_Score_Query
            
            | _ -> ()
        | _ -> ()

let check_TextBoxes (stack_Panel : StackPanel) (current_Respondent_Id : int) (window : Window) =
    let score_Panels =
        stack_Panel.Children
        |> Seq.cast<UIElement>
        |> Seq.choose (fun child ->
            match child with
            | :? StackPanel as sp -> Some sp
            | _ -> None)
        |> Seq.toList

    let any_Empty_Boxes = all_TextBoxes_Have_Value score_Panels

    if any_Empty_Boxes then
        error_Label.Visibility <- Visibility.Visible
        ()

    else
        save_Scores stack_Panel current_Respondent_Id |> ignore
        show_Results stack_Panel current_Respondent_Id window |> ignore

let create_Answare_Panel (stack_Panel : StackPanel) (window : Window) = 
    stack_Panel.Children.Clear() |> ignore

    

    let create_Respondnt_Query = "INSERT INTO Respondents VALUES(GETDATE())"
    let current_Respondent_Id = sql_Responent_Data_Insert create_Respondnt_Query

    let employee_List = sql_Get_Employees() 
    
    let score_Info_Label = new Label()
    score_Info_Label.Content <- "Skala ocen: 0 - 10
    0: Bardzo źle
    10: Idealnie"

    score_Info_Label.Background <- Brushes.White
    stack_Panel.Children.Add(score_Info_Label) |> ignore
    score_Info_Label.FontSize <- 25.0

    for employee in employee_List do
        

        let employee_Score_Panel = new StackPanel()
        employee_Score_Panel.Orientation <- Orientation.Vertical
        employee_Score_Panel.Height <- window.Height / 6.0
        employee_Score_Panel.Width <- window.Width /2.0

        employee_Score_Panel.HorizontalAlignment <- HorizontalAlignment.Center
        employee_Score_Panel.VerticalAlignment <- VerticalAlignment.Center
        employee_Score_Panel.Background <- Brushes.White
        employee_Score_Panel.Margin <- new System.Windows.Thickness(employee_Score_Panel.Margin.Left, 10, employee_Score_Panel.Margin.Right, employee_Score_Panel.Margin.Bottom)

        let employee_Info_Label = new Label()
        employee_Info_Label.Margin <- new System.Windows.Thickness(20.0)
        employee_Info_Label.FontSize <- 25.0
        employee_Info_Label.Content <- sprintf "Oceń prace pracownika: %s %s || Dział: %s" employee.Name employee.Surname employee.Departament


        let employee_Score_TextBox = new TextBox()
        employee_Score_TextBox.BorderThickness <- new System.Windows.Thickness(3.0)
        employee_Score_TextBox.Height <- employee_Score_Panel.Height / 3.5
        employee_Score_TextBox.Width <- employee_Score_Panel.Width/ 10.0
        employee_Score_TextBox.Tag <- "Score_TextBox"
        employee_Score_TextBox.HorizontalAlignment <- HorizontalAlignment.Left
        employee_Score_TextBox.FontSize <- 25.0
        employee_Score_TextBox.Margin <- new System.Windows.Thickness(3.0)
        employee_Score_TextBox.Margin <- new System.Windows.Thickness(20.0, employee_Score_TextBox.Margin.Top, employee_Score_TextBox.Margin.Right, employee_Score_TextBox.Margin.Bottom)
        employee_Score_TextBox.Padding <- new System.Windows.Thickness(6.0)
        employee_Score_TextBox.BorderBrush <- Brushes.LightGray
        


        employee_Score_TextBox.TextChanged.Add(fun _ -> check_Score employee_Score_TextBox save_Button) |> ignore
        
        let employee_Id_Hiden_Label = new Label()
        employee_Id_Hiden_Label.Content <- employee.Id
        employee_Id_Hiden_Label.Visibility <- Visibility.Collapsed
        employee_Id_Hiden_Label.Tag <- "Employee_Id"
        
        employee_Score_Panel.Children.Add(employee_Id_Hiden_Label) |> ignore
        employee_Score_Panel.Children.Add(employee_Info_Label) |> ignore
        employee_Score_Panel.Children.Add(employee_Score_TextBox) |> ignore
        stack_Panel.Children.Add(employee_Score_Panel) |> ignore
    

    error_Label.Height <- window.Height / 20.0
    error_Label.Width <- window.Width / 4.0
    error_Label.Margin <- new System.Windows.Thickness(save_Button.Margin.Left, 20.0, save_Button.Margin.Right, 20.0)
    error_Label.Content <- "Nie wypełniono wszytkich pól ankiety."
    error_Label.Foreground <- System.Windows.Media.Brushes.Red
    error_Label.Background <- Brushes.White
    error_Label.FontSize <- 25.0
    error_Label.HorizontalAlignment <- HorizontalAlignment.Right
    error_Label.Visibility <- Visibility.Collapsed

    save_Button.IsEnabled <- false
    save_Button.Background <- Brushes.LightBlue
    save_Button.Height <- window.Height / 15.0
    save_Button.Width <- window.Width / 7.0
    save_Button.Margin <- new System.Windows.Thickness(save_Button.Margin.Left, 20.0, save_Button.Margin.Right, 20.0)
    save_Button.Content <- "Zapisz"
    save_Button.FontSize <- 25.0
    save_Button.HorizontalAlignment <- HorizontalAlignment.Right

    save_Button.Click.Add(fun _ -> (check_TextBoxes stack_Panel current_Respondent_Id window))

    stack_Panel.Children.Add(error_Label) |> ignore
    stack_Panel.Children.Add(save_Button) |> ignore