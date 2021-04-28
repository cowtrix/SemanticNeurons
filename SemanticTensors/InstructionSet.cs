namespace SemanticTensors
{
	public enum InstructionSet : byte
	{
		NULL = 0,               // null operation, no effect

		REG_1,              // the next 2 bytes are a int that should be written into the 1st registry
		REG_2,              // the next 2 bytes are a int that should be written into the 2nd registry
		REG_3,              // the next 2 bytes are a int that should be written into the 3rd registry
		REG_4,              // the next 2 bytes are a int that should be written into the 4th registry

		ADD_REG_1,          // add the value in registry 1 to the input
		ADD_REG_2,          // add the value in registry 2 to the input
		ADD_REG_3,          // add the value in registry 3 to the input
		ADD_REG_4,          // add the value in registry 4 to the input

		SUB_REG_1,          // SUB the value in registry 1 to the input
		SUB_REG_2,          // SUB the value in registry 2 to the input
		SUB_REG_3,          // SUB the value in registry 3 to the input
		SUB_REG_4,         // SUB the value in registry 4 to the input

		DIV_REG_1,          // DIV the value in registry 1 to the input
		DIV_REG_2,         // DIV the value in registry 2 to the input
		DIV_REG_3,         // DIV the value in registry 3 to the input
		DIV_REG_4,         // DIV the value in registry 4 to the input

		MUL_REG_1,          // MUL_ the value in registry 1 to the input
		MUL_REG_2,         // MUL_ the value in registry 2 to the input
		MUL_REG_3,         // MUL_ the value in registry 3 to the input
		MUL_REG_4,         // MUL_ the value in registry 4 to the input

		COPY_IN_REG_1,          // COPY_IN_ the value in registry 1 to the input
		COPY_IN_REG_2,         // COPY_IN_ the value in registry 2 to the input
		COPY_IN_REG_3,         // COPY_IN_ the value in registry 3 to the input
		COPY_IN_REG_4,         // COPY_IN_ the value in registry 4 to the input

		COPY_VAL_REG_1,          // COPY_VAL_ the value in registry 1 to the input
		COPY_VAL_REG_2,         // COPY_VAL_ the value in registry 2 to the input
		COPY_VAL_REG_3,         // COPY_VAL_ the value in registry 3 to the input
		COPY_VAL_REG_4,         // COPY_VAL_ the value in registry 4 to the input
	}
}
