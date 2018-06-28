# DotNetTar
Allow to read, create, or write Stream of tar file.
Nuget package: https://www.nuget.org/packages/dotnettar/

# Why ?
I had a hard time to found an async Tar manager that allow to read/write the content without unpacking on the filesystem, so i made this library.

I'm using the .tar of the redis distribution
Done: Read all the files and compare their MD5 to the files unpacked with 7zip

TODO:
More tests.
