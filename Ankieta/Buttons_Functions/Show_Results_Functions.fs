module Show_Results_Functions
open Sql_Functions

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media



let get_Employee_Score_Sum (employee_Scores_List : Employee_Scores list) = 
    employee_Scores_List |> List.map(fun score -> score.Score) 
    |> List.reduce (fun acc score -> acc + score) 


let get_Employee_Median (employee_Scores: Employee_Scores list) =
    let unical_Scores = employee_Scores |> List.map (fun score -> score.Score) |> List.distinct   |> List.sort


    if unical_Scores.Length % 2 = 1 then
        float unical_Scores.[unical_Scores.Length / 2 - 1]
    else
        let mid1 = unical_Scores.[unical_Scores.Length / 2 - 1]
        let mid2 = unical_Scores.[unical_Scores.Length / 2 ]

        let average =(float mid1 + float mid2) / 2.0
        Math.Round(average, 2)

let get_Employee_Average (employee_Scores: Employee_Scores list) =
    let sum_Of_Score = get_Employee_Score_Sum employee_Scores

    let average = float sum_Of_Score / float employee_Scores.Length
    Math.Round(average, 2)

let get_Employee_Most_Popular_Scores (employee_Scores: Employee_Scores list) = 
    employee_Scores
    |> List.map (fun s -> s.Score)                      
    |> List.groupBy id                                  
    |> fun groups -> 
        let max_Frequency = groups |> List.map (fun (_, group) -> List.length group) |> List.max
        groups
        |> List.filter (fun (_, group) -> List.length group = max_Frequency) 
        |> List.map fst                                 
    |> List.sort                                        
    |> List.map string                                  
    |> String.concat ", " 

let get_Current_Respondent_Choice (employee_Scores: Employee_Scores list) (current_Respondent_Id : int) = 
    employee_Scores 
    |> List.filter(fun score -> score.Respondent_Id = current_Respondent_Id)
    |> List.map (fun score -> score.Score)
    |> List.head
    


let show_Results (stack_Panel : StackPanel) (respondent_Id : int) (window : Window) =
    stack_Panel.Children.Clear()

    let list_Of_Employees = sql_Get_Employees_With_Scores()

    
    let amount_Of_Evaluations = list_Of_Employees.Head.Scores.Length

    let amount_Of_Evaluations_Label = new Label()

    amount_Of_Evaluations_Label.Height <- window.Height / 13.0
    amount_Of_Evaluations_Label.Width <- window.Width /5.0
    amount_Of_Evaluations_Label.FontSize <- 25.0
    amount_Of_Evaluations_Label.Padding <- new System.Windows.Thickness(10, 20, amount_Of_Evaluations_Label.Padding.Right, amount_Of_Evaluations_Label.Padding.Bottom)
    amount_Of_Evaluations_Label.Margin <- new System.Windows.Thickness(amount_Of_Evaluations_Label.Margin.Left, 10, amount_Of_Evaluations_Label.Margin.Right, amount_Of_Evaluations_Label.Margin.Bottom)
    amount_Of_Evaluations_Label.HorizontalAlignment <- HorizontalAlignment.Left
    amount_Of_Evaluations_Label.Background <- Brushes.White
    amount_Of_Evaluations_Label.Content <- sprintf "Ilość wypełnionych ankiet: %d" amount_Of_Evaluations

    stack_Panel.Children.Add(amount_Of_Evaluations_Label) |> ignore


    for employee in list_Of_Employees do
        let employee_Result_Panel = new StackPanel()
        employee_Result_Panel.Height <- window.Height / 4.0
        employee_Result_Panel.Width <- window.Width /2.0

        employee_Result_Panel.HorizontalAlignment <- HorizontalAlignment.Center
        employee_Result_Panel.VerticalAlignment <- VerticalAlignment.Center
        employee_Result_Panel.Background <- Brushes.White
        employee_Result_Panel.Margin <- new System.Windows.Thickness(employee_Result_Panel.Margin.Left, 10, employee_Result_Panel.Margin.Right, employee_Result_Panel.Margin.Bottom)

        let employee_Info_Label = new Label()
        employee_Info_Label.Margin <- new System.Windows.Thickness(20.0)
        employee_Info_Label.FontSize <- 25.0
        

        let employee_Sum_Of_Score = get_Employee_Score_Sum employee.Scores
        let employee_Score_Median = get_Employee_Median employee.Scores
        let employee_Scores_Avreage = get_Employee_Average employee.Scores
        let employee_Most_Popular_Score = get_Employee_Most_Popular_Scores employee.Scores

        if respondent_Id <> -1 then
            let current_Responent_Choice =  get_Current_Respondent_Choice employee.Scores respondent_Id

            employee_Result_Panel.Height <- window.Height / 3.5
            employee_Info_Label.Content <- sprintf "Oceń prace pracownika: %s %s || Dział: %s

            Suma wszystkich ocen: %d
            Średnia ocen: %.2f
            Mediana: %.2f
            Najczęściej wybierana ocena: %s

            Twój wybór: %d" employee.Employee.Name employee.Employee.Surname employee.Employee.Departament employee_Sum_Of_Score employee_Scores_Avreage employee_Score_Median employee_Most_Popular_Score current_Responent_Choice
        else
            employee_Info_Label.Content <- sprintf "Oceń prace pracownika: %s %s || Dział: %s

            Suma wszystkich ocen: %d
            Średnia ocen: %.2f
            Mediana: %.2f
            Najczęściej wybierana ocena: %s" employee.Employee.Name employee.Employee.Surname employee.Employee.Departament employee_Sum_Of_Score employee_Scores_Avreage  employee_Score_Median employee_Most_Popular_Score
        
        
        employee_Result_Panel.Children.Add(employee_Info_Label) |> ignore
        stack_Panel.Children.Add(employee_Result_Panel) |> ignore

