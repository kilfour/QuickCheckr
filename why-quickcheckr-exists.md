# From QuickNet to QuickCheckr
> The Wonder(ing) Years

Around 2007, I was taking a break from development, and pursuing a different career.  
But I could never stop tinkering with code. So I ([Learned Me a Haskell for Great Good!](https://learnyouahaskell.github.io/chapters.html))

That's when I first encountered **QuickCheck**, and it absolutely blew my mind.

A while later I got a new job as a C# dev and I really wanted something similar for .NET, but there was nothing there.  
The closest thing around was [PEX](https://www.microsoft.com/en-us/research/project/pex-and-moles-isolation-and-white-box-unit-testing-for-net/), but it was… let’s say *not ergonomic*.  

So I rolled my own.

But since I had dropped out of school at 17, the theory and math behind QuickCheck completely escaped me.  
So I did what I always do: I tried to solve the problem my own way, one step at a time.  
Back then, C# was still growing up, and I had to fight the type system at every turn, but eventually I ended up with **QuickNet**,
a semi-working prototype I used personally with some success.

At around 2009 I got involved with an open-source project called [Agatha](https://github.com/davybrion/Agatha), a library similar to the current day [Mediatr](https://mediatr.io/).

I convinced Davy of the value of property-based testing and added some simple QuickNet tests. 

A couple of months later, this happened (retrieved from the Wayback Machine):

> **How QuickNet Found 2 Bugs That You And I Didn’t**  
> *Posted by Davy Brion on December 22nd, 2009*
> 
> I recently posted the first draft of the caching implementation of Agatha.  You might want to take another look at the piece of code that deals with processing requests in the Request Processor.  I thought it was alright.  And if only 10% of my readers actually read that code, then it means that about 200 people thought it was alright too because nobody mentioned a possible problem with that part.  Today i was adding some cached requests/responses to the RequestProcessor’s QuickNet exception handling test.  And lo and behold, 2 bugs showed up that i hadn’t anticipated.  
> ...  
> And here’s the beauty of the whole thing.  I hadn’t even thought about how exception handling also influences how you deal with cached responses.  I didn’t take exception handling into account when adding the caching code, and without even adding specific tests to see whether the caching worked correctly, my QuickNet specs that intended to cover something that doesn’t really have anything to do with caching uncovered 2 issues with my caching code.  
> ...

That caused a bit of a stir.  
I wasn't surprised, I'd had similar experiences on my own, less public projects.  
But the *ceremony* needed to write these tests was problematic, and shrinking failed me a lot of the time.

I kept working on it every once in a while, but mainly used it as a personal tool,
something to attach to CI whenever I wanted to verify that a piece of code that looked *sus* actually behaved correctly.
Once a failure was found (or I was convinced the SUT was correct), I removed the test again because, honestly, **I was the only person on this god-given earth who could maintain it** ;-).

Agatha eventually faded, its main target was Silverlight, and we all know how that ended.  
I also burned out for a while, drifted into an easy job, and wrote too much bad code.

Eventually I woke up, went freelance, and returned to writing functional code.

By then QuickNet had become **QuickDotNetCheck**, still an OO fluent interface library.  
Later it became **QuickAcid**, where I introduced the LINQ composition technique that would later make QuickFuzzr so pleasant to use.  
But the QuickAcid engine was still unreliable.

Earlier this year I got a gig teaching full-stack .NET to job-seekers.  
Since I hadn't written C# in five years, I decided to modernize my old library to get back into the swing of things.  
The FP stint paid off, I got the engine running better than ever, in record time.

I used it extensively during the course to verify the code students submitted ;-).

Near the end of the course, when students were preparing for internships and my technical duties were mostly done,
I had some time on my hands to finish what I started.  
I'm not saying the journey is complete, but I've definitely reached some sort of destination ...

And now I have the tool I wished I had, all the way back in 2009:

**QuickCheckr: CSI .NET**



