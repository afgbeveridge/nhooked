# What does it do?
nhooked (en-hooked) is a C# framework designed to allow a flexible web hook provider to be created. By flexible, I mean:

* Reliable
* Scalable
* Simple

There are a great number of 'how to's', blogs and the like which describe how to *consume* web hooks, but few that indicate how to create an implementation that 
can serve as a web hook provider i.e the source of the events that web hook consumers await.

nhooked tries to provide such a framework, making it as pluggable as possible and provding some base implementations.