using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using epu;

class Program
{
    static void Main()
    {
        string inputFileName = "SCUANAGRAFESTAT20232420230901.csv"; // Nome del file CSV di input
        string outputFileName = "ScuoleCheUsanoENonUsano.csv"; // Nome del file di output CSV
        string percorsoFile = "ScuolEpuration.csv";
        string inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), inputFileName);
        string inputFileEpuPath = Path.Combine(Directory.GetCurrentDirectory(), percorsoFile);
        string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
        int nScuole;
        List<string> risultatiDomainChecker = new List<string>();
        try
        {
            //controlla se esiste già il file "ScuolEpuration", in tal caso non viene eseguito l'if
            if (!File.Exists(percorsoFile)){
                Epuration ist= new Epuration();
                 ist.RemoveDuplicateRows(inputFileName);
            }

            var lines = File.ReadLines(inputFileEpuPath).Skip(25000);//.Take(5)
            nScuole=lines.Count();
            int i=0;
            string lowerLine;
            foreach (string line in lines)
            {
                if(i%1000==0)EseguiComandoFlushDns();
                i++;
                Console.Write($"Esecuzione in corso: {i}/{nScuole}  |   Dominio analizzato: {line}                        \r");
                
                
                //effettuare controllo di ridondanza del nuovo file csv
                if (line.Contains("Non Disponibile"))
                {
                    risultatiDomainChecker.Add("Non Disponibile;ND;ND;ND");
            
                }
                else if(line.Contains("Doppione"))
                {
                    risultatiDomainChecker.Add($"{line};Doppione;Doppione;Doppione");
                }
                else
                {
                    lowerLine=line.ToLower(); 

                    if(lowerLine.Contains("www."))
                    {
                        lowerLine=lowerLine.Replace("www.", "");  
                    }
                    
                    if(lowerLine.Contains("w.w.w."))
                    {
                        lowerLine=lowerLine.Replace("w.w.w.", "");  
                    }
                    if(lowerLine.Contains("https"))
                    {
                        lowerLine=lowerLine.Replace("https", "");  
                    }
                    if(lowerLine.Contains("http"))
                    {
                        lowerLine=lowerLine.Replace("http", "");  
                    }
                    if(lowerLine.Contains("/"))
                    {
                        lowerLine=lowerLine.Replace("/", "");  
                    }
                    string[] output=DomainChecker(lowerLine);    
                    risultatiDomainChecker.Add($"{output[0]};{output[1]};{output[2]};{output[3]}");
            
                }
                
            }
            using (var writer = new StreamWriter(outputFilePath))
            {    
                writer.WriteLine("DominioScuola;StatoDominio;Output;Google_Or_Microsoft");
                foreach (var risultato in risultatiDomainChecker)
                {
                    writer.WriteLine(risultato);
                }
                writer.Close();
                Console.WriteLine("\nSalvataggio completato");
            }
            Console.WriteLine($"Percorso file: {outputFilePath}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Si è verificato un errore durante la lettura/scrittura del file: {ex.Message}");
        }
        
    }

    static string[] DomainChecker(string input)
    {
        string[] result = new string[4];
        string error;
        string output;
        var i=0;

        //Console.WriteLine("Esecuzione di txt e mx");

        var comando = EseguiComandoNslookup(input, "txt");
        error= comando.Error;
        output = comando.Output.Replace(Environment.NewLine, " ");

        comando = EseguiComandoNslookup(input, "mx");
        error= error + comando.Error;
        output= output + comando.Output;
        output = output.Replace(Environment.NewLine, " ");
           
        do
        {
            if(error.Contains("Non-existent") == true|output.Contains("timed out") == true)
                {
                    input=input.Replace("gov", "edu");
                    
                    if(i==-1)
                    {
                        result[0]=input;
                        result[2]="Er";
                        result[3]="Er";
                    }
                    i--;

                    comando = EseguiComandoNslookup(input, "txt");
                    error = comando.Error;
                    output = comando.Output.Replace(Environment.NewLine, " ");

                    comando = EseguiComandoNslookup(input, "mx");
                    error = error + comando.Error;
                    output = output + comando.Output;
                    output = output.Replace(Environment.NewLine, " ");

                }
                else
                {
                    if( output.Contains("GOOGLE") | output.Contains("google") |  output.Contains("MS=") | output.Contains("outlook"))
                    {
                        i++;
                        result[0]=input;
                        result[2]=output;
                        result[3]="USA";

                
                    }
                    else
                    {
                        i++;
                        result[0]=input;
                        result[2]=output;
                        result[3]="NON USA";
                    }
                }
            
        }while(i>-2 & i<0);
        
        switch(i)
        {
            case 0: result[1]="Modificato"; break;
            case 1: result[1]="Corretto"; break;
            case -2: result[1]="Errato"; break;
            default: result[1]="Errore nello switch";break;
        }
        return result;
    }

    static (string Output, string Error) EseguiComandoNslookup(string dominio, string tipoRecord)
    {
        string output = string.Empty;
        string error = string.Empty;

        try
        {
            string comando = $"nslookup -q={tipoRecord} {dominio} | findstr /V /C:\"Address:\" /C:\"Server:\"";

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {comando}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
            process.Start();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            error = $"Errore durante l'esecuzione di nslookup -q={tipoRecord} {dominio}: {ex.Message}";
            output = "Exception";
        }

        return (Output: output, Error: error);
    }


    static void EseguiComandoFlushDns()
    {
        try
        {
            // Esegui il comando ipconfig /flushdns
            var processo = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            processo.Start();
            processo.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore durante l'esecuzione di ipconfig /flushdns: {ex.Message}");
            // Gestisci eventuali errori
        }
    }

}


