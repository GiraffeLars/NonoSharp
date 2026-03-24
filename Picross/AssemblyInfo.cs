using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("9b5e1c84-de02-4f78-b73e-ead56484f191")]

// For testing purposes, allow the unit tests access to internals
[assembly: InternalsVisibleTo("Core.UnitTests")]