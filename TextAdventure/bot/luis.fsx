#I "../../packages/Newtonsoft.Json/lib/netstandard2.0"
#I "../../packages/NETStandard.Library/build/netstandard2.0/ref"
#I "../../packages/System.Net.Http/ref/netstandard1.3"
#r "Newtonsoft.Json"
#r "netstandard"
#r "System.Net.Http"

//open System.Threading.Tasks
open System.Net.Http

let appId = ""
let appKey = ""
let region = ""

//**

(***
https://[location].api.cognitive.microsoft.com/luis/v2.0/apps/{appId}?q={q}[&timezoneOffset][&verbose][&spellCheck][&staging][&bing-spell-check-subscription-key][&log]
 ***)


// let awaitTask (t: Task) = t.ContinueWith (fun t -> ()) |> Async.AwaitTask

let getResponse query = 
    async {
        let client = new HttpClient()
        let url =
            sprintf "https://%s.api.cognitive.microsoft.com/luis/v2.0/apps/%s?q=%s"
                region
                appId
                query

        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", appKey)
        let! response = client.GetAsync(url) |> Async.AwaitTask

        let! result = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return result
    }



(** Movement Tests **)
getResponse "go north" |> Async.RunSynchronously
getResponse "go northeast" |> Async.RunSynchronously
getResponse "walk west" |> Async.RunSynchronously


getResponse "climb up the tree" |> Async.RunSynchronously
getResponse "climb down the stairs" |> Async.RunSynchronously




getResponse "unlock the door with the rusty key" |> Async.RunSynchronously

