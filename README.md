VersionTools
============
This project contains a console application that helps managing the versions of .Net projects. It uses a set of common conventions and [semantic versioning](http://semver.org/spec/v2.0.0.html).

Conventions
---------------

### Root version  
The _root version_ is the default version that is used when no other version is defined for a project. The root version is set by either passing a valid semantic version as an argument using the `-version` option or by putting a `version.txt` file in the starting directory.

### version.txt files
Versions are defined by putting a file named `version.txt` in a directory. In a recursive scan of a solution, versions are inherited by sub directories. E.g. if a version.txt file is put in the root directory of the 

    [src]
     |- version.txt
     |- [Project1]
        |- version.txt
     |- [Project2]

Getting help
------------
To get a list of the available actions and options type

```batchfile
> aver help
```

Getting a list of versions in a solution
----------------------------------------

The `list` action lets you list the versions of the project in a directory or a directory structure.

```batchfile
> aver list c:\projects\myapp -recurse
```

Lists the resolved versions of the projects found in a recursive search in the `c:\projects\myapp` directory.

Getting a list of .Net assembly versions
----------------------------------------

You can also list the versions of assemblies in a directory or directoy structure. 

```batchfile
> aver list c:\projects\myapp -assembly
```

Lists the versions of assemblies in the `c:\projects\myapp` directory.

