using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using System;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Addon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AddonAttribute : Attribute
    {
        #region Fields

        public readonly string Name;
        public readonly string Description;
        public readonly AddonTarget Target;
        public Type Behaviour;

        #endregion Fields

        #region Methods

        public AddonAttribute(string name, string description, AddonTarget target)
        {
            Name = name;
            Description = description;
            Target = target;
        }

        #endregion Methods
    }
}