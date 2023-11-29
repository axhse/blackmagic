module FileReader
open System.IO

let dropCarriageReturns text =
    let rec drop result restSymbols =
        match restSymbols with
        | [] -> result
        | '\r'::rest -> drop result rest
        | symbol::rest -> drop (result + string symbol) rest
    drop "" (Seq.toList text)

let readFileText (filePath: string) = dropCarriageReturns (File.ReadAllText(filePath))
