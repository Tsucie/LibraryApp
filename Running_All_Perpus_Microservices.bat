@echo off
start /i /d "./API_Gateway" dotnet run
start /i /d "./JWT_Authentication" dotnet run
start /i /d "./PerpusApp" dotnet run