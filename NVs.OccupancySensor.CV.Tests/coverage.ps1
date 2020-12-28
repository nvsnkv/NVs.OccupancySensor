rm coveragereport -r
dotnet test --collect:"XPlat Code Coverage"
$folder = gci .\TestResults | ? { $_.PSIsContainer } | sort CreationTime -desc | select -f 1 | select Name
$folder = ".\TestResults\" + $folder.Name
reportgenerator "-reports:${folder}\coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:Html

coveragereport\index.html