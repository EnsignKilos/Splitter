using System.Text;
using System.Text.RegularExpressions;

const int LinesPerFile = 10_000_000;
const int WriteBufferCapacity = 1_000_000;

if (args.Length != 4)
{
    Console.WriteLine("Usage: FileSplitter <folder_path> <regex_pattern> <output_path> <mode>");
    Console.WriteLine("  mode: 'include' - write lines that match the regex");
    Console.WriteLine("        'exclude' - write lines that don't match the regex");
    return;
}

string folderPath = args[0];
string pattern = args[1];
string outputFolder = args[2];
string mode = args[3].ToLowerInvariant();

if (mode != "include" && mode != "exclude")
{
    Console.WriteLine("Error: Mode must be either 'include' or 'exclude'");
    return;
}

bool includeMatches = mode == "include";

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

Directory.CreateDirectory(outputFolder);

var textFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly);

Console.WriteLine($"Mode: {(includeMatches ? "Including" : "Excluding")} lines that match the pattern");

for (int fileIndex = 0; fileIndex < textFiles.Length; fileIndex++)
{
    Console.Write($"\rProcessing {fileIndex + 1}/{textFiles.Length}: {Path.GetFileName(textFiles[fileIndex])} {(double)fileIndex / textFiles.Length:P0} Done          ");
    ProcessFile(textFiles[fileIndex], outputFolder, stringRegex, includeMatches);
}
Console.WriteLine();

static void ProcessFile(string inputFile, string outputFolder, Regex suppliedRegex, bool includeMatches)
{
    string baseFileName = Path.GetFileNameWithoutExtension(inputFile);
    int partNumber = 1;
    int currentLineCount = 0;
    long totalLinesProcessed = 0;
    long validLinesCount = 0;

    var writeBuffer = new StringBuilder();
    string outputFile = Path.Combine(outputFolder, $"{baseFileName}_part{partNumber:D4}.txt");

    StreamWriter? writer = null;

    try
    {
        using var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8, false);

        writer = new StreamWriter(outputFile, false, Encoding.UTF8);
        string? line;
        int bufferLineCount = 0;

        while ((line = reader.ReadLine()) != null)
        {
            totalLinesProcessed++;

            bool isMatch = suppliedRegex.IsMatch(line);
            
            if (includeMatches != isMatch)
                continue;

            validLinesCount++;
            writeBuffer.AppendLine(line);
            currentLineCount++;
            bufferLineCount++;

            bool needNewFile = currentLineCount >= LinesPerFile;
            bool bufferFull = bufferLineCount >= WriteBufferCapacity;

            if (bufferFull || needNewFile)
            {
                writer.Write(writeBuffer.ToString());
                writeBuffer.Clear();
                bufferLineCount = 0;

                if (needNewFile)
                {
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();

                    partNumber++;
                    outputFile = Path.Combine(outputFolder, $"{baseFileName}_part{partNumber:D4}.txt");
                    writer = new StreamWriter(outputFile, false, Encoding.UTF8);

                    currentLineCount = 0;

                    if (validLinesCount % 10_000_000 == 0)
                        Console.Write($"\r  {totalLinesProcessed:N0} lines processed, {validLinesCount:N0} valid lines          ");
                }
            }
        }

        if (writeBuffer.Length > 0)
        {
            writer.Write(writeBuffer.ToString());
        }
    }
    finally
    {
        writer?.Dispose();
    }

    Console.WriteLine($"\n  Completed: {totalLinesProcessed:N0} total lines, {validLinesCount:N0} valid lines written");
}