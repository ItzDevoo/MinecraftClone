# DevCraft Texture Pack Support

## How to Use Texture Packs

DevCraft supports Minecraft-style texture packs! Here's how to add them:

### 📦 **Option 1: Minecraft Resource Packs (Recommended)**

1. **Download a free/open-source resource pack**:
   - [Faithful](https://faithfulpack.net/) - High-resolution faithful recreation
   - [John Smith Legacy](https://www.johnsmithlegacy.co.uk/) - Medieval style
   - [Soartex Fanver](https://soartex.net/) - Smooth, clean textures
   - [Clarity](https://www.curseforge.com/minecraft/texture-packs/clarity) - Modern, clean look

2. **Place the .zip file** in this `TexturePacks` folder
3. **Load in-game** through the settings menu (coming soon!)

### 🎨 **Option 2: Custom Texture Folder**

1. **Create a folder** in `TexturePacks` (e.g., "MyCustomPack")
2. **Add PNG files** with these names:
   ```
   stone.png, dirt.png, grass_side.png, grass_top.png,
   cobblestone.png, brick.png, sand.png, water.png,
   glass.png, leaves.png, oak_log.png, spruce_plank.png, etc.
   ```
3. **Match the names** from the existing blocks.json file

### ⚖️ **Legal Notice**

- ✅ **USE**: Free/open-source texture packs
- ✅ **USE**: Texture packs with permissive licenses
- ✅ **USE**: Your own custom textures
- ❌ **DON'T USE**: Official Minecraft textures (copyrighted by Mojang)
- ❌ **DON'T USE**: Paid texture packs without permission

### 🔧 **Technical Details**

**Supported Formats:**
- ZIP files (Minecraft resource pack format)
- Loose PNG files in folders
- Standard 16x16, 32x32, 64x64 resolution textures

**File Structure for ZIP packs:**
```
MyTexturePack.zip
├── assets/
│   └── minecraft/
│       └── textures/
│           └── block/
│               ├── stone.png
│               ├── dirt.png
│               └── ...
```

**File Structure for folder packs:**
```
MyTexturePack/
├── stone.png
├── dirt.png
├── grass_side.png
└── ...
```

### 🎮 **Usage**

The texture pack loader will:
1. **Auto-detect** packs in this folder
2. **Load textures** on demand
3. **Fall back** to default textures if pack texture missing
4. **Handle errors** gracefully

Currently implements programmatic loading - UI integration coming in next update!

### 📝 **Block Names Reference**

Current supported blocks (must match these names):
- `stone`, `dirt`, `grass_side`, `grass_top`
- `cobblestone`, `brick`, `sand`, `sandstone_side`, `sandstone_top`
- `water`, `glass`, `leaves`, `snow`
- `oak_log`, `oak_bark`, `oak_plank`
- `birch_log`, `birch_bark`, `birch_plank`
- `spruce_plank`, `stone_brick`
- `bedrock`, `glowstone`, `iron`, `granite`, `terracotta`

More blocks can be easily added by updating blocks.json!
