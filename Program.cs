using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

const int LinesPerFile = 10_000_000;
const int WriteBufferCapacity = 1_000_000;

if (args.Length < 3 || args.Length > 4)
{
    Console.WriteLine("Usage: FileSplitter <folder_path> <regex_pattern> <output_path> [no-split]");
    Console.WriteLine("  Matching lines will be written to: <output_path>/matches/");
    Console.WriteLine("  Non-matching lines will be written to: <output_path>/non-matches/");
    Console.WriteLine("  no-split: Optional. Add 'no-split' to keep all output in single files (no chunking)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  FileSplitter /wordlists \"^.{8}$\" /output");
    Console.WriteLine("  FileSplitter /wordlists \"^.{8}$\" /output no-split");
    return;
}

string folderPath = args[0];
string pattern = args[1];
string outputFolder = args[2];
bool splitFiles = true;

// Check for no-split flag
if (args.Length == 4)
{
    if (args[3].ToLowerInvariant() == "no-split")
    {
        splitFiles = false;
        Console.WriteLine("File splitting disabled - outputting to single files");
    }
    else
    {
        Console.WriteLine($"Warning: Unknown parameter '{args[3]}' ignored. Use 'no-split' to disable file chunking.");
    }
}

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
    RegexOptions.CultureInvariant);
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

if (textFiles.Length == 0)
{
    Console.WriteLine($"Error: No .txt files found in {folderPath}");
    return;
}

Console.WriteLine($"Found {textFiles.Length} .txt file(s) to process");
Console.WriteLine($"Processing files - matches go to: {matchesFolder}");
Console.WriteLine($"                   non-matches go to: {nonMatchesFolder}");
Console.WriteLine($"Chunking: {(splitFiles ? $"Enabled ({LinesPerFile:N0} lines per file)" : "Disabled (single output files)")}");
Console.WriteLine();

for (int fileIndex = 0; fileIndex < textFiles.Length; fileIndex++)
{
    string fileName = Path.GetFileName(textFiles[fileIndex]);
    string displayName = fileName;
    if (displayName.Length > 40) displayName = string.Concat("...", fileName.AsSpan(fileName.Length - 37));
    
    Console.WriteLine($"Processing {fileIndex + 1}/{textFiles.Length}: {displayName}");
    
    ProcessFile(textFiles[fileIndex], matchesFolder, nonMatchesFolder, stringRegex, splitFiles);
}

Console.WriteLine("\nAll files processed.");

static void ProcessFile(string inputFile, string matchesFolder, string nonMatchesFolder, Regex suppliedRegex, bool splitFiles)
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
    
    // Speed tracking
    var stopwatch = Stopwatch.StartNew();
    
    string matchOutputFile;
    string nonMatchOutputFile;
    
    // Determine output file naming
    if (splitFiles)
    {
        matchOutputFile = Path.Combine(matchesFolder, $"{baseFileName}_matches_part{matchPartNumber:D4}.txt");
        nonMatchOutputFile = Path.Combine(nonMatchesFolder, $"{baseFileName}_nonmatches_part{nonMatchPartNumber:D4}.txt");
    }
    else
    {
        matchOutputFile = Path.Combine(matchesFolder, $"{baseFileName}_matches.txt");
        nonMatchOutputFile = Path.Combine(nonMatchesFolder, $"{baseFileName}_nonmatches.txt");
    }

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

                bool needNewFile = splitFiles && matchLineCount >= LinesPerFile;
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

                bool needNewFile = splitFiles && nonMatchLineCount >= LinesPerFile;
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
            
            // Show progress every 500,000 lines
            if (totalLinesProcessed % 500_000 == 0 && totalLinesProcessed > 0)
            {
                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                double linesPerSecond = totalLinesProcessed / elapsedSeconds;
                
                Console.Write($"\r  {linesPerSecond:N0} lines/sec | {totalMatches:N0} matches, {totalNonMatches:N0} non-matches          ");
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
        
        // Final statistics
        Console.Write($"\r  Complete: {totalLinesProcessed:N0} lines | {totalMatches:N0} matches, {totalNonMatches:N0} non-matches          ");
        Console.WriteLine();
        if (splitFiles && (matchPartNumber > 1 || nonMatchPartNumber > 1))
        {
            Console.WriteLine($"  Files created: {matchPartNumber} match part(s), {nonMatchPartNumber} non-match part(s)");
        }
    }
    finally
    {
        matchWriter?.Flush();
        nonMatchWriter?.Flush();
        matchWriter?.Dispose();
        nonMatchWriter?.Dispose();
    }
}