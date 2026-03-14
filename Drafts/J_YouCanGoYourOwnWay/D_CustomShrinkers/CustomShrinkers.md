# Custom Shrinkers
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively. 

They are registered on a per-input basis using an overload on the `Checkr.Input)()` method.  
Contruction of a shrinker is done using factory methods on the `Shrink` static class.

Let's have a look at a simple example:  

[InputShrinking]: ../../../Docs/H_OnShrinking/C_InputShrinking/InputShrinking.md

[Classifiers]: A_Classification/Classifiers.md

[Reducers]: B_Reduction/Reducers.md
