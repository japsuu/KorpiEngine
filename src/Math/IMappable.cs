namespace KorpiEngine;

public interface IMappable<TContainer, TPart>
{
    TContainer Map(Func<TPart, TPart> f);
}