Progetto Reti 
Il progetto è stato sviluppato in C# su VSCode, richiede l'installazione di DSK .NET e delle estensioni C# Dev Kit.
Una volta aperto il progetto, dal terminale, entrare nella cartella di progetto e inserire il seguente comando:"dotnet run".
Per evitare di analizzare tutte e 51081 istanze del file del MIUR, aggiungere ".Take(n)" alla riga 27 "var lines = File.ReadLines(inputFileEpuPath).Skip(1);" in "Program.cs"
Il file di output che viene sovrascritto ogni esecuzione è "ScuoleCheUsanoENonUsano.csv", il file di input che contiene le anagrafiche di tutte le scuole italiane è "SCUANAGRAFESTAT20232420230901.csv", il file "ScuolEpuration.csv" è un file utilizzato per l'elaborazione dell'output.
