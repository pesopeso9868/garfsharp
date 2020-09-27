taskkill /im "Garfield.exe"
".\csc\csc.exe" -debug:full -reference:Newtonsoft.Json.dll -reference:System.Net.Http.dll -out:Garfield.exe -target:winexe garfield.cs && garfield.exe || pause
