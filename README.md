# FileSplitter

A C# utility for splitting and filtering wordlists and dictionaries based on regex pattern matching. Purpose-built for processing multi-gigabyte password lists, dictionaries, and text files quickly. Automatically separates matching and non-matching lines into separate output directories, with optional file chunking control.

## Features

- **Dual output streams**: Simultaneously writes matches and non-matches to separate directories
- **Optional file chunking**: Choose between chunked output (default) or single files per input
- **Regex-based filtering**: Extract or exclude specific patterns from wordlists (length, character sets, formats)
- **Large file handling**: Efficiently processes multi-gigabyte dictionaries and wordlists
- **Automatic file splitting**: Optionally splits output into manageable chunks (10 million lines per file)
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
FileSplitter <folder_path> <regex_pattern> <output_path> [no-split]
```

### Parameters

- `folder_path`: Directory containing `.txt` files to process
- `regex_pattern`: Regular expression pattern to match lines
- `output_path`: Base directory where output folders will be created
- `no-split`: **Optional**. Prevents file chunking, outputs to single files instead

### Output Structure

The utility creates two subdirectories in your output path:
- `<output_path>/matches/` - Contains lines that match the regex pattern
- `<output_path>/non-matches/` - Contains lines that don't match the regex pattern

Both matching and non-matching lines are preserved, allowing you to:
- Keep matched patterns for targeted attacks
- Retain non-matches for different pattern extraction or verification

### Output Files

**With chunking (default):**
- **Matches**: `{original_filename}_matches_part{number}.txt`
- **Non-matches**: `{original_filename}_nonmatches_part{number}.txt`
- Each output file contains a maximum of 10 million lines
- Files are numbered sequentially with zero-padding (e.g., `rockyou_matches_part0001.txt`)

**Without chunking (no-split mode):**
- **Matches**: `{original_filename}_matches.txt`
- **Non-matches**: `{original_filename}_nonmatches.txt`
- All matching/non-matching lines in single files per input file

### Examples

```bash
# Default behaviour - splits into 10M line chunks
FileSplitter /path/to/wordlists "^.{8}$" /output/folder

# Single file output - no chunking
FileSplitter /path/to/wordlists "^.{8}$" /output/folder no-split

# Extract complex passwords, keep simple ones (with chunking)
FileSplitter ./dictionaries "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$" ./output

# Separate admin-prefixed entries without chunking
FileSplitter C:\wordlists "^admin" C:\output no-split

# Split numeric-only strings from mixed content (single files)
FileSplitter ./passwords "^\d+$" ./filtered no-split

# Extract passwords between 6-12 characters (chunked output)
FileSplitter /wordlists "^.{6,12}$" /processed

# Separate special character passwords (no chunking)
FileSplitter ./dictionaries "[!@#$%^&*()_+\-=\[\]{};':\"\\|,.<>\/?]" ./sorted no-split

# Extract email-format entries from combo lists
FileSplitter ./combo-lists "[\w\.-]+@[\w\.-]+\.\w+" ./emails no-split

# Separate hex strings (e.g., hashes) with chunking
FileSplitter ./hashes "^[a-fA-F0-9]+$" ./hex-sorted
```

## When to Use Chunking vs No-Split

### Use chunking (default) when:
- Processing massive wordlists (100M+ lines) that you will manually sift each file after (no massive loading times for big dictionaries)
- Output needs to be distributed across multiple processes, or you want to run multiple dictionaries or rules against them in hashcat etc
- Feeding output to tools that can handle multiple input files

### Use no-split mode when:
- Maintaining 1:1 file correspondence is important, or you like seeing how many bytes you nibble away
- Downstream tools expect single input files, and you want to combine and filter at the same time
- Processing moderate-sized wordlists, already at correct sizing
- Tracking which patterns came from which original files
- Simplifying file management in automated pipelines

## Performance Characteristics

- **Dual write buffers**: 1 million lines each for matches and non-matches
- **Lines per file**: 10 million (when chunking enabled)
- **Memory usage**: Minimal, uses streaming with dual buffered writes
- **Regex engine**: Compiled, culture-invariant, non-backtracking for optimal performance
- **Progress display**: Clean, non-verbose output showing file progress

## Technical Details

### Regex Options
- `Compiled`: Pre-compiles regex for faster execution
- `CultureInvariant`: Ensures consistent behaviour across locales
- `NonBacktracking`: To enforce not to backtrack in queries, for speed 

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

## Licence

Use it however you like - it's not exactly rocket surgery.

## Author

**EnsignKilos**  / 0xChikn
Security Researcher | Red Teamer | Penetration Tester

## Acknowledgements

Built for high-performance wordlist processing and dictionary management in penetration testing and security research workflows. Designed to preserve all data whilst enabling efficient pattern-based separation with flexible output options.

## Support

For issues, feature requests, or questions, please open an issue on GitHub.