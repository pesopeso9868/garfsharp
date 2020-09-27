taskkill /im "Garfield.exe"
".\csc\csc.exe" -debug:full -r:Newtonsoft.Json.dll -r:System.Net.Http.dll -out:Garfield.exe -target:winexe garfield.cs && garfield.exe || pause
