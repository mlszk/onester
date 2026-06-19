# GitHub instructions for onester releases

Program:

```text
onester — Object nester
```

Recommended repo:

```text
https://github.com/mlszk/onester
```

Local Windows folder:

```text
D:\syncthing\work\_AI\onester\github\
```

WSL path:

```bash
/mnt/d/syncthing/work/_AI/onester/github/
```

## First upload from WSL

```bash
cd /mnt/d/syncthing/work/_AI/onester/github/
pwd
ls
```

You should see:

```text
onester.csproj
Program.cs
MainForm.cs
NestingEngine.cs
DxfWriter.cs
README.md
```

Initialize and push:

```bash
git init
git add .
git commit -m "Initial onester v0.1.0 experimental source"
git branch -M main
git remote add origin https://github.com/mlszk/onester.git
git push -u origin main
```

If Git asks for identity:

```bash
git config --global user.name "Alx Malyszko"
git config --global user.email "mlszk@gmx.com"
```

## Build release

```bash
cd /mnt/d/syncthing/work/_AI/onester/github/
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false -o publish/onester-v0.1.0
```

Expected Windows output:

```text
D:\syncthing\work\_AI\onester\github\publish\onester-v0.1.0\onester.exe
```

Create ZIP:

```bash
powershell.exe -Command "Compress-Archive -Path 'D:\syncthing\work\_AI\onester\github\publish\onester-v0.1.0\*' -DestinationPath 'D:\syncthing\work\_AI\onester\github\onester-v0.1.0-win-x64.zip' -Force"
```

## Full source ZIP

```bash
cd /mnt/d/syncthing/work/_AI/onester/github/
zip -r onester-v0.1.0-full-source.zip . \
  -x "bin/*" "obj/*" ".git/*" ".vs/*" "publish/*" "*.user" "*.suo" "*.pdb"
```

If `zip` is missing:

```bash
sudo apt install zip
```

## GitHub Release

Open:

```text
https://github.com/mlszk/onester/releases/new
```

Use:

```text
Tag: v0.1.0
Title: onester v0.1.0 experimental
```

Upload assets:

```text
onester-v0.1.0-win-x64.zip
onester-v0.1.0-full-source.zip
```

## After publishing

Download your own release from GitHub and test it:

```text
ZIP downloads correctly
exe launches
title says onester v0.1.0 experimental
default 600 x 300 / Ø64 / gap 4.5 / edge 7 gives valid layout
DXF exports
DXF opens in AutoCAD
README is included
```

## Normal update

```bash
cd /mnt/d/syncthing/work/_AI/onester/github/
git status
git add .
git commit -m "Fix onester preview guide lines"
git push
```

Do not use names like:

```text
final.zip
fixed.zip
latest.exe
newnew.zip
```

Use boring versioned names.

- Gap envelope and edge clearance logic corrected: edge clearance now keeps the full object+gap envelope inside the sheet.
