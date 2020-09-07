taskkill /im "Garfield.exe"
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe" -debug -reference:Newtonsoft.Json.dll -out:Garfield.exe -target:winexe garfield.cs && garfield || pause
