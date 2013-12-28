AssemblyHasher
==============

Hash tool for .Net assemblies

Usage
-------

``` powershell
  AsemblyHasher.exe SomeLibrary.dll
  AsemblyHasher.exe SomeApp.exe

  # ignore AssemblVersion and AssemblyFileVersion 
  AsemblyHasher.exe --ignore-versions SomeLibrary.dll

  # hash multiple files at once
  AsemblyHasher.exe SomeLibrary.dll Another.dll A3rdone.dll

  # other files (not *.dll|exe) content gets hashed too
  AsemblyHasher.exe SomeLibrary.dll Picture.jpeg REAME.md
```

The output is a hash (using [MurMur-128](http://en.wikipedia.org/wiki/MurmurHash)) of assembly contents (source code + embedded resources, which are extracted using an embedded copy of ildasm.exe).

Why
-----

Why not hash the .dll/.exe file directly? Because every time an assembly is compiled a few always changing values are added (MVID, timestamp, Image Base, and many others) making the file content different on every compilation.
This tool will disassemble and then remove those values before hashing.

There are many possible use cases, but my main motivation for this is using it in git-based deployments to keep a cleaner history and to avoid restarting the app when no dlls have changed (IIS hot deploy).
For that purpose you can use the included powershell script ```GitResetUnmodifiedAssemblies.ps1```.

Requirements
----------

 - .Net Framework 4.5

Credits
-------

- Vasil Trifonov (@vtrifonov), idea and initial version published at [this article](http://www.vtrifonov.com/2012/11/compare-two-dll-files-programmatically.html?showComment=1365644703161#c1212265983525966443)
- Benjamin Eidelman (@benjamine), packing/publishing, sha1, hashing embedded resources

