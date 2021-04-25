namespace SemanticTensors
{
	public enum InstructionSet : byte
	{
		NULL = 0,				// null operation, no effect

		REG_1 = 1,				// the next 2 bytes are a int that should be written into the 1st registry
		REG_2 = 2,              // the next 2 bytes are a int that should be written into the 2nd registry
		REG_3 = 3,              // the next 2 bytes are a int that should be written into the 3rd registry
		REG_4 = 4,              // the next 2 bytes are a int that should be written into the 4th registry

		ADD_REG_1 = 5,			// add the value in registry 1 to the input
		ADD_REG_2 = 6,			// add the value in registry 2 to the input
		ADD_REG_3 = 7,			// add the value in registry 3 to the input
		ADD_REG_4 = 8,			// add the value in registry 4 to the input

		SUB_REG_1 = 9,			// SUB the value in registry 1 to the input
		SUB_REG_2 = 10,			// SUB the value in registry 2 to the input
		SUB_REG_3 = 11,			// SUB the value in registry 3 to the input
		SUB_REG_4 = 12,         // SUB the value in registry 4 to the input

		DIV_REG_1 = 13,          // DIV the value in registry 1 to the input
		DIV_REG_2 = 14,         // DIV the value in registry 2 to the input
		DIV_REG_3 = 15,         // DIV the value in registry 3 to the input
		DIV_REG_4 = 16,         // DIV the value in registry 4 to the input

		COPY_IN_REG_1 = 17,          // COPY_IN_ the value in registry 1 to the input
		COPY_IN_REG_2 = 18,         // COPY_IN_ the value in registry 2 to the input
		COPY_IN_REG_3 = 19,         // COPY_IN_ the value in registry 3 to the input
		COPY_IN_REG_4 = 20,         // COPY_IN_ the value in registry 4 to the input

		COPY_VAL_REG_1 = 21,          // COPY_VAL_ the value in registry 1 to the input
		COPY_VAL_REG_2 = 22,         // COPY_VAL_ the value in registry 2 to the input
		COPY_VAL_REG_3 = 23,         // COPY_VAL_ the value in registry 3 to the input
		COPY_VAL_REG_4 = 24,         // COPY_VAL_ the value in registry 4 to the input

		MUL_REG_1 = 25,          // MUL_ the value in registry 1 to the input
		MUL_REG_2 = 26,         // MUL_ the value in registry 2 to the input
		MUL_REG_3 = 27,         // MUL_ the value in registry 3 to the input
		MUL_REG_4 = 28,         // MUL_ the value in registry 4 to the input
	}
}
