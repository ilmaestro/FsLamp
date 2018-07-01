module LUISApi


module Model =
    type ApiSettings = {
        Region: string
        AppId: string
        AppKey: string
    }

    type LuisQuery = {
        Query: string
        TopScoringIntent: IntentResult
        Entities: EntityResult list
    }

    and IntentResult = {
        Intent: string
        Score: float
    }

    and EntityResult = {
        Entity: string
        Type: string
        StartIndex: int
        EndIndex: int
        Score: float
        Role: string
    }

module Client =
    open System.Net
    open System.Net.Http
    open Newtonsoft.Json

    let private httpClient = new HttpClient()

    let getResponse (settings: Model.ApiSettings) query = 
        async {
            let url =
                sprintf "https://%s.api.cognitive.microsoft.com/luis/v2.0/apps/%s?q=%s"
                    settings.Region
                    settings.AppId
                    query

            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", settings.AppKey)

            let! response = httpClient.GetAsync(url) |> Async.AwaitTask

            if response.StatusCode <> HttpStatusCode.OK then
                return None
            else
                let! result = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                return JsonConvert.DeserializeObject<Model.LuisQuery>(result) |> Some
        }

