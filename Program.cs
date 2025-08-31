using System.Text;
using System.Text.RegularExpressions;

const int LinesPerFile = 10_000_000;
const int WriteBufferCapacity = 1_000_000;

if (args.Length != 3)
{
    Console.WriteLine("Usage: FileSplitter <folder_path> <regex_pattern> <output_path>");
    Console.WriteLine("  Matching lines will be written to: <output_path>/matches/");
    Console.WriteLine("  Non-matching lines will be written to: <output_path>/non-matches/");
    return;
}

string folderPath = args[0];
string pattern = args[1];
string outputFolder = args[2];

if (!Directory.Exists(folderPath))
{
    Console.WriteLine($"Folder not found: {folderPath}");
    return;
}

Regex stringRegex;
try
{
    stringRegex = new Regex(pattern,
    RegexOptions.Compiled |
    RegexOptions.CultureInvariant | 
    RegexOptions.NonBacktracking);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: Invalid regex pattern - {ex.Message}");
    return;
}

// Create both output directories
string matchesFolder = Path.Combine(outputFolder, "matches");
string nonMatchesFolder = Path.Combine(outputFolder, "non-matches");
Directory.CreateDirectory(matchesFolder);
Directory.CreateDirectory(nonMatchesFolder);

var textFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly);

Console.WriteLine($"Processing files - matches go to: {matchesFolder}");
Console.WriteLine($"                   non-matches go to: {nonMatchesFolder}");

for (int fileIndex = 0; fileIndex < textFiles.Length; fileIndex++)
{
    string fileName = Path.GetFileName(textFiles[fileIndex]);
    if (fileName.Length > 40) fileName = string.Concat("...", fileName.AsSpan(fileName.Length - 37));
    Console.Write($"\rProcessing {fileIndex + 1}/{textFiles.Length}: {fileName,-40} {(double)(fileIndex + 1) / textFiles.Length:P0}");
    ProcessFile(textFiles[fileIndex], matchesFolder, nonMatchesFolder, stringRegex);
}
Console.WriteLine("\n\nAll files processed.");

static void ProcessFile(string inputFile, string matchesFolder, string nonMatchesFolder, Regex suppliedRegex)
{
    string baseFileName = Path.GetFileNameWithoutExtension(inputFile);
    
    // Separate tracking for matches and non-matches
    int matchPartNumber = 1;
    int nonMatchPartNumber = 1;
    int matchLineCount = 0;
    int nonMatchLineCount = 0;
    long totalLinesProcessed = 0;
    long totalMatches = 0;
    long totalNonMatches = 0;

    var matchBuffer = new StringBuilder();
    var nonMatchBuffer = new StringBuilder();
    
    string matchOutputFile = Path.Combine(matchesFolder, $"{baseFileName}_matches_part{matchPartNumber:D4}.txt");
    string nonMatchOutputFile = Path.Combine(nonMatchesFolder, $"{baseFileName}_nonmatches_part{nonMatchPartNumber:D4}.txt");

    StreamWriter? matchWriter = null;
    StreamWriter? nonMatchWriter = null;

    try
    {
        using var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8, false);

        matchWriter = new StreamWriter(matchOutputFile, false, Encoding.UTF8);
        nonMatchWriter = new StreamWriter(nonMatchOutputFile, false, Encoding.UTF8);
        
        string? line;
        int matchBufferLineCount = 0;
        int nonMatchBufferLineCount = 0;

        while ((line = reader.ReadLine()) != null)
        {
            totalLinesProcessed++;
            bool isMatch = suppliedRegex.IsMatch(line);
            
            if (isMatch)
            {
                totalMatches++;
                matchBuffer.AppendLine(line);
                matchLineCount++;
                matchBufferLineCount++;

                bool needNewFile = matchLineCount >= LinesPerFile;
                bool bufferFull = matchBufferLineCount >= WriteBufferCapacity;

                if (bufferFull || needNewFile)
                {
                    matchWriter.Write(matchBuffer.ToString());
                    matchBuffer.Clear();
                    matchBufferLineCount = 0;

                    if (needNewFile)
                    {
                        matchWriter.Flush();
                        matchWriter.Close();
                        matchWriter.Dispose();

                        matchPartNumber++;
                        matchOutputFile = Path.Combine(matchesFolder, $"{baseFileName}_matches_part{matchPartNumber:D4}.txt");
                        matchWriter = new StreamWriter(matchOutputFile, false, Encoding.UTF8);

                        matchLineCount = 0;
                    }
                }
            }
            else
            {
                totalNonMatches++;
                nonMatchBuffer.AppendLine(line);
                nonMatchLineCount++;
                nonMatchBufferLineCount++;

                bool needNewFile = nonMatchLineCount >= LinesPerFile;
                bool bufferFull = nonMatchBufferLineCount >= WriteBufferCapacity;

                if (bufferFull || needNewFile)
                {
                    nonMatchWriter.Write(nonMatchBuffer.ToString());
                    nonMatchBuffer.Clear();
                    nonMatchBufferLineCount = 0;

                    if (needNewFile)
                    {
                        nonMatchWriter.Flush();
                        nonMatchWriter.Close();
                        nonMatchWriter.Dispose();

                        nonMatchPartNumber++;
                        nonMatchOutputFile = Path.Combine(nonMatchesFolder, $"{baseFileName}_nonmatches_part{nonMatchPartNumber:D4}.txt");
                        nonMatchWriter = new StreamWriter(nonMatchOutputFile, false, Encoding.UTF8);

                        nonMatchLineCount = 0;
                    }
                }
            }
        }

        // Write any remaining buffered content
        if (matchBuffer.Length > 0)
        {
            matchWriter.Write(matchBuffer.ToString());
        }
        if (nonMatchBuffer.Length > 0)
        {
            nonMatchWriter.Write(nonMatchBuffer.ToString());
        }
    }
    finally
    {
        matchWriter?.Dispose();
        nonMatchWriter?.Dispose();
    }
}