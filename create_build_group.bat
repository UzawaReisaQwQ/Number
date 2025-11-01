@echo off
setlocal enabledelayedexpansion

set PARALLEL=20

set SRC_DIR=chunks

if not exist "%SRC_DIR%\" (
  echo Source folder "%SRC_DIR%" not found.
  pause
  exit /b 1
)

if not exist dlls mkdir dlls

for %%G in (group_*.bat) do del "%%G" 2>nul

set /A idx=0
for %%F in ("%SRC_DIR%\Chunk_*.cs") do (
  set /A g = idx %% PARALLEL
  set grp=group_!g!.bat
  if not exist "!grp!" (
    echo @echo off > "!grp!"
    echo rem group file for index !g! >> "!grp!"
  )
  echo echo Compiling %%~nxF ... >> "!grp!"
  echo csc -nologo -target:library -out:"dlls\%%~nF.dll" "%%F" >> "!grp!"
  set /A idx+=1
)

echo Created %idx% tasks distributed into %PARALLEL% groups.
echo Generated group_0.bat ... group_%PARALLEL%-1.bat (some may be empty if few files).
pause
