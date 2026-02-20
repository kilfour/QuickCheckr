# Custom Shrinkers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively.  

  

[Classifiers]: A_Classification/Classifiers.md

[Reducers]: B_Reduction/Reducers.md
