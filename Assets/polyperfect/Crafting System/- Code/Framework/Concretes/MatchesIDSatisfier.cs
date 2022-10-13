namespace Polyperfect.Crafting.Framework
{
    public class MatchesIDSatisfier : ISatisfier<RuntimeID, bool>
    {
        public bool SatisfactionWith(RuntimeID requirement, RuntimeID supplied)
        {
            return requirement == supplied;
        }
    }
}