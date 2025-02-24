module Sql_Functions 
    
open System.Data.SqlClient

    
type Employee = {
    Id: int
    Name: string
    Surname: string
    Departament: string
    
        }



type Employee_Scores ={
    Employee_Id : int
    Respondent_Id : int
    Score : int
    }

type Employee_With_Score = {
    Employee: Employee
    Scores : Employee_Scores list
    }

let connection_String = "Server=DESKTOP-1R7C1KM;Database=Questionnaire;Integrated Security=True;"

let rec read_Employee_Data (reader: SqlDataReader) (acc: Employee list) : Employee list =
    if reader.Read() then
        let employee = 
            { Id = reader.GetInt32(0)
              Name = reader.GetString(1)
              Surname = reader.GetString(2)
              Departament = reader.GetString(3)  
            }


        read_Employee_Data reader (employee :: acc)
    else
        List.rev acc


let rec read_Employee_Scores (reader: SqlDataReader) (acc: Employee_Scores list) : Employee_Scores list =
    if reader.Read() then
        let score = 
            { Employee_Id = reader.GetInt32(0)
              Respondent_Id = reader.GetInt32(1)
              Score = reader.GetInt32(2) }

        read_Employee_Scores reader (score :: acc)
    else
        List.rev acc

let sql_Get_Employees (_) = 
    
    
    use connection = new SqlConnection(connection_String)
    connection.Open()

    use command = new SqlCommand("SELECT * FROM Employees", connection)

    use reader = command.ExecuteReader()
    read_Employee_Data reader [] 


let sql_Get_Employees_With_Scores (_) =
    
    let employees_List = sql_Get_Employees()

    use connection = new SqlConnection(connection_String)
    connection.Open()

    let sql_Get_Employees_Scores_Query = "SELECT Employee_Id, Respondent_Id, Evaluation_Score FROM Employee_Evaluation"
    use scoreCommand = new SqlCommand(sql_Get_Employees_Scores_Query, connection)
    use scoreReader = scoreCommand.ExecuteReader()
    let scores = read_Employee_Scores scoreReader []

    let grouped_Scores = 
        scores
        |> List.groupBy (fun score -> score.Employee_Id)


    let employees_With_Scores = 
        employees_List
        |> List.map (fun emp -> 
            let employeeScores = 
                grouped_Scores 
                |> List.tryFind (fun (id, _) -> id = emp.Id)
                |> Option.map snd
                |> Option.defaultValue []
            { Employee = emp; Scores = employeeScores })

    employees_With_Scores

let sql_Responent_Data_Insert sql_Query =
    
    use connection = new SqlConnection(connection_String)
    connection.Open()

    use command = new SqlCommand(sql_Query + "; SELECT SCOPE_IDENTITY();", connection)

    let result = command.ExecuteScalar() 

    match result with
    | :? int as id -> id 
    | :? decimal as id -> int id


let sql_Result_Data_Insert sql_Query = 
    
    use connection = new SqlConnection(connection_String)
    connection.Open()

    use command = new SqlCommand(sql_Query, connection)
    command.ExecuteNonQuery() |> ignore
