namespace Polyperfect.Crafting.Placement
{
    public interface IPlacementPostprocessor
    {
        void PostprocessPlacement(in PlacementInfo info);
    }
}