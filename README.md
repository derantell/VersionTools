VersionTools
============

This project contains a console application that helps managing the versions of .Net projects. It uses a set of common conventions and [semantic versioning](http://semver.org/spec/v2.0.0.html).

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

```batchfile
> aver list c:\projects\myapp -assembly
```