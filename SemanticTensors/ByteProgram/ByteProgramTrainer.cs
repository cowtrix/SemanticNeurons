using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticNeurons
{
	public class ByteProgramTrainer : Trainer<ByteProgram, float, float>
	{
		public ByteProgramTrainer(Func<ByteProgram, float, float, float> errorCalculator, IObjectMutator<ByteProgram> mutator, TrainerOptions options) 
			: base(errorCalculator, mutator, options)
		{
		}		
	}

}
