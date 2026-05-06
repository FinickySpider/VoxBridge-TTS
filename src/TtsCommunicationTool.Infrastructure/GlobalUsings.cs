// Explicit global usings required because UseWPF=true in a class library replaces
// the standard Microsoft.NET.Sdk implicit usings set with the WindowsDesktop set,
// which does not include System.IO. These restore the missing namespaces.
global using System.IO;
