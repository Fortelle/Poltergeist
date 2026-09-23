using Microsoft.VisualStudio.TestTools.UnitTesting;

// Explicitly enable test parallelization at the assembly level to satisfy MSTEST0001.
// Workers = 0 lets the framework decide (usually number of processors).
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
