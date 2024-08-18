To compile in Windows, load the solution in Visual Studio 2022, and build.

In linux, make sure you have NET 8.0 installed

1. clone this repo into a directory on your system (git clone)
2. change to the repo directory  (cd submit)
3. build the solution (dotnet build)
4. You will see a warning about FStream being used before being assigned a value.  It isn't but .NET is picky.
5. You will find the resulting binary in /submit/bin/Debug/net8.0 as 'submit'  You can copy that to anywhere in your path

