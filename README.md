(AI SLOP AHEAD):

# FileSplitter

A high-performance C# utility for splitting massive wordlists and dictionaries based on regex pattern matching. Purpose-built for processing multi-gigabyte password lists, dictionaries, and text corpora with minimal memory footprint.

## Features

- **Regex-based filtering**: Extract specific patterns from wordlists (length, character sets, formats)
- **Large file handling**: Efficiently processes multi-gigabyte dictionaries and wordlists
- **Automatic file splitting**: Splits output into manageable chunks (10 million lines per file)
- **Memory optimised**: Uses buffered writing with 1 million line buffer capacity
- **Progress tracking**: Real-time processing statistics and completion percentage (in a mega basic way)
- **Batch processing**: Processes all `.txt` files in a directory automatically

## Requirements

- .NET 8.0 or later
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
FileSplitter <folder_path> <regex_pattern>
```

### Parameters

- `folder_path`: Directory containing `.txt` files to process
- `regex_pattern`: Regular expression pattern to match lines for inclusion

### Examples

```bash
# Extract 8-character passwords only
FileSplitter /path/to/wordlists "^.{8}$"

# Extract passwords with uppercase, lowercase, and numbers
FileSplitter ./dictionaries "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"

# Extract words starting with specific prefix
FileSplitter C:\wordlists "^admin"

# Extract numeric-only strings
FileSplitter ./passwords "^\d+$"

# Extract passwords between 6-12 characters
FileSplitter /wordlists "^.{6,12}$"

# Extract entries containing special characters
FileSplitter ./dictionaries "[!@#$%^&*()_+\-=\[\]{};':\"\\|,.<>\/?]"

# Extract email-format entries
FileSplitter ./combo-lists "[\w\.-]+@[\w\.-]+\.\w+"

# Extract hex strings (e.g., hashes)
FileSplitter ./hashes "^[a-fA-F0-9]+$"
```

## Output

- Creates a `split_output` directory within the input folder
- Output files are named: `{original_filename}_part{number}.txt`
- Each output file contains a maximum of 10 million matching lines
- Files are numbered sequentially (e.g., `access_log_part0001.txt`, `access_log_part0002.txt`)

## Performance Characteristics

- **Write buffer**: 1 million lines (reduces I/O operations)
- **Lines per file**: 10 million (configurable in source)
- **Memory usage**: Minimal, uses streaming and buffered writes
- **Regex engine**: Compiled, culture-invariant, non-backtracking for optimal performance

## Technical Details

### Regex Options
- `Compiled`: Pre-compiles regex for faster execution
- `CultureInvariant`: Ensures consistent behaviour across locales
- `NonBacktracking`: Prevents catastrophic backtracking on complex patterns

### File Processing
- Streams input files to minimise memory usage
- Buffered writing reduces disk I/O operations
- UTF-8 encoding for input and output
- Automatic resource disposal with proper exception handling

## Use Cases

- **Password Analysis**: Split large password dumps by complexity patterns
- **Dictionary Management**: Organise wordlists by length, character sets, or patterns
- **Hashcat/John Preparation**: Create targeted wordlists for specific attack patterns
- **Combo List Processing**: Extract username:password pairs or email:password formats
- **Security Testing**: Filter wordlists for specific compliance testing requirements
- **Linguistic Analysis**: Extract words matching specific linguistic patterns
- **Data Sanitisation**: Remove or extract entries with specific characteristics

## Error Handling

The utility includes comprehensive error handling for:
- Invalid folder paths
- Malformed regex patterns
- File access permissions
- Disk space issues

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

I don't mind if you use it however you like - it's not exacxtly rocket surgery.

## Author

**EnsignKilos**  
Security Researcher | Red Teamer | Penetration Tester

## Acknowledgements

Built for high-performance wordlist processing and dictionary management in penetration testing and security research workflows.

## Support

For issues, feature requests, or questions, please open an issue on GitHub.
