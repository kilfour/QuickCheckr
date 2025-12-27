namespace QuickCheckr;

public class Pool<T>(IEnumerable<T> pool)
{
    public IEnumerable<T> Values => pool;
}