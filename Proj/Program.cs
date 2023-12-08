using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        string inputFileName = "SCUANAGRAFESTAT20232420230901.csv"; // Nome del file CSV di input
        string outputFileName = "ScuoleCheUsanoENonUsano.csv"; // Nome del file di output CSV
        string inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), inputFileName);
        string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
        int columnIndex = 18; 
        int nScuole;

        try
        {
            var lines = File.ReadLines(inputFilePath).Skip(1).Take(50);//.Take(5)
            nScuole=lines.Count();
            int i=0;
            using (var writer = new StreamWriter(outputFilePath))
            {
                writer.WriteLine("DominioScuola;StatoDominio;Output;Google_Or_Microsoft");

                foreach (string line in lines)
                {
                    if(i%100==0)EseguiComandoFlushDns();
                    i++;
                    Console.WriteLine($"Esecuzione in corso: {i}/{nScuole}");
                    
                    string[] columns = line.Split(',');
                    //effettuare controllo di ridondanza del nuovo file csv
                    if (columns[columnIndex].Contains("Non Disponibile"))
                    {
                        writer.WriteLine("Non Disponibile;ND;ND;ND");
                
                    }
                    else /*if(columns[columnIndex].Contains("www.") | columns[columnIndex].Contains("https//") | columns[columnIndex].Contains("http//") | columns[columnIndex].Contains("/"))*/
                    {
                        columns[columnIndex]=columns[columnIndex].ToLower();  

                        if(columns[columnIndex].Contains("www."))
                        {
                            columns[columnIndex]=columns[columnIndex].Replace("www.", "");  
                        }
                        
                        if(columns[columnIndex].Contains("w.w.w."))
                        {
                            columns[columnIndex]=columns[columnIndex].Replace("w.w.w.", "");  
                        }
                        if(columns[columnIndex].Contains("https"))
                        {
                            columns[columnIndex]=columns[columnIndex].Replace("https", "");  
                        }
                        if(columns[columnIndex].Contains("http"))
                        {
                            columns[columnIndex]=columns[columnIndex].Replace("http", "");  
                        }
                        if(columns[columnIndex].Contains("/"))
                        {
                            columns[columnIndex]=columns[columnIndex].Replace("/", "");  
                        }
                        string[] output=DomainChecker(columns[columnIndex]);
                        writer.WriteLine($"{output[0]};{output[1]};{output[2]};{output[3]}");

                    }
                    
                }
                writer.Close();
            }
            Console.WriteLine($"Il contenuto delle colonne è stato salvato nel file: {outputFilePath}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Si è verificato un errore durante la lettura/scrittura del file: {ex.Message}");
        }
        
    }

    static string[] DomainChecker(string input)
    {
        string[] result = new string[5];
        
        string error;
        string output;

        var comando = EseguiComandoNslookup(input, "all");
        if(comando.Output.Length<150 & comando.Output.Length>50 | comando.Error.Contains("timed out"))
        {   
            Console.WriteLine("Esecuzione di txt e mx");
            comando = EseguiComandoNslookup(input, "txt");
            error= comando.Error;
            output = comando.Output.Replace(Environment.NewLine, " ");

            comando = EseguiComandoNslookup(input, "mx");
            error= error + comando.Error;
            output= output + comando.Output;
            output = output.Replace(Environment.NewLine, " ");
        }
        else
        {
            error= comando.Error;
            output = comando.Output.Replace(Environment.NewLine, " ");
        }
                
        var i=0;
        do
        {
            if(error.Contains("Non-existent") == true|output.Contains("timed out") == true)
                {
                    input=input.Replace("gov", "edu");
                    
                    if(i==-1)
                    {
                        result[0]=input;
                        result[2]="ND";
                        result[3]="ND";
                    }
                    i--;

                    comando = EseguiComandoNslookup(input, "all");
                    if(comando.Output.Length<150 & comando.Output.Length>50 | comando.Error.Contains("timed out"))
                    {   
                        Console.WriteLine("Esecuzione di txt e mx");
                        comando = EseguiComandoNslookup(input, "txt");
                        error= comando.Error;
                        output = comando.Output.Replace(Environment.NewLine, " ");

                        comando = EseguiComandoNslookup(input, "mx");
                        error= error + comando.Error;
                        output= output + comando.Output;
                        output = output.Replace(Environment.NewLine, " ");
                    }
                    else
                    {
                        error= comando.Error;
                        output = comando.Output.Replace(Environment.NewLine, " ");
                    }
                    

                }
                else
                {
                    if( output.Contains("ASPMX.L.GOOGLE.COM") | output.Contains("google-site-verification") | output.Contains("spf.google") |  output.Contains("MS=") | output.Contains("outlook.com") | output.Contains("spf.protection.outlook"))
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

            var process = new Process
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
            };

            process.Start();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            error = $"Errore durante l'esecuzione di nslookup -q={tipoRecord} {dominio}: {ex.Message}";
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


