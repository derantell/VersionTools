VersionTools
============
This project contains a .Net console application which helps managing the versions of .Net projects. It uses a set of common conventions and [semantic versioning](http://semver.org/spec/v2.0.0.html). 

It was started because we wanted a more fine grained control over how [TeamCity](http://www.jetbrains.com/teamcity/) versioned assemblies. 

The most likely use case is to run the tool as a step in your [continuous intergration](http://en.wikipedia.org/wiki/Continuous_integration) pipeline after the code is cloned but before the project is built. The versions are set by patching _AssemblyInfo_ and _nuspec_ files. 

Getting started
---------------
Download the latest version from [Releases](https://github.com/derantell/VersionTools/releases) or clone the repo and build it locally. You might want to add the location of the executable to your **PATH** if you are putting it on a CI server.

The program is called `aver.exe`. Type `aver help` at the console to get a list of the various _actions_ and _options_ supported. 

Conventions
-----------

### Root version  

The _root version_ is the default version that is used when no other version is defined for a project. The root version is set by either passing a **valid semantic version** as an argument using the `-version` option or by putting a _version.txt_ file in the starting directory.

### version.txt files

Versions are defined by putting a file named _version.txt_ in a directory. In a recursive scan of a solution, versions are inherited by sub directories. 

The version.txt file format is simple: the first line of the file **must** be a valid semantic version. Any other lines are skipped. Example:

    1.2.3-rc3

E.g. if a version.txt file is put in the root directory of this example, the _root version_ of the solution will be `1.0.0`. Project1 will have version `2.0.0` and Project2 will inherit the _root version_ since it does not have a _version.txt_ file.

    [sln]
    |- version.txt (1.0.0)
    |- [Project1]
       |- version.txt (2.0.0)
    |- [Project2]

Usage
-----

The application uses the common pattern of actions and option like e.g. git 

```batchfile
> aver <action> [options]
```

### Getting help

To get a list of the available actions and options type

```batchfile
> aver help
```

### Getting a list of versions in a solution

The `list` action lets you list the versions of the project in a directory or a directory structure. 

```batchfile
> aver list c:\projects\myapp -recurse
VersionTools.Cli
  Location:           c:\projects\myapp
  Resolved version:   1.0.1-beta
...
```

Lists the resolved versions of the projects found in a recursive search in the `c:\projects\myapp` directory. 

### Getting a list of .Net assembly versions

You can also list the versions of assemblies in a directory or directoy structure by adding the `-assembly` option. 

```batchfile
> aver list c:\projects\myapp -assembly -recurse
aver
  Location:           c:\projects\myapp\bin\debug
  Assembly version:   1.0.1.0
  File version:       1.0.1.0
  Product version:    1.0.1-beta
...
```

Lists the versions of assemblies in the `c:\projects\myapp` directory.

