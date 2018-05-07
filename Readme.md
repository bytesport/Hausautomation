#startprojekt für AWS lambda per VS 2017
```
    cd "Hausautomation"
    dotnet restore
```

Execute unit tests
```
    cd "Hausautomation/test/Hausautomation.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "Hausautomation/src/Hausautomation"
    dotnet lambda deploy-function
```
