namespace Polyperfect.Crafting.Integration
{
    public interface IProcessor<T>
    {
        T Process(T input);
    }
}