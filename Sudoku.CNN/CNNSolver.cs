using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Shared;
using Python.Runtime;

namespace Sudoku.CNN
{
	public class CNNSolver : PythonSolverBase
	{
		public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
		{
			using (PyModule scope = Py.CreateScope())
			{

				// Injectez le script de conversion
				AddNumpyConverterScript(scope);

				// Convertissez le tableau .NET en tableau NumPy
				var pyCells = AsNumpyArray(s.Cells, scope);

				// create a Python variable "instance"
				scope.Set("instance", pyCells);

				// run the Python script
				
				var codes = new List<string> { Resources.data_processes, Resources.model, Resources.Sudoku_py };

				codes.ForEach(code => scope.Exec(code));

				PyObject result = scope.Get("result");

				// Convertissez le résultat NumPy en tableau .NET
				var managedResult = AsManagedArray(scope, result);

				return new SudokuGrid() { Cells = managedResult };
			}
		}

		protected override void InitializePythonComponents()
		{
			//declare your pip packages here
			InstallPipModule("numpy");
			InstallPipModule("scikit-learn");
			InstallPipModule("pandas");
			InstallPipModule("keras");
			InstallPipModule("tensorflow");
			//InstallPipModule("copy");
			base.InitializePythonComponents();
		}
	}
}
