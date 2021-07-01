#!/bin/sh

git pull
dotnet build -c Release

mv nuget.config0 nuget.config

dotnet nuget push "./src/EntityFramework/bin/Release/Gridify.EntityFramewor.1.3.3.nupkg" --source "github" 
dotnet nuget push "./src/EntityFramework5/bin/Release/Gridify.EntityFramework5.1.3.3.nupkg" --source "github"
dotnet nuget push "./src/Core/bin/Release/Gridify.1.3.3.nupkg" --source "github" 
mv nuget.config nuget.config0