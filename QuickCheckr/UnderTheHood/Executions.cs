// namespace QuickCheckr.UnderTheHood;

// public class Executions
// {
//     private readonly List<int> numbers = [];
//     public IReadOnlyList<int> Numbers => numbers;
//     private int currentExecutionNumber = 0;

//     public int Next()
//     {
//         currentExecutionNumber++;
//         numbers.Add(currentExecutionNumber);
//         return currentExecutionNumber;
//     }

//     public int First()
//     {
//         return numbers[0];
//     }

//     public int NextNumber(int previous)
//     {
//         return numbers.First(a => a > previous);
//     }

//     public int GetMaxNumber()
//     {
//         return numbers.Max();
//     }

//     public int Count()
//     {
//         return numbers.Count;
//     }

//     public void Remove(int executionNumber)
//     {
//         numbers.Remove(executionNumber);
//     }
// }