using System.Text;
using Xunit;

namespace AElfScan.TokenApp;

public class InitBalanceTest
{
    private const string PrePath = "/Users/weihubin/Desktop/explore2_data/MainChain/";
    
    [Fact]
    public async Task HandleEvent_Test()
    {
        Process("tDVV");
        Process("AELF");
    }
    
    [Fact]
    public async Task InitBalance_Test()
    {
        //'tDVV' addressSet count: 41921
        //'AELF' addressSet count: 29891
        var provider = new InitialBalanceProvider();
        foreach (var (chainId, value) in provider._initialBalances)
        {
            HashSet<string> addressSet = new HashSet<string>();
            foreach (var pair in value)
            {
                foreach (var line in pair.Value)
                {
                    //Console.WriteLine($"line: {line}");
                    var initialBalance = line.Split(',');
                    var address = initialBalance[0];
                    var symbol = initialBalance[1];
                    var amount = long.Parse(initialBalance[2]);
                    addressSet.Add(line.Split(",")[0]);
                }
            }
            Console.WriteLine($"'{chainId}' count: {addressSet.Count}");
        }
    }
    
    
    public async Task Process(string chainId)
    {
        string directoryPath = PrePath + chainId;
        
        var aggregatedContents = AggregateContent(chainId, directoryPath);
        
        string outputFile = PrePath + chainId + ".txt";
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }
        WriteToFile(outputFile, aggregatedContents);
    }

    
    public List<string> AggregateContent(string chainId, string directoryPath, int batchSize = 10)
    {
        string[] fileEntries = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        List<string> results = new List<string>();
        List<string> currentBatch = new List<string>();
        int batchNumber = 1001;

        HashSet<string> addressSet = new HashSet<string>();
        foreach (string fileName in fileEntries)
        {
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                addressSet.Add(line.Split(",")[0]);
                currentBatch.Add(line);
                if (currentBatch.Count == batchSize)
                {
                    var num = batchNumber++;
                    /*if (num == 1535)
                    {
                         Console.WriteLine($"'{num}' fileName: {fileName}");
                    }*/
                    string aggregatedContent = FormatBatchContent(num, currentBatch);
                    results.Add(aggregatedContent);
                    currentBatch.Clear();
                }
            }
        }

        Console.WriteLine($"'{chainId}' addressSet count: {addressSet.Count}");
        // Handle any remaining lines
        if (currentBatch.Count > 0)
        {
            string aggregatedContent = FormatBatchContent(batchNumber, currentBatch);
            results.Add(aggregatedContent);
        }

        return results;
    }

    private string FormatBatchContent(int batchNumber, List<string> batchContent)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("    {");
        sb.AppendLine($"        {batchNumber}, new List<string>");
        sb.AppendLine("        {");
        sb.Append(string.Join(",\n", batchContent.Select(line => $"            \"{line}\"")));
        sb.AppendLine();
        sb.AppendLine("        }");
        sb.AppendLine("    },");
        return sb.ToString();
    }

    public void WriteToFile(string filePath, List<string> content)
    {
        File.WriteAllText(filePath, "[\n" + string.Join("\n", content) + "\n]");
    }
}