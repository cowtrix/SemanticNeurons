using System;
using System.Collections.Generic;

namespace SemanticNeurons
{
	public interface IFunctionObject<TIn, TOut>
	{
		TOut Calculate(TIn value);
		T Clone<T>() where T: IFunctionObject<TIn, TOut>;
	}

	public interface IObjectMutator<T>
	{
		void Mutate(T obj, float mutationStrength);
	}

	public interface IByteProgramMutator : IObjectMutator<ByteProgram>
	{
		public void AdjustWeights(IEnumerable<ValueTuple<InstructionSet, int>> wDelta);
	}

}
