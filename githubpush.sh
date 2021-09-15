#!/bin/sh

git pull
dotnet build -c Release

mv nuget.config0 nuget.config

dotnet nuget push "./src/Gridify.EntityFramework/bin/Release/Gridify.EntityFramewor.1.3.3.nupkg" --source "github" 
dotnet nuget push "./src/Gridify/bin/Release/Gridify.1.3.3.nupkg" --source "github" 
mv nuget.config nuget.config0