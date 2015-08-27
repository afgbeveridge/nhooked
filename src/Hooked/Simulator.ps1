start-process Hooked.exe -WorkingDirectory ".\NHookedExamples\bin\Debug" -WindowStyle Normal -argumentList @("ef", "rhttp")
start-process SimulatedRemoteClient.exe -WorkingDirectory ".\SimulatedRemoteClient\bin\Debug" -WindowStyle Normal
Write-Host "Waiting to make sure all processes are booted"
Start-Sleep -s 10
start-process NHookedSimulator.exe -WorkingDirectory ".\NHookedSimulator\bin\Debug" -WindowStyle Normal -argumentList @("http://localhost:55555")