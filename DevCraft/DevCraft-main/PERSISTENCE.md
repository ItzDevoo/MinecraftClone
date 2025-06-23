# DevCraft File-Based Persistence System

## Overview
DevCraft now uses a purely file-based persistence system using JSON files instead of SQLite. This provides better portability, easier debugging, and simpler backup/restore operations.

## File Structure
```
Saves/
├── [SaveName]/
│   ├── parameters.json          # World metadata (seed, world type, etc.)
│   ├── save_icon.png           # Screenshot thumbnail
│   └── chunks/                 # Chunk modifications
│       ├── chunk_0_2_0.json   # Individual chunk files
│       ├── chunk_1_2_0.json
│       └── ...
```

## Chunk File Format
Each chunk file contains only the blocks that have been modified from the original generated state:

```json
{
  "ChunkIndex": {
    "X": 0,
    "Y": 2, 
    "Z": 0
  },
  "BlockModifications": {
    "8,12,7": 1,     // Block at position (8,12,7) = Block type 1
    "15,5,3": 0      // Block at position (15,5,3) = Air (removed)
  }
}
```

## Benefits
- **Human Readable**: JSON files can be inspected and modified manually
- **No Dependencies**: No SQLite runtime required
- **Better Performance**: No database connection overhead
- **Easier Backup**: Simply copy the Saves directory
- **Cross Platform**: JSON works everywhere
- **Smaller Files**: Only modified blocks are stored

## Migration
The system automatically cleans up old `data.db` files when initialized. Existing saves will be regenerated using the original world generation seed.
