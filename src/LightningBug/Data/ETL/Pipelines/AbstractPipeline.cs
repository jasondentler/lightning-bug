using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LightningBug.Data.ETL.Operations;

namespace LightningBug.Data.ETL.Pipelines
{
    public abstract class AbstractPipeline : IPipeline
    {
        private readonly List<OperationEntry> operationEntries = new List<OperationEntry>();

        public void AddOperation<TInput, TOutput>(IOperation<TInput, TOutput> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            EnsureInputMatchesPreviousOutput(operation);
            var entry = new AbstractPipeline.OperationEntry(operation, typeof(TInput), typeof(TOutput));
            operationEntries.Add(entry);
        }

        private void EnsureInputMatchesPreviousOutput<TInput, TOutput>(IOperation<TInput, TOutput> operation)
        {
            if (!operationEntries.Any()) return; // First step, assume no input

            var lastEntry = operationEntries.Last();

            if (lastEntry.OutputType.IsAssignableFrom(typeof (TInput)))
                return;

            const string messageFormat = "Output from {0} can't be used as input for {1} because {2} is not assignable from {3}";
            var message = string.Format(messageFormat, lastEntry.Operation.Name, operation.Name, lastEntry.OutputType.Name, typeof (TInput).Name);
            throw new InvalidCastException(message);
        }

        private void AddFinalOperation()
        {
            var entry = operationEntries.Last();
            var openGenericFinalOperation = typeof (FinalOperation<>);
            var finalOperationType = openGenericFinalOperation.MakeGenericType(entry.OutputType);
            var instance = Activator.CreateInstance(finalOperationType);

            var openGenericAddOperation = typeof (AbstractPipeline).GetMethod("AddOperation");
            var addOperation = openGenericAddOperation.MakeGenericMethod(entry.OutputType, typeof (object));
            addOperation.Invoke(this, new[] {instance});
        }

        public virtual void Execute()
        {
            Debug.WriteLine("Executing pipeline");
            try
            {
                if (!operationEntries.Any()) return;

                AddFinalOperation();

                ExecuteOperation(operationEntries.First(), null, operationEntries.Skip(1));
            }
            finally
            {
                Debug.WriteLine("Executed pipeline");
            }
        }

        private void ExecuteOperation(OperationEntry operationEntry, IEnumerable previousOutput, IEnumerable<OperationEntry> childOperations)
        {
            var nextOperationCallback = GenerateNextOperationCallback(operationEntry, childOperations);
            ExecuteOperation(operationEntry, previousOutput, nextOperationCallback);
        }

        private object GenerateNextOperationCallback(OperationEntry currentOperationEntry, IEnumerable<OperationEntry> remainingEntries)
        {
            var openGenericNextOperationCallback = typeof (AbstractPipeline)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(mi => mi.Name == "GenerateNextOperationCallback")
                .Where(mi => mi.IsGenericMethodDefinition)
                .Single(mi => mi.GetGenericArguments().Count() == 1);

            var closedGenericNextOperationCallback = openGenericNextOperationCallback.MakeGenericMethod(currentOperationEntry.OutputType);
            var callback = closedGenericNextOperationCallback.Invoke(this, new object[] {remainingEntries});
            return callback;
        }

        private Action<IEnumerable<TInput>> GenerateNextOperationCallback<TInput>(IEnumerable<OperationEntry> remainingEntries)
        {
            remainingEntries = remainingEntries.ToArray();
            if (!remainingEntries.Any()) return input => { /*NOOP*/ }; // No more operations to execute
            var currentEntry = remainingEntries.First();
            var childEntries = remainingEntries.Skip(1);
            return input =>
            {
                ExecuteOperation(currentEntry, input, childEntries);
            };
        }

        private void ExecuteOperation(OperationEntry operationEntry, IEnumerable previousOutput, object nextOperationCallback)
        {
            var openGenericExecuteOperation = typeof (AbstractPipeline)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(mi => mi.Name == "ExecuteOperation")
                .Where(mi => mi.IsGenericMethodDefinition)
                .Single(mi => mi.GetGenericArguments().Count() == 2);

            var closedGenericExecuteOperation = openGenericExecuteOperation.MakeGenericMethod(operationEntry.InputType, operationEntry.OutputType);
            closedGenericExecuteOperation.Invoke(this, new[] {operationEntry.Operation, previousOutput, nextOperationCallback});
        }

        protected abstract void ExecuteOperation<TInput, TOuput>(IOperation<TInput, TOuput> operation, IEnumerable<TInput> input, Action<IEnumerable<TOuput>> nextOperationCallback);

        protected class OperationEntry
        {
            private readonly IOperation operation;
            private readonly Type inputType;
            private readonly Type outputType;

            public OperationEntry(IOperation operation, Type inputType, Type outputType)
            {
                this.operation = operation;
                this.inputType = inputType;
                this.outputType = outputType;
            }

            public IOperation Operation { get { return operation; } }
            public Type InputType { get { return inputType; } }
            public Type OutputType { get { return outputType; } }
        }
    }
}