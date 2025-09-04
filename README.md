# AlGraphTools
An extension to the Unity graph tools package with a dialogue graph sample. 
This package has been created with the design goal of allowing easy access graph tools for scriptable object workflows.
Graph data has been refactored to be saved as a single scriptable object where a node can references nodes within itself. 
Object data is persistent between Editor sessions as OnBefore and OnAfter Serialize methods have been extended for node references.  

