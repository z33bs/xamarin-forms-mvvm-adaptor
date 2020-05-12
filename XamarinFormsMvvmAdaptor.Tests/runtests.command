#!/bin/sh

# Change to directory using absolute path this script is in
BASEDIR=$(dirname "$0")
echo "$BASEDIR"
cd $BASEDIR

# Remove old test results
rm -r TestResults/

# Run tests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput='./TestResults/'

# Run ReportGenerator
dotnet ~/.nuget/packages/reportgenerator/4.5.6/tools/netcoreapp3.0/ReportGenerator.dll "-reports:./TestResults/coverage.cobertura.xml" "-targetdir:./TestResults/CoverageReport" -reporttypes:Html

# Open Report
open ./TestResults/CoverageReport/index.htm
