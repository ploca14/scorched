namespace Polyperfect.Crafting.Framework
{
    public class QuantityAndIDSatisfier<T> : ISatisfier<T, Quantity> where T : IValueAndID<Quantity>
    {
        public Quantity SatisfactionWith(T requirements, T supplied)
        {
            if (requirements == null || requirements.Value <= 0)
                return int.MaxValue;
            if (supplied == null || supplied.Value <= 0)
                return default;

            if (requirements.ID != supplied.ID)
                return 0;

            return supplied.Value / requirements.Value;
        }
    }
}