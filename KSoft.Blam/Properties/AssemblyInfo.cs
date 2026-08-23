using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("69451799-d2c2-4d3f-9a7d-1dc3e134de19")]

[assembly: InternalsVisibleTo("Test.KSoft.Blam")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1028:EnumStorageShouldBeInt32")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1034:NestedTypesShouldNotBeVisible",
	Justification = "Because I do this all over the place")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1051:DoNotDeclareVisibleInstanceFields",
	Justification = "Because I do this all over the place")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1062:ValidateArgumentsOfPublicMethods",
	Justification = "Explicit guard clauses validate public inputs where needed")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1303:DoNotPassLiteralsAsLocalizedParameters")]

[assembly: SuppressMessage("Style",
	"IDE1005:Delegate invocation can be simplified.",
	Justification = "Can't breakpoint simplification")]
[assembly: SuppressMessage("Design",
	"CA1033:Interface methods should be callable by child types",
	Justification = "Explicit interface implementations intentionally preserve collection and serialization model contracts")]
