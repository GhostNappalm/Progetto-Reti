using System;
using System.Collections.Generic;
using System.IO;

 //PSEUDOCODICE
      //lettura file
      //ciclo che scannerizza tutte le righe del file .csv
      //controllo (all'interno del ciclo) per ogni riga del numero di occorrenze di "DENOMINAZIONEISTITUTORIFERIMENTO" maggiore ad 1
      //in caso di esito positivo viene lasciata la prima occorrenza e tutte le altre vengono eliminate


namespace CsvProcessing
{
class Program 
{
    // Percorso del file CSV
    string filePath = "ProvaEpuration.csv";

    static void RemoveDuplicateRows(string filePath)
    {
        // Leggi il file e rimuovi le righe duplicate
        RemoveDuplicateRows(filePath);

        Console.WriteLine("Operazione completata.");
        try
        {
            // Leggi tutte le righe dal file
            string[] lines = File.ReadAllLines(filePath);

            // Creazione di un insieme per tenere traccia delle città già incontrate
            HashSet<string> citiesSet = new HashSet<string>();

            // Lista per tenere traccia delle righe non duplicate
            List<string> uniqueRows = new List<string>();

            // Ciclo attraverso tutte le righe del file
            foreach (string line in lines)
            {
                // Split della riga per ottenere i singoli campi
                string[] fields = line.Split(',');

                // Controllo se il campo "Città" è già stato incontrato
                if (fields.Length > 3 && citiesSet.Add(fields[3]))
                {
                    // Se è la prima occorrenza, aggiungi la riga alla lista
                    uniqueRows.Add(line);
                }
            }

            // Sovrascrivi il file con le righe non duplicate
            File.WriteAllLines(filePath, uniqueRows);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Si è verificato un errore: {ex.Message}");
        }
    }
}

}