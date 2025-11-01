@echo off
REM Build each existing chunks\Chunk_*.cs into dlls\Chunk_xxxxx.dll using csc
if not exist chunks (
  echo chunks folder not found.
  pause
  exit /b 1
)
if not exist dlls mkdir dlls

for %%F in (chunks\Chunk_*.cs) do (
  echo Compiling %%F ...
  rem %%~nF is the filename without extension, e.g. Chunk_00012
  csc -nologo -target:library -out:"dlls\%%~nF.dll" "%%F"
  if errorlevel 1 (
    echo Failed compiling %%F
  )
)
echo Done. DLLs are in .\dlls\
pause
