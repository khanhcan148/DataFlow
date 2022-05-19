using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowExample
{
    internal class Program
    {
        private static BufferBlock<Input> _bufferBlock;
        private static TransformBlock<Input, OutPut> _stepPlus1Block;
        private static TransformBlock<OutPut, FinalResult> _stepFinalBlock;

        static async Task Main(string[] args)
        {
            //init flow
            _bufferBlock = new BufferBlock<Input>(new DataflowBlockOptions());
            _stepPlus1Block = new TransformBlock<Input, OutPut>(async input =>
            {
                var output = new OutPut()
                {
                    OutputValue = input.InputValue++
                };
                await Task.Delay(1000).ConfigureAwait(false);
                Console.WriteLine("OutputValue: " + output.OutputValue);
                return output;
            });
            _stepFinalBlock = new TransformBlock<OutPut, FinalResult>(async put =>
            {
                Console.WriteLine("FinalResultValue: " + put.OutputValue * 10);
                await Task.Delay(1000).ConfigureAwait(false);
                return new FinalResult() { FinalResultValue = put.OutputValue * 10 };
            });

            //link flow
            _bufferBlock.LinkTo(_stepPlus1Block);
            _stepPlus1Block.LinkTo(_stepFinalBlock);

            
            // start flow
            var loop = new List<int> { 1, 2, 3, 4, 5 };
            Parallel.ForEach(loop, async (a) =>
            {
                Console.WriteLine("sending job: " + a);
                while (true)
                {
                    Console.WriteLine("Working on job#: " + a);
                    await _bufferBlock.SendAsync(new Input() { InputValue = a });
                    await Task.Delay(5000).ConfigureAwait(false);
                }
            });
            
            await Task.WhenAll(_stepPlus1Block.Completion, _stepFinalBlock.Completion).ConfigureAwait(false);
            Console.ReadLine();
        }
    }

    class Input
    {
        public int InputValue { get; set; }
    }

    class OutPut
    {
        public int OutputValue { get; set; }
    }

    class FinalResult
    {
        public int FinalResultValue { get; set; }
    }
}