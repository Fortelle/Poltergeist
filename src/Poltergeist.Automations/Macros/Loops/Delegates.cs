using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Loops;

public delegate Task<bool> StartCallback(IMacroProcessorShared processor);

public delegate Task ExecuteCallback(IMacroProcessorShared processor, IterationContext context);

public delegate Task TransitionCallback(IMacroProcessorShared processor, TransitionContext context);

public delegate Task EndCallback(IMacroProcessorShared processor);

public delegate void FinalizeCallback(IMacroProcessorShared processor);
