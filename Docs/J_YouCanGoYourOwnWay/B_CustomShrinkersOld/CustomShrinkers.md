# Custom Shrinkers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
As stated in OnShrinking(todo: link), a Shrinker is comprised of:
  1. A *Classifier* : responsible for finding the relevant inputs.
  2. A *Reducer* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these per input.
  
