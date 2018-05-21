# DotNetTar
async/await

# Why ?
I had a hard time to found an async Tar manager that allow to read/write the content without unpacking on the filesystem, so i made this library.

I'm using the .tar of the redis distribution
Done: Read all the files and compare their MD5 to the files unpacked with 7zip

TODO:
Test of the two different checksum(7Zip doesn't calculate the checksum the same way)
Test of the Tar creation.
Test of the Tar unpacking on filesystem.
