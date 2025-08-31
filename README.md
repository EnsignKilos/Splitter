(AI SLOP AHEAD):

# FileSplitter

A high-performance C# utility for splitting large text files based on regex pattern matching. Designed for efficient processing of massive datasets with minimal memory footprint.

## Features

- **Regex-based filtering**: Process only lines matching your specified pattern
- **Large file handling**: Efficiently processes multi-gigabyte text files
- **Automatic file splitting**: Splits output into manageable chunks (10 million lines per file)
- **Memory optimised**: Uses buffered writing with 1 million line buffer capacity
- **Progress tracking**: Real-time processing statistics and completion percentage
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
# Extract all lines containing IP addresses
FileSplitter /path/to/logs "\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b"

# Extract lines with email addresses
FileSplitter ./data "[\w\.-]+@[\w\.-]+\.\w+"

# Extract lines containing specific keywords
FileSplitter C:\logs "error|warning|critical"

# Extract lines with timestamps
FileSplitter ./audit "^\d{4}-\d{2}-\d{2}"
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

- **Log Analysis**: Extract specific error patterns from application logs
- **Data Mining**: Filter large datasets for relevant information
- **Security Auditing**: Extract suspicious patterns from audit logs
- **Text Processing**: Split and filter massive text corpora
- **Compliance**: Extract specific data patterns for regulatory requirements

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

[Specify your licence here]

## Author

**EnsignKilos**  
Security Researcher | Red Teamer | Penetration Tester

## Acknowledgements

Built for high-performance text processing in security operations and data analysis workflows.

## Roadmap

- [ ] Parallel processing support for multiple files
- [ ] Custom output directory specification
- [ ] Configurable lines-per-file via command line
- [ ] Support for additional file formats (CSV, JSON)
- [ ] Progress bar visualisation
- [ ] Compression options for output files

## Support

For issues, feature requests, or questions, please open an issue on GitHub.
