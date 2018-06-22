#I "../../packages/Newtonsoft.Json/lib/netstandard2.0"
#I "../../packages/NETStandard.Library/build/netstandard2.0/ref"
#r "netstandard"
#r "Newtonsoft.Json"

open Newtonsoft.Json

// question, what does a recordtype with a function look like when serialized?
type Myrecord = {
     Name: string
     UpdateFunction: Myrecord -> Myrecord
}

let value = { Name = "try to serialize this"; UpdateFunction = (fun mr -> {mr with Name = "serialized!" }) }

let json = JsonConvert.SerializeObject(value)

// throws exception: Type is an interface or abstract class and cannot be instantiated. Path 'UpdateFunction'
let value' = JsonConvert.DeserializeObject<Myrecord>(json)


