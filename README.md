# FileSplitter

A high-performance C# utility for splitting massive wordlists and dictionaries based on regex pattern matching. Purpose-built for processing multi-gigabyte password lists, dictionaries, and text corpora with minimal memory footprint. Automatically separates matching and non-matching lines into separate output directories.

## Features

- **Dual output streams**: Simultaneously writes matches and non-matches to separate directories
- **Regex-based filtering**: Extract or exclude specific patterns from wordlists (length, character sets, formats)
- **Large file handling**: Efficiently processes multi-gigabyte dictionaries and wordlists
- **Automatic file splitting**: Splits output into manageable chunks (10 million lines per file)
- **Memory optimised**: Uses dual buffered writing with 1 million line buffer capacity per stream
- **Progress tracking**: Real-time processing statistics with filename truncation for better display
- **Batch processing**: Processes all `.txt` files in a directory automatically

## Requirements

- .NET 9.0 or later
- Windows, Linux, or macOS

## Installation

### Build from source
```bash
git clone https://github.com/EnsignKilos/FileSplitter.git
cd FileSplitter
dotnet build -c Release
```

### Or create a self-contained executable
```bash
dotnet publish -c Release -r win-x64 --self-contained
# For Linux: -r linux-x64
# For macOS: -r osx-x64
```

## Usage

```bash
FileSplitter <folder_path> <regex_pattern> <output_path>
```

### Parameters

- `folder_path`: Directory containing `.txt` files to process
- `regex_pattern`: Regular expression pattern to match lines
- `output_path`: Base directory where output folders will be created

### Output Structure

The utility creates two subdirectories in your output path:
- `<output_path>/matches/` - Contains lines that match the regex pattern
- `<output_path>/non-matches/` - Contains lines that don't match the regex pattern

Both matching and non-matching lines are preserved, allowing you to:
- Keep matched patterns for targeted attacks
- Retain non-matches for different pattern extraction or verification

### Examples

```bash
# Separate 8-character passwords from others
FileSplitter /path/to/wordlists "^.{8}$" /output/folder

# Split passwords with uppercase, lowercase, and numbers from those without
FileSplitter ./dictionaries "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$" ./output

# Extract words starting with 'admin' (rest go to non-matches)
FileSplitter C:\wordlists "^admin" C:\output

# Separate numeric-only strings from mixed content
FileSplitter ./passwords "^\d+$" ./filtered

# Split passwords between 6-12 characters from others
FileSplitter /wordlists "^.{6,12}$" /processed

# Separate entries containing special characters
FileSplitter ./dictionaries "[!@#$%^&*()_+\-=\[\]{};':\"\\|,.<>\/?]" ./sorted

# Extract email-format entries (combo lists)
FileSplitter ./combo-lists "[\w\.-]+@[\w\.-]+\.\w+" ./emails

# Separate hex strings (e.g., hashes) from non-hex content
FileSplitter ./hashes "^[a-fA-F0-9]+$" ./hex-sorted
```

## Output Files

- **Matches**: `{original_filename}_matches_part{number}.txt`
- **Non-matches**: `{original_filename}_nonmatches_part{number}.txt`
- Each output file contains a maximum of 10 million lines
- Files are numbered sequentially with zero-padding (e.g., `rockyou_matches_part0001.txt`)

## Performance Characteristics

- **Dual write buffers**: 1 million lines each for matches and non-matches
- **Lines per file**: 10 million (configurable in source)
- **Memory usage**: Minimal, uses streaming with dual buffered writes
- **Regex engine**: Compiled, culture-invariant, non-backtracking for optimal performance
- **Progress display**: Truncates long filenames for cleaner output

## Technical Details

### Regex Options
- `Compiled`: Pre-compiles regex for faster execution
- `CultureInvariant`: Ensures consistent behaviour across locales
- `NonBacktracking`: Prevents catastrophic backtracking on complex patterns

### File Processing
- Streams input files to minimise memory usage
- Dual buffered writing for both output streams
- UTF-8 encoding for input and output
- Automatic resource disposal with proper exception handling
- Simultaneous writing to both match and non-match streams

## Use Cases

- **Password Complexity Analysis**: Separate passwords by complexity patterns whilst retaining simpler ones
- **Dictionary Stratification**: Organise wordlists by characteristics for phased attacks
- **Hashcat/John Preparation**: Create both targeted and exclusion wordlists simultaneously
- **Combo List Processing**: Separate valid email:password formats from malformed entries
- **Compliance Testing**: Split wordlists into compliant and non-compliant sets
- **Pattern Validation**: Verify regex patterns by reviewing both matches and non-matches
- **Data Quality Control**: Identify and separate malformed entries from valid data
- **Multi-stage Processing**: Use non-matches from one pattern as input for different patterns

## Error Handling

The utility includes comprehensive error handling for:
- Invalid folder paths
- Malformed regex patterns
- File access permissions
- Disk space issues
- Buffer overflow protection

## Building and Contributing

### Development Setup
```bash
git clone https://github.com/EnsignKilos/FileSplitter.git
cd FileSplitter
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

## Licence

Use it however you like - it's not exactly rocket surgery.

## Author

**EnsignKilos**  
Security Researcher | Red Teamer | Penetration Tester

## Acknowledgements

Built for high-performance wordlist processing and dictionary management in penetration testing and security research workflows. Designed to preserve all data whilst enabling efficient pattern-based separation.

## Support

For issues, feature requests, or questions, please open an issue on GitHub.