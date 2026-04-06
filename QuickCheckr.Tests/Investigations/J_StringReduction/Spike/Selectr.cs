using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction.Spike;

public static class Selectr
{
    public static SelectrOf<ISelectionr> OneOf(params char[] options) =>
        state =>
        {

            if (state.NewRun)
            {
                state.NewRun = false;
                for (int i = state.Index; i < state.Text.Length; i++)
                {
                    if (options.Contains(state.Text[i]))
                    {
                        state.Index = i;
                        break;
                    }
                }
            }
            else
                state.Index++;
            if (options.Contains(state.Text[state.Index]))
            {
                return SelectrResult.Some<ISelectionr>(state,
                    new Selectionr(state, new Slice(state.Index, 1), state.Text.Substring(state.Index, 1)));
            }
            state.RunStopped = true;
            return SelectrResult.Some<ISelectionr>(state, new NullSelectionr());
        };

    public interface ISelectionr
    {
        string Text { get; }

        Func<string, string> Remove();
        Func<string, string> Replace(string replacement);
    }

    public class Selectionr(SelectrState state, Slice slice, string text) : ISelectionr
    {
        public string Text => text;
        public Func<string, string> Remove() =>
            a =>
            {
                state.Index -= slice.Length;
                return slice.Remove(a);
            };

        public Func<string, string> Replace(string replacement) =>
            a =>
            {
                state.Index -= slice.Length;
                state.Index += replacement.Length;
                return slice.Replace(a, replacement);
            };
    }

    public class NullSelectionr() : ISelectionr
    {
        public string Text => string.Empty;
        public Func<string, string> Remove() => a => a;
        public Func<string, string> Replace(string replacement) => a => a;
    }
}
public static class SelectrOld
{
    public static SelectrOf<ISelection> OneOf(params char[] options) =>
        state =>
        {
            if (state.NewRun)
            {
                state.NewRun = false;
                for (int i = 0; i < state.Text.Length; i++)
                {
                    if (options.Contains(state.Text[i]))
                    {
                        state.Index = i;
                        break;
                    }
                }
            }
            if (options.Contains(state.Text[state.Index]))
                return SelectrResult.Some<ISelection>(state, new Slice(state.Index, 1));
            state.RunStopped = true;
            return SelectrResult.Default<ISelection>(state);
        };



    public static SelectrOf<string> Remove(this SelectrOf<ISelection> selection) =>
       state =>
       {
           var slice = selection(state).Value;
           if (slice == null) return SelectrResult.Default<string>(state);
           var result = slice.GetText(state.Text);
           state.Text = selection(state).Value.Remove(state.Text);
           return SelectrResult.Some(state, result);
       };

    public static SelectrOf<string> Replace(this SelectrOf<ISelection> selection, string replacement) =>
       state =>
       {
           var slice = selection(state).Value;
           if (slice == null) return SelectrResult.Default<string>(state);
           state.Text = slice.Replace(state.Text, replacement);
           return SelectrResult.Some(state, state.Text);
       };
}

public static class SelectrRunnerOld
{

    public class SelectrRun(SelectrOf<Selection> selectr, string text)
    {
        private readonly SelectrState state = new(text);
        public string Next()
        {
            state.NewRun = true;
            selectr(state);
            return state.Text;
        }
    }
    public static SelectrRun For(SelectrOf<Selection> selectr, string text)
        => new(selectr, text);
}

public static class SelectrRunner
{

    public class SelectrRun(SelectrOf<Func<string, string>> selectr, string text)
    {
        private readonly SelectrState state = new(text);
        public string Next()
        {
            state.NewRun = true;
            state.RunStopped = false;
            var result = selectr(state);
            if (result.Value == null) return state.Text;
            if (state.RunStopped) return state.Text;
            state.Text = result.Value(state.Text);
            return state.Text;
        }
    }

    public static SelectrRun Run(this SelectrOf<Func<string, string>> selectr, string text)
        => new(selectr, text);
}

public delegate SelectrResult<T> SelectrOf<T>(SelectrState state);

public record SelectrResult<TValue>(SelectrState State, TValue Value, bool Stop = false);

public static class SelectrResult
{
    public static SelectrResult<TValue> Some<TValue>(SelectrState state, TValue value) =>
        new(state, value);

    public static SelectrResult<TValue> Default<TValue>(SelectrState state) =>
        new(state, default!);

    public static SelectrResult<Selection> SelectionOnly(SelectrState state) =>
        Some(state, Selection.Defined);
}

public readonly struct Selection
{
    public static readonly Selection Defined = default;
}

public class SelectrState(string text)
{
    public bool NewRun { get; set; } = true;
    public bool RunStopped { get; set; } = false;
    public int Index { get; set; } = 0;
    public string Text { get; set; } = text;
}

public static class Selectr_ToLinq
{
    public static SelectrOf<TValueTwo> Select<TValueOne, TValueTwo>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, TValueTwo> selector) =>
            state =>
                {
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TValueTwo>(state);
                    return SelectrResult.Some(state, selector(script(state).Value));
                };

    public static SelectrOf<TResult> SelectMany<TSource, TResult>(
        this SelectrOf<TSource> source,
        Func<TSource, SelectrOf<TResult>> selector) =>
            state =>
                {
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TResult>(state);
                    var result = source(state);
                    // if (state.RunStopped)
                    //     return SelectrResult.Default<TResult>(state);
                    return selector(result.Value)(state);
                };

    public static SelectrOf<TValueThree> SelectMany<TValueOne, TValueTwo, TValueThree>(
        this SelectrOf<TValueOne> script,
        Func<TValueOne, SelectrOf<TValueTwo>> selector,
        Func<TValueOne, TValueTwo, TValueThree> projector)
            => script.SelectMany(x => selector(x).Select(y => projector(x, y)));
}