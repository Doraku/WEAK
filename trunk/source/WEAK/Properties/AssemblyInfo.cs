using System.Reflection;

[assembly: AssemblyTitle("WEAK Core")]
[assembly: AssemblyDescription(
@"Core functionalities of WEAK
Communication
    - Publisher/Subscriber pattern implementation

Helper
    - Extension method to create a weak delegate from a delegate, so it doesn't hold strong reference on its target
    - Extensions methods on IDisposable to merge them as one
    - Extensions methods on object to check input parameters

Input
    - Command pattern implementation with numerous extensions to handle most commonly used operations and objects (ICollection, IList, IDictionary, value setting, custom action)

Object
    - IOC pattern, object factory

Serialization
    - Methods to handle load/save operations on DataContract decorated classes")]
