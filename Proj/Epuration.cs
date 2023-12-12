using System;
using System.Collections.Generic;
using System.IO;

 //PSEUDOCODICE
      //lettura file
      //ciclo che scannerizza tutte le righe del file .csv
      //controllo (all'interno del ciclo) per ogni riga del numero di occorrenze di "DENOMINAZIONEISTITUTORIFERIMENTO" maggiore ad 1
      //in caso di esito positivo viene lasciata la prima occorrenza e tutte le altre vengono eliminate

namespace epu{
public class Epuration 
{
        // Percorso del file CSV
        // Leggi il file e rimuovi le righe duplicate
    
    public void RemoveDuplicateRows(string filePath)
    {
        string outputfilePath2 = "ScuolEpuration.csv";
        
        try
        {
            // Leggi tutte le righe dal file
            string[] lines = File.ReadAllLines(filePath);

            // Creazione di un insieme per tenere traccia delle scuole già incontrate
            HashSet<string> DominiDup = new HashSet<string>();

            // Lista per tenere traccia delle righe non duplicate
            List<string> uniqueRows = new List<string>();

            // Ciclo attraverso tutte le righe del file
            foreach (string line in lines)
            {
                // Split della riga per ottenere i singoli campi
                string[] fields = line.Split(',');

                // Controllo se il campo "SITOWEB" è già stato incontrato
                if (fields.Length > 18 && DominiDup.Add(fields[18]))
                {
                    // Stampa della colonna "SITOWEB" nel nuovo file "ScuolEpuration.csv"
                    uniqueRows.Add(fields[18]);
                }
                else if(!fields[18].Contains("Non Disponibile"))
                {
                    uniqueRows.Add("Doppione");
                }
                else
                {
                    uniqueRows.Add("Non Disponibile");
                }
            }
            // Sovrascrivi il file con le righe non duplicate
            File.WriteAllLines(outputfilePath2, uniqueRows);
            Console.WriteLine($"Operazione completata, toto righe: {uniqueRows.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Si è verificato un errore: {ex.Message}");
        }
    }
}
}